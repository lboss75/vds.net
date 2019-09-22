using CommandLine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;

namespace IVySoft.VDS.Client.Cmd
{
    class Program
    {
        static int Main(string[] args)
        {
            return Parser.Default.ParseArguments<ChannelsOptions, SyncOptions>(args)
                .MapResult(
                  (ChannelsOptions opts) => RunAddAndReturnExitCode(opts),
                  (SyncOptions opts) => RunAddAndReturnExitCode(opts),
                  errs => 1);

        }

        private static int RunAddAndReturnExitCode(SyncOptions opts)
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

                var exists_files = new Dictionary<string, List<Transactions.FileInfo>>();
                foreach (var message in api.GetChannelMessages(channel).Result)
                {
                    switch (message)
                    {
                        case Transactions.UserMessageTransaction msg:
                            foreach (var f in msg.Files)
                            {
                                List<Transactions.FileInfo> versions;
                                if (!exists_files.TryGetValue(f.Name, out versions))
                                {
                                    versions = new List<Transactions.FileInfo>();
                                    exists_files.Add(f.Name, versions);
                                }
                                versions.Add(f);
                            }
                            break;
                    }
                }

                if (opts.Method == SyncMethod.Both || opts.Method == SyncMethod.Download)
                {
                    foreach (var f in exists_files)
                    {
                        DownloadFile(api, System.IO.Path.Combine(opts.DestinationPath, f.Key), f.Value[0]);
                    }
                }

                return 0;
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
            catch(Exception ex)
            {
                try { System.IO.File.Delete(tmp); } catch (Exception e) { }
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

        private static int RunAddAndReturnExitCode(ChannelsOptions opts)
        {
            using (VdsApi api = new VdsApi(new VdsApiConfig
            {
                ServiceUri = "ws://" + opts.Server + "/api/ws"
            }))
            {
                api.Login(opts.Login, opts.Password).Wait();

                foreach (var message in api.GetChannels().Result)
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
}
