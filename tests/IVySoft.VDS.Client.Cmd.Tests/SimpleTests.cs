using System;
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
            Assert.Equal(0, VdsProcess.InitRoot(login, password));

            using(var servers = new VdsProcessSet(10))
            {
                servers.start();

                servers.allocate_storage(login, password, 10);
            }
        }
    }
}
