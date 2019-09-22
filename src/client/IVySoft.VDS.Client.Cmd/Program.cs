using CommandLine;
using System;
using System.Linq;

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

                if (opts.Method == SyncMethod.Both || opts.Method == SyncMethod.Download)
                {
                    foreach(var message in api.GetChannelMessages(channel).Result)
                    {
                        switch (message)
                        {
                            case Transactions.UserMessageTransaction msg:
                                foreach (var f in msg.Files) {
                                    Console.WriteLine($"{f.Name}|{f.MimeType}|{f.Size}");
                                }
                                break;
                        }
                    }
                }

                return 0;
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
