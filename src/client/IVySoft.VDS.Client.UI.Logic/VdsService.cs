using IVySoft.VDS.Client.Transactions.Data;
using System;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace IVySoft.VDS.Client.UI.Logic
{
    public class VdsService : IDisposable
    {
        public static string Server = "localhost:8050";
        private VdsApi api_;

        public VdsApi Api => this.api_;
        public Action<Exception> ErrorHandler { get; set; }

        public VdsService()
        {
            this.api_ = new VdsApi(new VdsApiConfig
            {
                ServiceUri = "ws://" + Server + "/api/ws"
            });
        }

        public void Dispose()
        {
            if (null != this.api_)
            {
                this.api_.Dispose();
                this.api_ = null;
            }
        }
        private static byte[] CalculateHash(string file_name)
        {
            using (var f = System.IO.File.OpenRead(file_name))
            {
                using (var provider = SHA256.Create())
                {
                    return provider.ComputeHash(f);
                }
            }
        }
        public async Task<string> Download(System.Threading.CancellationToken token, Api.ChannelMessageFileInfo file_info, string target_folder)
        {
            foreach (var f in System.IO.Directory.GetFiles(target_folder,
                System.IO.Path.GetFileNameWithoutExtension(file_info.Name)
                + "*"
                + System.IO.Path.GetExtension(file_info.Name)))
            {
                var h = CalculateHash(f);
                if (h.SequenceEqual(file_info.Id))
                {
                    return f;
                }
            }

            var tmp = System.IO.Path.GetTempFileName();
            try
            {
                using (var tmp_file = System.IO.File.Create(tmp))
                {
                    foreach (var file_block in file_info.Blocks)
                    {
                        var result = await this.api_.Download(token, file_block);
                        tmp_file.Write(result, 0, result.Length);
                    }
                }
            }
            catch
            {
                try { System.IO.File.Delete(tmp); } catch { }
                throw;
            }

            for(int i = 0; i < int.MaxValue; ++i)
            {
                var file_name = System.IO.Path.Combine(target_folder,
                    System.IO.Path.GetFileNameWithoutExtension(file_info.Name)
                    + ((0 == i) ? string.Empty : $"({i})")
                    + System.IO.Path.GetExtension(file_info.Name));
                if (!System.IO.File.Exists(file_name))
                {
                    System.IO.File.Copy(tmp, file_name, true);
                    return file_name;
                }
            }

            throw new Exception("Invalid program");
        }
    }
}
