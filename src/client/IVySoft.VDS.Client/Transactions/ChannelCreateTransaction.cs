using System;
using System.IO;

namespace IVySoft.VDS.Client.Transactions
{
    public class ChannelCreateTransaction : ChannelMessage
    {
        public const int MessageId = 110;
        private string channel_type;
        private string channel_name;
        private byte[] read_public_key;
        private byte[] read_private_key;
        private byte[] write_public_key;
        private byte[] write_private_key;
        private byte[] admin_public_key;
        private byte[] admin_private_key;

        public ChannelCreateTransaction(
            string channel_type,
            string channel_name,
            byte[] read_public_key,
            byte[] read_private_key,
            byte[] write_public_key,
            byte[] write_private_key,
            byte[] admin_public_key,
            byte[] admin_private_key)
        {
            this.channel_type = channel_type;
            this.channel_name = channel_name;
            this.read_public_key = read_public_key;
            this.read_private_key = read_private_key;
            this.write_public_key = write_public_key;
            this.write_private_key = write_private_key;
            this.admin_public_key = admin_public_key;
            this.admin_private_key = admin_private_key;
        }


        public string Type
        {
            get
            {
                return this.channel_type;
            }
        }

        public KeyPair ReadKey
        {
            get
            {
                return new KeyPair
                {
                    PublicKey = Crypto.CryptoUtils.public_key_from_der(this.read_public_key),
                    PrivateKey = Crypto.CryptoUtils.private_key_from_der(this.read_private_key)
                };
            }
        }

        public KeyPair WriteKey
        {
            get
            {
                return new KeyPair
                {
                    PublicKey = Crypto.CryptoUtils.public_key_from_der(this.write_public_key),
                    PrivateKey = Crypto.CryptoUtils.private_key_from_der(this.write_private_key)
                };
            }
        }
        public KeyPair AdminKey
        {
            get
            {
                return new KeyPair
                {
                    PublicKey = Crypto.CryptoUtils.public_key_from_der(this.admin_public_key),
                    PrivateKey = Crypto.CryptoUtils.private_key_from_der(this.admin_private_key)
                };
            }
        }


        public string Name
        {
            get
            {
                return this.channel_name;
            }
        }

        internal void Serialize(System.IO.Stream ms)
        {
            ms.WriteByte(MessageId);
            ms.push_string(this.channel_type);
            ms.push_string(this.channel_name);
            ms.push_data(this.read_public_key);
            ms.push_data(this.read_private_key);
            ms.push_data(this.write_public_key);
            ms.push_data(this.write_private_key);
            ms.push_data(this.admin_public_key);
            ms.push_data(this.admin_private_key);
        }
        internal static ChannelMessage Deserialize(Stream stream)
        {
            var channel_type = stream.get_string();
            var channel_name = stream.get_string();
            var read_public_key = stream.pop_data();
            var read_private_key = stream.pop_data();
            var write_public_key = stream.pop_data();
            var write_private_key = stream.pop_data();
            var admin_public_key = stream.pop_data();
            var admin_private_key = stream.pop_data();

            return new ChannelCreateTransaction(
                channel_type,
                channel_name,
                read_public_key,
                read_private_key,
                write_public_key,
                write_private_key,
                admin_public_key,
                admin_private_key
            );
        }
    }
}