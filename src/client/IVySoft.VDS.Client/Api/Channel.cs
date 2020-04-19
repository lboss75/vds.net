using IVySoft.VDS.Client.Crypto;
using IVySoft.VDS.Client.Transactions;
using IVySoft.VDS.Client.Transactions.Data;
using Org.BouncyCastle.Security;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace IVySoft.VDS.Client.Api
{
    public class Channel
    {
        private readonly string id_;
        private readonly string type_;
        private readonly string name_;

        private readonly KeyPair read_key_;
        private readonly KeyPair write_key_;
        private readonly KeyPair admin_key_;

        private readonly Dictionary<string, KeyPair> read_keys_ = new Dictionary<string, KeyPair>();
        private readonly Dictionary<string, KeyPair> write_keys_ = new Dictionary<string, KeyPair>();

        internal Channel(ChannelCreateTransaction t)
        {
            this.id_ = Convert.ToBase64String(Crypto.CryptoUtils.public_key_fingerprint(t.AdminKey.PublicKey));
            this.type_ = t.Type;
            this.name_ = t.Name;
            this.read_key_ = t.ReadKey;
            this.write_key_ = t.WriteKey;

            this.add_read_key(this.read_key_);
            if (null != this.write_key_)
            {
                this.add_write_key(this.write_key_);
            }
        }

        internal Channel(string id, string type, string name, KeyPair read_key, KeyPair write_key)
        {
            this.id_ = id;
            this.type_ = type;
            this.name_ = name;
            this.read_key_ = read_key;
            this.write_key_ = write_key;

            this.add_read_key(read_key);
            if(null != write_key)
            {
                this.add_write_key(write_key);
            }            
        }

        public string Id { get => this.id_; }
        public string Type { get => this.type_; }
        public string Name { get => this.name_; }
        internal KeyPair WriteKey { get => this.write_key_; }

        internal void add_read_key(KeyPair read_key)
        {
            this.read_keys_.Add(Convert.ToBase64String(Crypto.CryptoUtils.public_key_fingerprint(read_key.PublicKey)), read_key);
        }

        internal void add_write_key(KeyPair write_key)
        {
            this.write_keys_.Add(Convert.ToBase64String(Crypto.CryptoUtils.public_key_fingerprint(write_key.PublicKey)), write_key);
        }

        internal byte[] channel_encrypt(byte[] data)
        {
            if (null == this.write_key_)
            {
                throw new Exception("Channel is read only");
            }

            SecureRandom random = new SecureRandom();

            var key = new byte[32];
            random.NextBytes(key);

            var iv = new byte[16];
            random.NextBytes(iv);

            var crypted_data = Crypto.CryptoUtils.encrypt_by_aes_256_cbc(key, iv, data);

            byte[] crypted_key;
            using (var ms = new System.IO.MemoryStream())
            {
                ms.push_data(key);
                ms.push_data(iv);

                crypted_key = this.read_key_.PublicKey.Encrypt(ms.ToArray(), false);
            }

            using (var ms = new System.IO.MemoryStream())
            {
                ms.WriteByte(99);
                ms.push_data(Convert.FromBase64String(this.id_));
                ms.push_data(Crypto.CryptoUtils.public_key_fingerprint(this.read_key_.PublicKey));
                ms.push_data(Crypto.CryptoUtils.public_key_fingerprint(this.write_key_.PublicKey));
                ms.push_data(crypted_key);
                ms.push_data(crypted_data);

                var signature = this.write_key_.PrivateKey.SignData(ms.ToArray(), new SHA256CryptoServiceProvider());
                ms.push_data(signature);

                return ms.ToArray();
            }
        }
        internal Transactions.ChannelMessage decrypt(CryptedChannelMessage message)
        {
            var read_keys = this.read_keys_[message.read_id];
            return decrypt(read_keys, message);
        }
        private Transactions.ChannelMessage decrypt(KeyPair read_keys, CryptedChannelMessage message)
        {
            var key_data = Crypto.CryptoUtils.decrypt_by_private_key(read_keys.PrivateKey, Convert.FromBase64String(message.crypted_key));


            byte[] key;
            byte[] iv; ;
            using (var stream = new System.IO.MemoryStream(key_data))
            {
                key = stream.pop_data();
                iv = stream.pop_data();
            }

            var data = Crypto.CryptoUtils.decrypt_by_aes_256_cbc(key, iv, Convert.FromBase64String(message.crypted_data));

            using (var stream = new System.IO.MemoryStream(data))
            {
                var message_id = stream.ReadByte();
                switch (message_id)
                {
                    //channel_create_transaction
                    case Transactions.ChannelCreateTransaction.MessageId:
                        {
                            return Transactions.ChannelCreateTransaction.Deserialize(stream);
                        }
                    case Transactions.UserMessageTransaction.MessageId:
                        {
                            return Transactions.UserMessageTransaction.Deserialize(stream);
                        }
                    case Transactions.CreateWalletMessage.MessageId:
                        {
                            return Transactions.CreateWalletMessage.Deserialize(stream);
                        }
                }

                throw new Exception($"Invalid message {message_id}");
            }
        }
    }
}
