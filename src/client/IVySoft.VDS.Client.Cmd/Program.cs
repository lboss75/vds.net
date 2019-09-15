using CommandLine;
using System;

namespace IVySoft.VDS.Client.Cmd
{
    class Program
    {
        static int Main(string[] args)
        {
            return Parser.Default.ParseArguments<ChannelsOptions, UploadOptions>(args)
                .MapResult(
                  (ChannelsOptions opts) => RunAddAndReturnExitCode(opts),
                  (UploadOptions opts) => RunAddAndReturnExitCode(opts),
                  errs => 1);

        }

        private static int RunAddAndReturnExitCode(UploadOptions opts)
        {
            using (VdsApi api = new VdsApi(new VdsApiConfig
            {
                ServiceUri = "ws://" + opts.Server + "/api/ws"
            }))
            {
                api.Login(opts.Login, opts.Password).Wait();

                for(int i = 0; i < opts.FilePath.Length; ++i)
                {
                    var file_path = opts.FilePath[i];
                    var file_name = (null != opts.FileName && i < opts.FileName.Length) ? opts.FileName[i] : System.IO.Path.GetFileName(file_path);

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
                        case ChannelCreateTransaction msg:
                            Console.WriteLine($"{msg.Id}|{msg.Type}|{msg.Name}");
                            break;
                    }
                }

                return 0;
            }
        }
    }
}
