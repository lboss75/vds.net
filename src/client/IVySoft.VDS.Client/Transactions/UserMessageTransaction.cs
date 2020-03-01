using System;
using System.Collections.Generic;
using System.Text;

namespace IVySoft.VDS.Client.Transactions
{
    public class UserMessageTransaction : ChannelMessage
    {
        public const int MessageId = 97;

        private string message_;
        private FileInfo[] files_;

        public UserMessageTransaction(string message, FileInfo[] files)
        {
            this.message_ = message;
            this.files_ = files;
        }

        public string Message { get => this.message_; }
        public IEnumerable<FileInfo> Files { get { return this.files_; } }

        internal static ChannelMessage Deserialize(System.IO.Stream stream)
        {
            var message = stream.get_string();
            var row_count = stream.read_number();
            var files = new List<FileInfo>();
            for (var i = 0; i < row_count; ++i)
            {
                files.Add(FileInfo.Deserialize(stream));
            }

            return new UserMessageTransaction(message, files.ToArray());
        }

        internal byte[] Serialize()
        {
            using(var ms = new System.IO.MemoryStream())
            {
                ms.WriteByte(MessageId);

                ms.push_string(this.message_);
                ms.write_number(this.files_.Length);
                foreach(var f in this.files_)
                {
                    f.Serialize(ms);
                }

                return ms.ToArray();
            }
        }
    }
}
