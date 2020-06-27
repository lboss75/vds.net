using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace IVySoft.VDS.Client.UI.Logic
{
    public class FilesCache
    {
        private string folder_;
        private Dictionary<string, string> files_ = new Dictionary<string, string>();

        public FilesCache(string folder)
        {
            Directory.CreateDirectory(folder);
            this.folder_ = folder;
            foreach (var f in System.IO.Directory.GetFiles(folder))
            {
                var h = VdsService.CalculateHash(f);
                this.files_.Add(Convert.ToBase64String(h), f);
            }
        }

        public string Folder { get => this.folder_; }

        public bool TryGetFile(byte[] id, out string file_name)
        {
            return this.files_.TryGetValue(Convert.ToBase64String(id), out file_name);
        }

        internal void Add(byte[] id, string file_name)
        {
            this.files_.Add(Convert.ToBase64String(id), file_name);
        }
    }
}
