using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace IVySoft.VDS.Client.Transactions
{
    internal class CreateWalletTransaction : ChannelMessage
    {
        public const int MessageId = 119;

        private readonly byte[] id_;
        private readonly byte[] public_key_;

        public CreateWalletTransaction(byte[] id, byte[] public_key)
        {
            this.id_ = id;
            this.public_key_ = public_key;
        }

        internal void Serialize(System.IO.Stream ms)
        {
            ms.WriteByte(MessageId);
            ms.push_data(this.id_);
            ms.push_data(this.public_key_);
        }

        internal static CreateWalletTransaction Deserialize(Stream stream)
        {
            var id = stream.pop_data();
            var public_key = stream.pop_data();

            return new CreateWalletTransaction(id, public_key);
        }
    }
}