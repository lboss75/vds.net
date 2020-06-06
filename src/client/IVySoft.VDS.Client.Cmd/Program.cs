using CommandLine;
using IVySoft.VDS.Client.Api;
using IVySoft.VDS.Client.Transactions;
using IVySoft.VDS.Client.Transactions.Data;
using IVySoft.VPlatform.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Threading;

namespace IVySoft.VDS.Client.Cmd
{
    public class Program
    {
        static int Main(string[] args)
        {
            return Parser.Default.ParseArguments<
                ChannelsOptions,
                SyncOptions,
                AllocateStorageOptions,
                GetStorageOptions,
                CreateChannelOptions>(args)
                .MapResult(
                  (CreateUserOptions opts) => RunAddAndReturnExitCode(opts),
                  (ChannelsOptions opts) => RunAddAndReturnExitCode(opts),
                  (CreateChannelOptions opts) => RunAddAndReturnExitCode(opts),
                  (SyncOptions opts) => RunAddAndReturnExitCode(opts),
                  (AllocateStorageOptions opts) => RunAddAndReturnExitCode(opts),
                  (GetStorageOptions opts) => RunAddAndReturnExitCode(opts),
                  errs => 1);

        }

        private static int RunAddAndReturnExitCode(CreateChannelOptions opts)
        {
            using (var source = new System.Threading.CancellationTokenSource(TimeSpan.FromSeconds(opts.Timeout)))
            {
                using (VdsApi api = new VdsApi(new VdsApiConfig
                {
                    ServiceUri = "ws://" + opts.Server + "/api/ws"
                }))
                {
                    var user = api.Login(source.Token, opts.Login, opts.Password).Result;
                    var channel = api.create_channel(source.Token, user, opts.ChannelType, opts.ChannelName).Result;
                    Console.WriteLine(channel.Id);
                }
            }

            return 0;
        }

        private static int RunAddAndReturnExitCode(GetStorageOptions opts)
        {
            using (var source = new System.Threading.CancellationTokenSource(TimeSpan.FromSeconds(opts.Timeout)))
            {
                using (VdsApi api = new VdsApi(new VdsApiConfig
                {
                    ServiceUri = "ws://" + opts.Server + "/api/ws"
                }))
                {
                    var user = api.Login(source.Token, opts.Login, opts.Password).Result;

                    Console.WriteLine("Id|Path|Reserved|Used|Type");
                    foreach (var storage in api.GetStorage(source.Token, user).Result)
                    {
                        Console.WriteLine($"{storage.id}|{storage.local_path}|{storage.reserved_size}|{storage.used_size}|{storage.usage_type}");
                    }
                }
            }

            return 0;
        }

        public static int RunAddAndReturnExitCode(CreateUserOptions opts)
        {
            using (var source = new System.Threading.CancellationTokenSource(TimeSpan.FromSeconds(opts.Timeout)))
            {
                using (VdsApi api = new VdsApi(new VdsApiConfig
                {
                    ServiceUri = "ws://" + opts.Server + "/api/ws"
                }))
                {
                    api.CreateUser(source.Token, opts.Login, opts.Password).Wait();
                }
            }

            return 0;
        }

        public static int RunAddAndReturnExitCode(AllocateStorageOptions opts)
        {
            using (var source = new System.Threading.CancellationTokenSource(TimeSpan.FromSeconds(opts.Timeout)))
            {
                using (VdsApi api = new VdsApi(new VdsApiConfig
                {
                    ServiceUri = "ws://" + opts.Server + "/api/ws"
                }))
                {
                    var user = api.Login(source.Token, opts.Login, opts.Password).Result;
                    api.AllocateStorage(
                        source.Token,
                        user,
                        opts.DestinationPath,
                        HumanReadableFormat.Parse(opts.Length),
                        string.IsNullOrWhiteSpace(opts.UsageType) ? "share" : opts.UsageType).Wait();
                }
            }

            return 0;
        }

        public static int RunAddAndReturnExitCode(SyncOptions opts)
        {
            using (var source = new System.Threading.CancellationTokenSource(TimeSpan.FromSeconds(opts.Timeout)))
            {
                using (VdsApi api = new VdsApi(new VdsApiConfig
                {
                    ServiceUri = "ws://" + opts.Server + "/api/ws"
                }))
                {
                    var user = api.Login(source.Token, opts.Login, opts.Password).Result;
                    var channel = api.GetChannels(source.Token, user).Result
                        .SingleOrDefault(x => x.Id == opts.ChannelId);
                    if (channel == null)
                    {
                        throw new Exception($"Channel {opts.ChannelId} not found");
                    }

                    var storage_files = new Dictionary<string, List<ChannelMessageFileInfo>>();
                    foreach (var message in api.GetChannelMessages(source.Token, channel).Result)
                    {
                        foreach (var f in message.Files)
                        {
                            List<ChannelMessageFileInfo> versions;
                            if (!storage_files.TryGetValue(f.Name, out versions))
                            {
                                versions = new List<ChannelMessageFileInfo>();
                                storage_files.Add(f.Name, versions);
                            }
                            versions.Add(f);
                        }
                    }

                    if (opts.Method == SyncMethod.Both || opts.Method == SyncMethod.Download)
                    {
                        foreach (var f in storage_files)
                        {
                            DownloadFile(source.Token, api, System.IO.Path.Combine(opts.DestinationPath, f.Key.Replace('/', System.IO.Path.DirectorySeparatorChar)), f.Value[0]);
                        }
                    }

                    if (opts.Method == SyncMethod.Both || opts.Method == SyncMethod.Upload)
                    {
                        var to_upload = new List<FileUploadStream>();

                        var exists_files = CollectFiles(opts.DestinationPath);
                        foreach (var f in exists_files)
                        {
                            if (IsNewFile(api, f, storage_files))
                            {
                                f.ProgressCallback = (x => { Console.Write("\r" + f.Name + " " + x + "%"); return true; });
                                to_upload.Add(f);
                            }
                        }

                        while(to_upload.Count > 0)
                        {
                            api.UploadFiles(source.Token, channel, opts.Comment, to_upload.Take(100).ToArray()).Wait();
                            to_upload.RemoveRange(0, 100);
                        }
                    }

                    return 0;
                }
            }
        }

        private static bool IsNewFile(VdsApi api, FileUploadStream f, Dictionary<string, List<ChannelMessageFileInfo>> storage_files)
        {
            List<ChannelMessageFileInfo> storageFiles;
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

        private static void DownloadFile(CancellationToken token, VdsApi api, string file_name, ChannelMessageFileInfo file_info)
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
                        var result = api.Download(token, file_block).Result;
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
        public static Api.Channel[] GetChannels(ChannelsOptions opts)
        {
            using (var source = new System.Threading.CancellationTokenSource(TimeSpan.FromSeconds(opts.Timeout)))
            {
                using (VdsApi api = new VdsApi(new VdsApiConfig
                {
                    ServiceUri = "ws://" + opts.Server + "/api/ws"
                }))
                {
                    var user = api.Login(source.Token, opts.Login, opts.Password).Result;

                    return api.GetChannels(source.Token, user).Result;
                }
            }
        }

        public static int RunAddAndReturnExitCode(ChannelsOptions opts)
        {
            foreach (var channel in GetChannels(opts))
            {
                Console.WriteLine($"{channel.Id}|{channel.Type}|{channel.Name}");
            }

            return 0;
        }
    }
}
