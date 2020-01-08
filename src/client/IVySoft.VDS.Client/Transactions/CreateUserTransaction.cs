using System;
using System.Collections.Generic;
using System.Text;

namespace IVySoft.VDS.Client.Transactions
{
    public class CreateUserTransaction
    {
        public const int MessageId = 117;

        public string user_email { get; set; }
        public byte[] user_public_key { get; set; }
        public byte[] user_profile_id { get; set; }
        public string user_name { get; set; }

        internal byte[] Serialize()
        {
            using (var ms = new System.IO.MemoryStream())
            {
                ms.WriteByte(MessageId);
                ms.push_string(this.user_email);
                ms.push_data(this.user_public_key);
                ms.push_data(this.user_profile_id);
                ms.push_string(this.user_name);

                return ms.ToArray();
            }
        }

    }
}
