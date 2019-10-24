using System.IO;

namespace IVySoft.VDS.Client
{
    public class FileUploadStream
    {
        public string Name { get; set; }
        public string SystemPath { get; set; }
        public byte[] FileHash { get; set; }
    }
}