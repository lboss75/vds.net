using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Xunit;
using Xunit.Abstractions;

namespace IVySoft.VDS.Client.Cmd.Tests
{
    public class SimpleTests
    {
        private const string login = "test@test.ru";
        private const string password = "123qwe";

        private readonly ITestOutputHelper output;

        public SimpleTests(ITestOutputHelper output)
        {
            this.output = output;
        }
        private void WriteLine(string line)
        {
            this.output.WriteLine(line);
            Debug.WriteLine("Test: " + line);
        }
        [Fact]
        public void AllocateStorageTest()
        {
            if (Directory.Exists(VdsProcess.RootFolder))
            {
                this.WriteLine($"Delete folder {VdsProcess.RootFolder}");
                Directory.Delete(VdsProcess.RootFolder, true);
            }

            this.WriteLine($"Init root in {VdsProcess.RootFolder}");
            Assert.Equal(0, VdsProcess.InitRoot(login, password));

            using (var servers = new VdsProcessSet(2))
            {
                this.WriteLine($"Starting servers");
                servers.start();
                System.Threading.Thread.Sleep(10000);
                this.WriteLine($"Waiting sync");
                servers.waiting_sync();

                //servers.create_user(4, login, password);

                this.WriteLine($"Allocate storage");
                servers.allocate_storage(login, password, "1G");
                this.WriteLine($"Waiting sync");
                servers.waiting_sync();

                var source_folder = Path.Combine(VdsProcess.RootFolder, "Original");
                var dest_folder_local = Path.Combine(VdsProcess.RootFolder, "DestinationLocal");
                var dest_folder_remote = Path.Combine(VdsProcess.RootFolder, "DestinationRemote");

                Directory.CreateDirectory(source_folder);
                Directory.CreateDirectory(dest_folder_local);
                Directory.CreateDirectory(dest_folder_remote);

                this.WriteLine($"Generate random files");
                const int file_count = 10;
                var rnd = new Random();
                for (int i = 0; i < file_count; ++i)
                {
                    GenerateRandomFile(rnd, Path.Combine(source_folder, i.ToString()));
                }

                this.WriteLine($"Create channel");
                Assert.Equal(0 , servers.create_channel(1, login, password, IVySoft.VDS.Client.Api.ChannelTypes.file_channel, "test"));

                string channel_id = null;
                this.WriteLine($"Looking channels");
                foreach (var channel in servers.GetChannels(login, password, 1))
                {
                    channel_id = channel.Id;
                    if (!string.IsNullOrEmpty(channel_id))
                    {
                        this.WriteLine($"Found channel {channel_id}");
                        break;
                    }
                }

                Assert.True(!string.IsNullOrEmpty(channel_id));

                this.WriteLine($"Sync local files");
                servers.sync_files(login, password, channel_id, 1, source_folder);
                servers.sync_files(login, password, channel_id, 1, dest_folder_local);
                for (int i = 0; i < file_count; ++i)
                {
                    CompareFile(Path.Combine(source_folder, i.ToString()), Path.Combine(dest_folder_local, i.ToString()));
                }
                this.WriteLine($"Waiting sync");
                servers.waiting_sync();

                this.WriteLine($"Sync remote files");
                servers.sync_files(login, password, channel_id, 0, dest_folder_remote);
                this.WriteLine($"Compare files");
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
