using System;
using System.Collections.Generic;
using System.Text;

namespace IVySoft.VDS.Client.Transactions
{
    public class CreateWalletMessage : ChannelMessage
    {
        public const int MessageId = 108;

        private readonly string name_;
        private readonly byte[] public_key_;
        private readonly byte[] private_key_;

        public CreateWalletMessage(string name, byte[] public_key, byte[] private_key)
        {
            this.name_ = name;
            this.public_key_ = public_key;
            this.private_key_ = private_key;
        }

        public string Name { get => this.name_; }
        public byte[] PublicKey { get => this.public_key_; }
        public byte[] PrivateKey { get => this.private_key_; }

        internal void Serialize(System.IO.Stream ms)
        {
            ms.WriteByte(MessageId);
            ms.push_string(this.name_);
            ms.push_data(this.public_key_);
            ms.push_data(this.private_key_);
        }

        internal static CreateWalletMessage Deserialize(System.IO.Stream stream)
        {
            var name = stream.get_string();
            var public_key = stream.pop_data();
            var private_key = stream.pop_data();

            return new CreateWalletMessage(name, public_key, private_key);
        }


    }
}
