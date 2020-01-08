using System;
using System.Collections.Generic;

namespace IVySoft.VDS.Client.Cmd.Tests
{
    internal class VdsProcessSet : IDisposable
    {
        private readonly VdsProcess[] servers_;

        public VdsProcessSet(int count)
        {
            this.servers_ = new VdsProcess[count];
        }

        public VdsProcess this[int index]
        {
            get
            {
                return this.servers_[index];
            }
        }

        public void start()
        {
            for(int i = 0; i < this.servers_.Length; ++i)
            {
                this.servers_[i] = new VdsProcess(i);
            }
        }

        internal int create_user(int server_index, string login, string password)
        {
            return Program.RunAddAndReturnExitCode(new CreateUserOptions
            {
                Login = login,
                Password = password,
                Server = $"localhost:{8050 + server_index}"
            });
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

        internal ChannelMessage[] GetChannels(string login, string password, int server_index)
        {
            return Program.GetChannels(new ChannelsOptions
            {
                Login = login,
                Password = password,
                Server = $"localhost:{8050 + server_index}"
            });
        }

        internal void sync_files(string login, string password, string channel_id, int server_index, string folder)
        {
            var code = Program.RunAddAndReturnExitCode(new SyncOptions
            {
                Login = login,
                Password = password,
                Server = $"localhost:{8050 + server_index}",
                DestinationPath = folder,
                Method = SyncMethod.Both,
                ChannelId = channel_id
            });

            if (0 != code)
            {
                throw new Exception($"Sync files {folder} failed with code {code}");
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