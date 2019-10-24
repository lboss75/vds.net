using CommandLine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;

namespace IVySoft.VDS.Client.Cmd
{
    public class Program
    {
        static int Main(string[] args)
        {
            return Parser.Default.ParseArguments<ChannelsOptions, SyncOptions, AllocateStorageOptions>(args)
                .MapResult(
                  (ChannelsOptions opts) => RunAddAndReturnExitCode(opts),
                  (SyncOptions opts) => RunAddAndReturnExitCode(opts),
                  (AllocateStorageOptions opts) => RunAddAndReturnExitCode(opts),
                  errs => 1);

        }

        public static int RunAddAndReturnExitCode(AllocateStorageOptions opts)
        {
            using (VdsApi api = new VdsApi(new VdsApiConfig
            {
                ServiceUri = "ws://" + opts.Server + "/api/ws"
            }))
            {
                api.Login(opts.Login, opts.Password).Wait();
                api.AllocateStorage(opts.DestinationPath, opts.Length).Wait();
            }

            return 0;
        }

        public static int RunAddAndReturnExitCode(SyncOptions opts)
        {
            using (VdsApi api = new VdsApi(new VdsApiConfig
            {
                ServiceUri = "ws://" + opts.Server + "/api/ws"
            }))
            {
                api.Login(opts.Login, opts.Password).Wait();
                var channel = api.GetChannels().Result
                    .Where(x => x is Transactions.ChannelCreateTransaction)
                    .Select(x => (Transactions.ChannelCreateTransaction)x)
                    .SingleOrDefault(x => x.Id == opts.ChannelId);
                if(channel == null)
                {
                    throw new Exception($"Channel {opts.ChannelId} not found");
                }

                var storage_files = new Dictionary<string, List<Transactions.FileInfo>>();
                foreach (var message in api.GetChannelMessages(channel).Result)
                {
                    switch (message)
                    {
                        case Transactions.UserMessageTransaction msg:
                            foreach (var f in msg.Files)
                            {
                                List<Transactions.FileInfo> versions;
                                if (!storage_files.TryGetValue(f.Name, out versions))
                                {
                                    versions = new List<Transactions.FileInfo>();
                                    storage_files.Add(f.Name, versions);
                                }
                                versions.Add(f);
                            }
                            break;
                    }
                }

                if (opts.Method == SyncMethod.Both || opts.Method == SyncMethod.Download)
                {
                    foreach (var f in storage_files)
                    {
                        DownloadFile(api, System.IO.Path.Combine(opts.DestinationPath, f.Key.Replace('/', System.IO.Path.DirectorySeparatorChar)), f.Value[0]);
                    }
                }

                if (opts.Method == SyncMethod.Both || opts.Method == SyncMethod.Upload)
                {
                    var to_upload = new List<FileUploadStream>();

                    var exists_files = CollectFiles(opts.DestinationPath);
                    foreach (var f in exists_files)
                    {
                        if(IsNewFile(api, f, storage_files))
                        {
                            to_upload.Add(f);
                        }
                    }

                    if (to_upload.Count > 0)
                    {
                        api.UploadFiles(opts.ChannelId, opts.Comment, to_upload.ToArray()).Wait();
                    }
                }

                return 0;
            }
        }

        private static bool IsNewFile(VdsApi api, FileUploadStream f, Dictionary<string, List<Transactions.FileInfo>> storage_files)
        {
            List<Transactions.FileInfo> storageFiles;
            storage_files.TryGetValue(f.Name, out storageFiles);


            var h = CalculateHash(f.SystemPath);

            if (storageFiles != null && storageFiles.Count > 0 && h.SequenceEqual(storageFiles[0].Id))
            {
                return false;
            }

            f.FileHash = h;
            return true;
        }

        private static List<FileUploadStream> CollectFiles(string destinationPath)
        {
            var result = new List<FileUploadStream>();
            CollectFiles(result, destinationPath, string.Empty);
            return result;
        }

        private static void CollectFiles(List<FileUploadStream> result, string systemPath, string relativePath)
        {
            foreach(var f in System.IO.Directory.GetFiles(systemPath))
            {
                result.Add(new FileUploadStream { Name = (string.IsNullOrEmpty(relativePath) ? string.Empty : (relativePath + "/")) + System.IO.Path.GetFileName(f), SystemPath = f });
            }

            foreach (var f in System.IO.Directory.GetDirectories(systemPath))
            {
                CollectFiles(result, f, (string.IsNullOrEmpty(relativePath) ? string.Empty : (relativePath + "/")) + System.IO.Path.GetFileName(f));
            }
        }

        private static void DownloadFile(VdsApi api, string file_name, Transactions.FileInfo file_info)
        {
            if (System.IO.File.Exists(file_name))
            {
                var h = CalculateHash(file_name);
                if (h.SequenceEqual(file_info.Id))
                {
                    return;
                }
            }
            else
            {
                Console.WriteLine($"Downloading new file {file_name}");
            }

            var tmp = System.IO.Path.GetTempFileName();
            try
            {
                using (var tmp_file = System.IO.File.Create(tmp))
                {
                    foreach (var file_block in file_info.Blocks)
                    {
                        var result = api.Dawnload(file_block).Result;
                        tmp_file.Write(result, 0, result.Length);
                    }
                }
            }
            catch
            {
                try { System.IO.File.Delete(tmp); } catch { }
                throw;
            }

            System.IO.File.Copy(tmp, file_name, true);
        }

        private static byte[] CalculateHash(string file_name)
        {
            using(var f = System.IO.File.OpenRead(file_name))
            {
                using (var provider = SHA256.Create())
                {
                    return provider.ComputeHash(f);
                }
            }
        }
        public static ChannelMessage[] GetChannels(ChannelsOptions opts)
        {
            using (VdsApi api = new VdsApi(new VdsApiConfig
            {
                ServiceUri = "ws://" + opts.Server + "/api/ws"
            }))
            {
                api.Login(opts.Login, opts.Password).Wait();

                return api.GetChannels().Result;
            }
        }

        public static int RunAddAndReturnExitCode(ChannelsOptions opts)
        {
            foreach (var message in GetChannels(opts))
            {
                switch (message)
                {
                    case Transactions.ChannelCreateTransaction msg:
                        Console.WriteLine($"{msg.Id}|{msg.Type}|{msg.Name}");
                        break;
                }
            }

            return 0;
        }
    }
}
