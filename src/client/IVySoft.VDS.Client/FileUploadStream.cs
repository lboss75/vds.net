using System;

namespace IVySoft.VDS.Client
{
    public class FileUploadStream
    {
        public string Name { get; set; }
        public string SystemPath { get; set; }
        public byte[] FileHash { get; set; }
        public Func<int, bool> ProgressCallback { get; set; }
    }
}