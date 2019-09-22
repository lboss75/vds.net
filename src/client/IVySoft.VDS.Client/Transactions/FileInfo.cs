using System;
using System.Collections.Generic;

namespace IVySoft.VDS.Client.Transactions
{
    public class FileInfo
    {
        private readonly string name_;
        private readonly string mime_type_;
        private readonly long size_;
        private readonly byte[] file_id_;
        private readonly FileBlock[] file_blocks_;

        public FileInfo(string name, string mime_type, long size, byte[] file_id, FileBlock[] file_blocks)
        {
            this.name_ = name;
            this.mime_type_ = mime_type;
            this.size_ = size;
            this.file_id_ = file_id;
            this.file_blocks_ = file_blocks;
        }

        public byte[] Id { get => file_id_; }
        public string Name { get => name_; }
        public string MimeType { get => mime_type_; }
        public long Size { get => size_; }

        internal static FileInfo Deserialize(System.IO.Stream stream)
        {
            var name = stream.get_string();
            var mime_type = stream.get_string();
            var size = stream.get_int64();
            var file_id = stream.pop_data();

            var row_count = stream.read_number();
            var file_blocks = new List<FileBlock>();
            for (var i = 0; i < row_count; ++i)
            {
                file_blocks.Add(FileBlock.Deserialize(stream));
            }

            return new FileInfo(name, mime_type, size, file_id, file_blocks.ToArray());
        }
    }
}