using IVySoft.VDS.Client.Crypto;
using IVySoft.VDS.Client.Transactions;
using System;
using System.Collections.Generic;
using System.Text;

namespace IVySoft.VDS.Client.Api
{
    public class Wallet
    {
        private readonly string id_;
        private readonly string name_;
        private readonly KeyPair key_;

        internal Wallet(CreateWalletMessage msg)
        {
            this.name_ = msg.Name;
            this.key_ = new KeyPair
            {
                PublicKey = Crypto.CryptoUtils.public_key_from_der(msg.PublicKey),
                PrivateKey = Crypto.CryptoUtils.private_key_from_der(msg.PrivateKey)
            };
            this.id_ = Convert.ToBase64String(Crypto.CryptoUtils.public_key_fingerprint(this.key_.PublicKey));
        }

        public string Id { get => this.id_; }
        public string Name { get => this.name_; }
        public KeyPair Key { get => this.key_; }

    }
}
