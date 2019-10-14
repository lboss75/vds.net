using System;

namespace IVySoft.VDS.Client.Cmd.Tests
{
    internal class VdsProcessSet : IDisposable
    {
        private readonly VdsProcess[] servers_;

        public VdsProcessSet(int count)
        {
            this.servers_ = new VdsProcess[count];
        }

        public void start()
        {
            for(int i = 0; i < this.servers_.Length; ++i)
            {
                this.servers_[i] = new VdsProcess(i);
            }
        }

        public void allocate_storage(string login, string password, long size)
        {
            for (int i = 0; i < this.servers_.Length; ++i)
            {
                for (int j = 0; ; ++j)
                {
                    try
                    {
                        var code = Program.RunAddAndReturnExitCode(new AllocateStorageOptions
                        {
                            Login = login,
                            Password = password,
                            Server = $"localhost:{8050 + i}",
                            DestinationPath = System.IO.Path.Combine(this.servers_[i].ServerRoot, "storage"),
                            Length = size
                        });

                        if (0 == code)
                        {
                            break;
                        }

                        throw new Exception($"Allocate storage failed with code {code}");
                    }
                    catch (Exception ex)
                    {
                        if (ex.InnerException != null && ex.InnerException.Message == "User not found" && j < 100)
                        {
                            System.Threading.Thread.Sleep(60 * 1000);
                            continue;
                        }

                        throw;
                    }
                }
            }
        }

        public void Dispose()
        {
            foreach (var server in this.servers_)
            {
                if (null != server)
                {
                    server.Terminate();
                }
            }
        }
    }
}