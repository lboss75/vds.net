using System.Security.Cryptography;

namespace IVySoft.VDS.Client
{
    internal class KeyPair
    {
        public RSACryptoServiceProvider PublicKey { get; set; }
        public RSACryptoServiceProvider PrivateKey { get; set; }

    }
}