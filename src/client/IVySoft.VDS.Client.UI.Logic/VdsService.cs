﻿using IVySoft.VDS.Client.Transactions;
using System;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace IVySoft.VDS.Client.UI.Logic
{
    public class VdsService
    {
        public static string Server = "localhost:8050";
        public static VdsService Instance = new VdsService();

        private VdsApi api_;

        public event LoginRequiredDelegate OnLoginRequired;

        public VdsApi Api => this.api_;
        public Action<Exception> ErrorHandler { get; set; }

        public void OpenConnection()
        {
            this.api_ = new VdsApi(new VdsApiConfig
            {
                ServiceUri = "ws://" + Server + "/api/ws"
            });

            this.api_.ErrorHandler = this.Client_ErrorHandler;

            if(this.OnLoginRequired != null)
            {
                this.OnLoginRequired(this, new LoginRequiredEventArg());
            }
        }

        private void Client_ErrorHandler(Exception ex)
        {
            this.api_ = new VdsApi(new VdsApiConfig
            {
                ServiceUri = "ws://" + Server + "/api/ws"
            });

            this.api_.ErrorHandler = this.Client_ErrorHandler;
            this.ErrorHandler(ex);

        }

        public void Stop()
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
        public async Task<string> Dawnload(FileInfo file_info, string target_folder)
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
                        var result = await this.api_.Dawnload(file_block);
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
