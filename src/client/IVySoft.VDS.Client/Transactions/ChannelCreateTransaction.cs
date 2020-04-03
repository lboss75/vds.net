using System;
using System.IO;

namespace IVySoft.VDS.Client.Transactions
{
    public class ChannelCreateTransaction : ChannelMessage
    {
        public const int MessageId = 110;
        private byte[] channel_id;
        private string channel_type;
        private string channel_name;
        private byte[] read_public_key;
        private byte[] read_private_key;
        private byte[] write_public_key;
        private byte[] write_private_key;

        public ChannelCreateTransaction(
            byte[] channel_id,
            string channel_type,
            string channel_name,
            byte[] read_public_key,
            byte[] read_private_key,
            byte[] write_public_key,
            byte[] write_private_key)
        {
            this.channel_id = channel_id;
            this.channel_type = channel_type;
            this.channel_name = channel_name;
            this.read_public_key = read_public_key;
            this.read_private_key = read_private_key;
            this.write_public_key = write_public_key;
            this.write_private_key = write_private_key;
        }

        public string Id
        {
            get
            {
                return Convert.ToBase64String(this.channel_id);
            }
        }

        public string Type
        {
            get
            {
                return this.channel_type;
            }
        }

        internal KeyPair ReadKey
        {
            get
            {
                return new KeyPair
                {
                    PublicKey = VdsApi.public_key_from_der(this.read_public_key),
                    PrivateKey = VdsApi.private_key_from_der(this.read_private_key)
                };
            }
        }

        internal KeyPair WriteKey
        {
            get
            {
                if(null == this.write_private_key || 0 == this.write_private_key.Length)
                {
                    return null;
                }

                return new KeyPair
                {
                    PublicKey = VdsApi.public_key_from_der(this.write_public_key),
                    PrivateKey = VdsApi.private_key_from_der(this.write_private_key)
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
            ms.push_data(this.channel_id);
            ms.push_string(this.channel_type);
            ms.push_string(this.channel_name);
            ms.push_data(this.read_public_key);
            ms.push_data(this.read_private_key);
            ms.push_data(this.write_public_key);
            ms.push_data(this.write_private_key);
        }
        internal static ChannelMessage Deserialize(Stream stream)
        {
            var channel_id = stream.pop_data();
            var channel_type = stream.get_string();
            var channel_name = stream.get_string();
            var read_public_key = stream.pop_data();
            var read_private_key = stream.pop_data();
            var write_public_key = stream.pop_data();
            var write_private_key = stream.pop_data();

            return new ChannelCreateTransaction(
                channel_id,
                channel_type,
                channel_name,
                read_public_key,
                read_private_key,
                write_public_key,
                write_private_key
            );
        }
    }
}