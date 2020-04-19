using IVySoft.VDS.Client.Crypto;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace IVySoft.VDS.Client.Api
{
    public class ThisUser
    {
        private readonly string user_id_;
        private readonly Channel personal_channel_;

        public string Id { get => this.user_id_; }
        public Channel PersonalChannel { get => this.personal_channel_; }

        internal ThisUser(RSACryptoServiceProvider public_key, RSACryptoServiceProvider private_key)
        {
            this.user_id_ = Convert.ToBase64String(Crypto.CryptoUtils.public_key_fingerprint(public_key));
            var key = new KeyPair { PublicKey = public_key, PrivateKey = private_key };
            this.personal_channel_ = new Channel(this.user_id_, ChannelTypes.personal_channel, string.Empty, key, key);
        }


    }
}
