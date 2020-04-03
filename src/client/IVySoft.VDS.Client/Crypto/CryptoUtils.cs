using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Newtonsoft.Json;
using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Asn1.Pkcs;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.OpenSsl;
using Org.BouncyCastle.Security;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace IVySoft.VDS.Client.Crypto
{
    internal static class CryptoUtils
    {
        public static byte[] public_key_fingerprint(RSACryptoServiceProvider public_key)
        {
            byte[] sshrsa_bytes = Encoding.Default.GetBytes("ssh-rsa");
            byte[] n = public_key.ExportParameters(false).Modulus;
            byte[] e = public_key.ExportParameters(false).Exponent;

            using (var ms = new System.IO.MemoryStream())
            {
                ms.Write(ToBytes(sshrsa_bytes.Length), 0, 4);
                ms.Write(sshrsa_bytes, 0, sshrsa_bytes.Length);
                ms.Write(ToBytes(e.Length), 0, 4);
                ms.Write(e, 0, e.Length);
                ms.Write(ToBytes(n.Length + 1), 0, 4); //Remove the +1 if not Emulating Putty Gen
                ms.Write(new byte[] { 0 }, 0, 1); //Add a 0 to Emulate PuttyGen (remove it not emulating)
                ms.Write(n, 0, n.Length);
                ms.Flush();
                return sha256(ms.ToArray());
            }
        }

        public static RSACryptoServiceProvider parse_public_key(string public_key)
        {
            PemReader pemReader = new PemReader(new System.IO.StringReader(public_key));
            RsaKeyParameters publicKeyParameters = (RsaKeyParameters)pemReader.ReadObject();
            RSAParameters rsaParameters = DotNetUtilities.ToRSAParameters(publicKeyParameters);

            RSACryptoServiceProvider cryptoServiceProvider = new RSACryptoServiceProvider();
            cryptoServiceProvider.ImportParameters(rsaParameters);
            return cryptoServiceProvider;
        }

        public static byte[] symmetric_key_from_password(string password)
        {
            return KeyDerivation.Pbkdf2(
                password: password,
                salt: new byte[] { 0xdd, 0x04, 0xee, 0x6a, 0x8a, 0xd6, 0x46, 0x71, 0x9b, 0x4f, 0x9a, 0xfb, 0xc7, 0xf6, 0x73, 0xf8 },
                prf: KeyDerivationPrf.HMACSHA512,
                iterationCount: 1000,
                numBytesRequested: 32);
        }

        public static RSACryptoServiceProvider decrypt_private_key(byte[] private_key, string password)
        {
            var key = symmetric_key_from_password(password);

            byte[] key_der;
            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = key;
                aesAlg.IV = new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };

                ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);
                using (var msDecrypt = new System.IO.MemoryStream(private_key))
                {
                    using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                    {
                        using (var srDecrypt = new System.IO.MemoryStream())
                        {
                            csDecrypt.CopyTo(srDecrypt);
                            key_der = srDecrypt.ToArray();
                        }
                    }
                }
            }

            return private_key_from_der(key_der);
        }

        public static byte[] public_key_to_der(RSACryptoServiceProvider public_key)
        {
            var rsaParameters = DotNetUtilities.GetRsaPublicKey(public_key);
            return new RsaPublicKeyStructure(rsaParameters.Modulus, rsaParameters.Exponent).ToAsn1Object().GetDerEncoded();

            //return (new SubjectPublicKeyInfo(
            //    new AlgorithmIdentifier(PkcsObjectIdentifiers.RsaEncryption, DerNull.Instance),
            //    new RsaPublicKeyStructure(rsaParameters.Modulus, rsaParameters.Exponent).ToAsn1Object())).GetDerEncoded();
        }

        public static byte[] public_key_to_pem(RSACryptoServiceProvider public_key)
        {
            return Org.BouncyCastle.X509.SubjectPublicKeyInfoFactory.CreateSubjectPublicKeyInfo(DotNetUtilities.GetRsaPublicKey(public_key)).GetDerEncoded();
        }

        public static RSACryptoServiceProvider public_key_from_der(byte[] public_key)
        {
            var publicKeySequence = (DerSequence)Asn1Object.FromByteArray(public_key);

            var modulus = (DerInteger)publicKeySequence[0];
            var exponent = (DerInteger)publicKeySequence[1];

            var keyParameters = new RsaKeyParameters(false, modulus.PositiveValue, exponent.PositiveValue);
            var parameters = DotNetUtilities.ToRSAParameters(keyParameters);

            RSACryptoServiceProvider cryptoServiceProvider = new RSACryptoServiceProvider();
            cryptoServiceProvider.ImportParameters(parameters);
            return cryptoServiceProvider;
        }

        public static RSACryptoServiceProvider private_key_from_der(byte[] key_der)
        {
            var privKeyObj = Asn1Object.FromByteArray(key_der);
            var privStruct = RsaPrivateKeyStructure.GetInstance((Asn1Sequence)privKeyObj);

            // Conversion from BouncyCastle to .Net framework types
            var rsaParameters = new RSAParameters();
            rsaParameters.Modulus = privStruct.Modulus.ToByteArrayUnsigned();
            rsaParameters.Exponent = privStruct.PublicExponent.ToByteArrayUnsigned();
            rsaParameters.D = privStruct.PrivateExponent.ToByteArrayUnsigned();
            rsaParameters.P = privStruct.Prime1.ToByteArrayUnsigned();
            rsaParameters.Q = privStruct.Prime2.ToByteArrayUnsigned();
            rsaParameters.DP = privStruct.Exponent1.ToByteArrayUnsigned();
            rsaParameters.DQ = privStruct.Exponent2.ToByteArrayUnsigned();
            rsaParameters.InverseQ = privStruct.Coefficient.ToByteArrayUnsigned();

            var rsa = new RSACryptoServiceProvider();
            rsa.ImportParameters(rsaParameters);
            return rsa;
        }
        public static byte[] private_key_to_der(RSACryptoServiceProvider rsa)
        {
            RSAParameters rsaParameters = rsa.ExportParameters(true);
            return new RsaPrivateKeyStructure(
                new Org.BouncyCastle.Math.BigInteger(rsaParameters.Modulus),
                new Org.BouncyCastle.Math.BigInteger(rsaParameters.Exponent),
                new Org.BouncyCastle.Math.BigInteger(rsaParameters.D),
                new Org.BouncyCastle.Math.BigInteger(rsaParameters.P),
                new Org.BouncyCastle.Math.BigInteger(rsaParameters.Q),
                new Org.BouncyCastle.Math.BigInteger(rsaParameters.DP),
                new Org.BouncyCastle.Math.BigInteger(rsaParameters.DQ),
                new Org.BouncyCastle.Math.BigInteger(rsaParameters.InverseQ)).ToAsn1Object().GetDerEncoded();
        }
        public static byte[] private_key_to_der(RSACryptoServiceProvider rsa, string password)
        {
            return encrypt_by_aes_256_cbc(
                symmetric_key_from_password(password),
                new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 },
                private_key_to_der(rsa));
        }

        public static byte[] sha256(byte[] data)
        {
            using (var provider = SHA256.Create())
            {
                return provider.ComputeHash(data);
            }
        }
        public static byte[] sha256(byte[] data, int size)
        {
            using (var provider = SHA256.Create())
            {
                return provider.ComputeHash(data, 0, size);
            }
        }

        public static string user_credentials_to_key(string password)
        {
            var password_hash = Convert.ToBase64String(sha256(Encoding.UTF8.GetBytes(password)));
            return password_hash.Length.ToString() + "." + password_hash;
        }

        public static byte[] ToBytes(int i)
        {
            byte[] bts = BitConverter.GetBytes(i);
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(bts);
            }
            return bts;
        }
        public static byte[] decrypt_by_aes_256_cbc(byte[] key, byte[] iv, byte[] data)
        {
            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = key;
                aesAlg.IV = iv;

                ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);
                using (var msDecrypt = new System.IO.MemoryStream(data))
                {
                    using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                    {
                        using (var srDecrypt = new System.IO.MemoryStream())
                        {
                            csDecrypt.CopyTo(srDecrypt);
                            return srDecrypt.ToArray();
                        }
                    }
                }
            }
        }

        public static byte[] encrypt_by_aes_256_cbc(byte[] key, byte[] iv, byte[] data)
        {
            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = key;
                aesAlg.IV = iv;

                ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);
                using (var msEncrypt = new System.IO.MemoryStream())
                {
                    using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    {
                        using (var srEncrypt = new System.IO.MemoryStream(data))
                        {
                            srEncrypt.CopyTo(csEncrypt);
                        }
                    }

                    return msEncrypt.ToArray();
                }
            }
        }

        public static byte[] decrypt_by_private_key(RSACryptoServiceProvider privateKey, byte[] body)
        {
            return privateKey.Decrypt(body, false);
        }

    }
}
