using System;
using System.Collections.Generic;
using System.IO;

namespace IVySoft.VDS.Client.Transactions.Data
{
    internal class FileInfo
    {
        private readonly string name_;
        private readonly string mime_type_;
        private readonly long size_;
        private readonly byte[] file_id_;
        private readonly FileBlock[] file_blocks_;

        public FileInfo(string name, string mime_type, long size, byte[] file_id, FileBlock[] file_blocks)
        {
            name_ = name;
            mime_type_ = mime_type;
            size_ = size;
            file_id_ = file_id;
            file_blocks_ = file_blocks;
        }

        public byte[] Id { get => file_id_; }
        public string Name { get => name_; }
        public string MimeType { get => mime_type_; }
        public long Size { get => size_; }
        public IEnumerable<FileBlock> Blocks { get => file_blocks_; }

        internal static FileInfo Deserialize(Stream stream)
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

        internal void Serialize(Stream stream)
        {
            stream.push_string(name_);
            stream.push_string(mime_type_);
            stream.push_int64(size_);
            stream.push_data(file_id_);

            stream.write_number(file_blocks_.Length);
            foreach (var block in file_blocks_)
            {
                block.Serialize(stream);
            }
        }
    }
}