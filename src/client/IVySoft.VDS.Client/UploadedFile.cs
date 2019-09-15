using System.Collections.Generic;

namespace IVySoft.VDS.Client
{
    internal class UploadedFile
    {
        public string file_name { get; set; }
        public string mime_type { get; set; }
        public long file_size { get; set; }
        public byte[] file_id { get; set; }
        public FileBlock[] file_blocks { get; set; }
    }
}