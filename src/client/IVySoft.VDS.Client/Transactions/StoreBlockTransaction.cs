using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace IVySoft.VDS.Client.Transactions
{
    public class StoreBlockTransaction
    {
        public const int MessageId = 115;
        private readonly byte[] owner_id_;
        private readonly int replica_size_;
        private readonly byte[][] replicas_;
        private readonly byte[] owner_sig_;
        private readonly byte[] object_id_;
        private readonly long object_size_;

        public byte[] owner_id => owner_id_;
        public byte[] object_id => object_id_;
        public Int64 object_size => object_size_; 
        public Int32 replica_size => replica_size_;
        public byte[][] replicas => replicas_;
        public byte[] owner_sig => owner_sig_;
        public StoreBlockTransaction(
                    byte[] object_id,
                    Int64 object_size,
                    Int32 replica_size,
                    byte[][] replicas,
                    RSACryptoServiceProvider user_key)
        {
            this.owner_id_ = Crypto.CryptoUtils.public_key_fingerprint(user_key);
            this.object_id_ = object_id;
            this.object_size_ = object_size;
            this.replica_size_ = replica_size;
            this.replicas_ = replicas;
            
            using (var ms = new System.IO.MemoryStream())
            {
                ms.push_data(this.owner_id);
                ms.push_data(this.object_id);
                ms.push_int64(this.object_size);
                ms.push_int32(this.replica_size);
                ms.write_number(this.replicas.Length);
                foreach (var replica in this.replicas)
                {
                    ms.push_data(replica);
                }

                this.owner_sig_ = user_key.SignData(ms.ToArray(), new SHA256CryptoServiceProvider());
            }

        }

        internal void Serialize(System.IO.Stream ms)
        {
            ms.WriteByte(MessageId);
            ms.push_data(this.owner_id);
            ms.push_data(this.object_id);
            ms.push_int64(this.object_size);
            ms.push_int32(this.replica_size);
            ms.write_number(this.replicas.Length);
            foreach (var replica in this.replicas)
            {
                ms.push_data(replica);
            }
            ms.push_data(this.owner_sig);
        }
        internal byte[] Serialize()
        {
            using (var ms = new System.IO.MemoryStream())
            {
                this.Serialize(ms);
                return ms.ToArray();
            }
        }

    }
}
