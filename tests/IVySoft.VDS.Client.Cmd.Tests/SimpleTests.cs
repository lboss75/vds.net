using System;
using System.IO;
using System.Linq;
using Xunit;

namespace IVySoft.VDS.Client.Cmd.Tests
{
    public class SimpleTests
    {
        private const string login = "test@test.ru";
        private const string password = "123qwe";

        [Fact]
        public void AllocateStorageTest()
        {
            if (Directory.Exists(VdsProcess.RootFolder))
            {
                Directory.Delete(VdsProcess.RootFolder, true);
            }

            Assert.Equal(0, VdsProcess.InitRoot(login, password));

            using (var servers = new VdsProcessSet(10))
            {
                servers.start();

                //servers.create_user(4, login, password);

                servers.allocate_storage(login, password, 10);

                var source_folder = Path.Combine(VdsProcess.RootFolder, "Original");
                var dest_folder_local = Path.Combine(VdsProcess.RootFolder, "DestinationLocal");
                var dest_folder_remote = Path.Combine(VdsProcess.RootFolder, "DestinationRemote");

                Directory.CreateDirectory(source_folder);
                Directory.CreateDirectory(dest_folder_local);
                Directory.CreateDirectory(dest_folder_remote);

                const int file_count = 2;
                var rnd = new Random();
                for (int i = 0; i < file_count; ++i)
                {
                    GenerateRandomFile(rnd, Path.Combine(source_folder, i.ToString()));
                }

                string channel_id = null;
                foreach (var message in servers.GetChannels(login, password, 5))
                {
                    switch (message)
                    {
                        case Transactions.ChannelCreateTransaction msg:
                            channel_id = msg.Id;
                            break;
                    }
                    if (!string.IsNullOrEmpty(channel_id))
                    {
                        break;
                    }
                }

                Assert.True(!string.IsNullOrEmpty(channel_id));


                servers.sync_files(login, password, channel_id, 5, source_folder);
                servers.sync_files(login, password, channel_id, 5, dest_folder_local);
                for (int i = 0; i < file_count; ++i)
                {
                    CompareFile(Path.Combine(source_folder, i.ToString()), Path.Combine(dest_folder_local, i.ToString()));
                }

                servers.sync_files(login, password, channel_id, 7, dest_folder_remote);
                for (int i = 0; i < file_count; ++i)
                {
                    CompareFile(Path.Combine(source_folder, i.ToString()), Path.Combine(dest_folder_remote, i.ToString()));
                }

            }
        }

        private void CompareFile(string source_file, string target_file)
        {
            var buffer1 = new byte[1024];
            var buffer2 = new byte[1024];

            using (var f1 = File.OpenRead(source_file))
            {
                using (var f2 = File.OpenRead(target_file))
                {
                    for(; ; )
                    {
                        var readed1 = f1.Read(buffer1, 0, buffer1.Length);
                        var readed2 = f2.Read(buffer2, 0, buffer2.Length);

                        Assert.Equal(readed1, readed2);

                        if(readed1 == 0)
                        {
                            break;
                        }

                        Assert.True(buffer1.SequenceEqual(buffer2));
                    }
                }
            }
        }

        private void GenerateRandomFile(Random rnd, string filePath)
        {
            var size = rnd.Next();
            while (size < 1 || size > 10000)
            {
                size = rnd.Next();
            }

            using (var f = new FileStream(filePath, FileMode.OpenOrCreate, FileAccess.Write, FileShare.Read))
            {
                while(0 < size)
                {
                    f.WriteByte((byte)rnd.Next());
                    --size;
                }

                f.Flush();
            }
        }
    }
}
