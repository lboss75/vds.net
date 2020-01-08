using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace IVySoft.VDS.Client.Transactions
{
    public class StoreBlockTransaction
    {
        public const int MessageId = 115;

        public byte[] owner_id { get; set; }
        public byte[] object_id { get; set; }
        Int64 object_size { get; set; }
        public byte[][] replicas { get; set; }
        public byte[] owner_sig { get; set; }
 
        internal byte[] Serialize()
        {
            using (var ms = new System.IO.MemoryStream())
            {
                ms.WriteByte(MessageId);
                ms.push_data(this.owner_id);
                ms.push_data(this.object_id);
                ms.push_int64(this.object_size);
                ms.write_number(this.replicas.Length);
                foreach(var replica in this.replicas)
                {
                    ms.push_data(replica);
                }
                ms.push_data(this.owner_sig);

                return ms.ToArray();
            }
        }

        internal StoreBlockTransaction Sing(RSACryptoServiceProvider user_key)
        {
            this.owner_id = VdsApi.public_key_fingerprint(user_key);
            using (var ms = new System.IO.MemoryStream())
            {
                ms.push_data(this.owner_id);
                ms.push_data(this.object_id);
                ms.push_int64(this.object_size);
                ms.write_number(this.replicas.Length);
                foreach (var replica in this.replicas)
                {
                    ms.push_data(replica);
                }

                this.owner_sig = user_key.SignData(ms.ToArray(), new SHA256CryptoServiceProvider());
            }

            return this;
        }
    }
}
