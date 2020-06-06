using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;

namespace IVySoft.VDS.Client.Cmd.Tests
{
    class VdsProcess : IDisposable
    {
        private readonly string server_root_;
        private readonly Process process_;

        public VdsProcess(int index)
        {
            this.server_root_ = Path.Combine(RootFolder, index.ToString());
            this.process_ = Process.Start(
                Path.Combine(Environment.CurrentDirectory, "vds_ws_server"),
                $"server start --root-folder {this.server_root_} -dev -P {8050 + index} -ll trace -lm dht_sync,dht,dht_session");

        }
        public static string RootFolder
        {
            get
            {
                return Path.Combine(Environment.CurrentDirectory, "servers");
            }
        }

        public string ServerRoot
        {
            get
            {
                return this.server_root_;
            }
        }

        public static int InitRoot(string login, string password)
        {
            using (var process = Process.Start(
                Path.Combine(Environment.CurrentDirectory, "vds_background"),
                $"server root -l {login} -p {password} --root-folder {Path.Combine(RootFolder, "0")} -dev"))
            {
                process.WaitForExit();
                return process.ExitCode;
            }
        }

        internal void Sync(string server, HashSet<string> items, ref bool bContinue)
        {
            using (var source = new CancellationTokenSource(TimeSpan.FromSeconds(60)))
            {
                using (VdsApi api = new VdsApi(new VdsApiConfig
                {
                    ServiceUri = "ws://" + server + "/api/ws"
                }))
                {
                    var current = api.get_sync_state(source.Token).Result;
                    foreach (var item in current)
                    {
                        if (!items.Contains(item))
                        {
                            items.Add(item);
                            bContinue = true;
                        }
                    }

                    foreach (var item in items)
                    {
                        bool bFound = false;
                        foreach (var citem in current)
                        {
                            if (item == citem)
                            {
                                bFound = true;
                                break;
                            }
                        }
                        if (!bFound)
                        {
                            bContinue = true;
                        }
                    }
                }
            }
        }

        internal void Terminate()
        {
            if (null != this.process_)
            {
                try
                {
                    if (!this.process_.HasExited)
                    {
                        this.process_.Kill();
                    }
                }
                catch { }
            }
        }

        public void Dispose()
        {
            if(null != this.process_)
            {
                this.process_.Dispose();
            }
        }
    }
}
