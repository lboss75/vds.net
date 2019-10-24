using System;
using System.IO;
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

            using(var servers = new VdsProcessSet(10))
            {
                servers.start();

                servers.allocate_storage(login, password, 10);

                var source_folder = Path.Combine(VdsProcess.RootFolder, "Original");
                var dest_folder = Path.Combine(VdsProcess.RootFolder, "Destination");

                Directory.CreateDirectory(source_folder);
                Directory.CreateDirectory(dest_folder);

                var rnd = new Random();
                for (int i = 0; i < 10; ++i)
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
                servers.sync_files(login, password, channel_id, 7, dest_folder);
            }
        }

        private void GenerateRandomFile(Random rnd, string filePath)
        {
            var size = rnd.Next();
            while (size < 1 || size > 100000000)
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
