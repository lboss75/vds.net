using System;
using System.Collections.Generic;
using System.Text;

namespace IVySoft.VDS.Client.Transactions
{
    public class UserProfile
    {
        public byte[] password_hash { get; set; }
        public byte[] user_private_key { get; set; }

        internal byte[] Serialize()
        {
            using (var ms = new System.IO.MemoryStream())
            {
                ms.push_data(this.password_hash);
                ms.push_data(this.user_private_key);
                return ms.ToArray();
            }
        }
        internal static UserProfile Deserialize(System.IO.Stream stream)
        {
            var password_hash = stream.pop_data();
            var user_private_key = stream.pop_data();
            return new UserProfile { password_hash = password_hash, user_private_key = user_private_key };
        }
    }
}
