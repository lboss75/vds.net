using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Asn1.Pkcs;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.OpenSsl;
using Org.BouncyCastle.Security;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace IVySoft.VDS.Client
{
    public class VdsApi : IVdsApi
    {
        private VdsApiClient client_;
        private string public_key_id_;
        private readonly VdsApiClientOptions options_;

        private readonly Dictionary<string, KeyPair> read_keys_ = new Dictionary<string, KeyPair>();
        private readonly Dictionary<string, KeyPair> write_keys_ = new Dictionary<string, KeyPair>();

        public VdsApi(VdsApiConfig config)
        {
            this.options_ = new VdsApiClientOptions
            {
                ServiceUri = new Uri(config.ServiceUri),
                ConnectionTimeout = TimeSpan.FromSeconds(config.ConnectionTimeout),
                SendTimeout = TimeSpan.FromSeconds(config.SendTimeout)
            };
        }

        public async Task<ChannelMessage[]> GetChannels()
        {
            var messages = await this.client_.call<CryptedChannelMessage[]>("get_channel_messages", this.public_key_id_);
            return messages.Select(x => this.decrypt(x)).ToArray();
        }

        public async Task<ChannelMessage[]> GetChannelMessages(Transactions.ChannelCreateTransaction channel)
        {
            var messages = await this.client_.call<CryptedChannelMessage[]>("get_channel_messages", channel.Id);
            return messages.Select(x => this.decrypt(channel.ReadKey, x)).ToArray();
        }

        private ChannelMessage decrypt(CryptedChannelMessage message)
        {
            var read_keys = this.read_keys_[message.read_id];
            return decrypt(read_keys, message);
        }
        private ChannelMessage decrypt(KeyPair read_keys, CryptedChannelMessage message)
        {
            var key_data = decrypt_by_private_key(read_keys.PrivateKey, Convert.FromBase64String(message.crypted_key));


            byte[] key;
            byte[] iv; ;
            using (var stream = new MemoryStream(key_data))
            {
                key = stream.pop_data();
                iv = stream.pop_data();
            }

            var data = decrypt_by_aes_256_cbc(key, iv, Convert.FromBase64String(message.crypted_data));

            using (var stream = new MemoryStream(data))
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

                        ////channel_message
                        //case 99:
                        //    {
                        //        const channel_id = base64(buffer_pop_data(data));
                        //        const read_id = base64(buffer_pop_data(data));
                        //        const write_id = base64(buffer_pop_data(data));
                        //        const crypted_key = base64(buffer_pop_data(data));
                        //        const crypted_data = base64(buffer_pop_data(data));
                        //        const signature = base64(buffer_pop_data(data));

                        //        return {
                        //        type: 'channel_message',
                        //    channel_id: channel_id,
                        //    read_id: read_id,
                        //    write_id: write_id,
                        //    crypted_key: crypted_key,
                        //    crypted_data: crypted_data,
                        //    signature: signature
                        //        };
                }

                throw new Exception($"Invalid message {message_id}");
            }
        }

        public async Task<byte[]> Dawnload(FileBlock file_block)
        {
            var result = await this.client_.call<string>("download", file_block.Replicas.Select(x => Convert.ToBase64String(x)).ToArray());
            return decrypt_file_block(file_block, Convert.FromBase64String(result));
        }

        private byte[] decrypt_file_block(FileBlock file_block, byte[] data)
        {
            var iv_data = new byte[] { 0xa5, 0xbb, 0x9f, 0xce, 0xc2, 0xe4, 0x4b, 0x91, 0xa8, 0xc9, 0x59, 0x44, 0x62, 0x55, 0x90, 0x24 };
            var zipped = decrypt_by_aes_256_cbc(file_block.BlockKey, iv_data, data);
            var original_data = inflate(zipped);
            var sig = sha256(original_data);
            if (!file_block.BlockId.SequenceEqual(sig))
            {
                throw new Exception("Data is corrupted");
            }

            return original_data;
        }

        private byte[] decrypt_by_aes_256_cbc(byte[] key, byte[] iv, byte[] data)
        {
            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = key;
                aesAlg.IV = iv;

                ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);
                using (MemoryStream msDecrypt = new MemoryStream(data))
                {
                    using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                    {
                        using (MemoryStream srDecrypt = new MemoryStream())
                        {
                            csDecrypt.CopyTo(srDecrypt);
                            return srDecrypt.ToArray();
                        }
                    }
                }
            }
        }

        private byte[] encrypt_by_aes_256_cbc(byte[] key, byte[] iv, byte[] data)
        {
            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = key;
                aesAlg.IV = iv;

                ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);
                using (MemoryStream msEncrypt = new MemoryStream())
                {
                    using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    {
                        using (MemoryStream srEncrypt = new MemoryStream(data))
                        {
                            srEncrypt.CopyTo(csEncrypt);
                        }
                    }

                    return msEncrypt.ToArray();
                }
            }
        }

        private byte[] decrypt_by_private_key(RSACryptoServiceProvider privateKey, byte[] body)
        {
            return privateKey.Decrypt(body, false);
        }

        public async Task Login(string login, string password)
        {
            var client = await this.get_client();
            var keys = await client.call<LoginResponse>("login", login, user_credentials_to_key(password));

            var private_key = decrypt_private_key(Convert.FromBase64String(keys.private_key), password);
            var public_key = parse_public_key(keys.public_key);
            this.public_key_id_ = Convert.ToBase64String(public_key_fingerprint(public_key));

            this.add_read_key(this.public_key_id_, public_key, private_key);
            this.add_write_key(this.public_key_id_, public_key, private_key);
        }

        private async Task<VdsApiClient> get_client()
        {
            if(null == this.client_)
            {
                var client = new VdsApiClient();
                await client.Connect(this.options_);
                this.client_ = client;
            }

            return this.client_;
        }

        private void add_write_key(string public_key_id, RSACryptoServiceProvider public_key, RSACryptoServiceProvider private_key)
        {
            this.read_keys_.Add(public_key_id, new KeyPair { PublicKey = public_key, PrivateKey = private_key });
        }

        private void add_read_key(string public_key_id, RSACryptoServiceProvider public_key, RSACryptoServiceProvider private_key)
        {
            this.write_keys_.Add(public_key_id, new KeyPair { PublicKey = public_key, PrivateKey = private_key });
        }

        private byte[] public_key_fingerprint(RSACryptoServiceProvider public_key)
        {
            byte[] sshrsa_bytes = Encoding.Default.GetBytes("ssh-rsa");
            byte[] n = public_key.ExportParameters(false).Modulus;
            byte[] e = public_key.ExportParameters(false).Exponent;

            using (MemoryStream ms = new MemoryStream())
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

        internal static RSACryptoServiceProvider parse_public_key(string public_key)
        {
            PemReader pemReader = new PemReader(new StringReader(public_key));
            RsaKeyParameters publicKeyParameters = (RsaKeyParameters)pemReader.ReadObject();
            RSAParameters rsaParameters = DotNetUtilities.ToRSAParameters(publicKeyParameters);

            RSACryptoServiceProvider cryptoServiceProvider = new RSACryptoServiceProvider();
            cryptoServiceProvider.ImportParameters(rsaParameters);
            return cryptoServiceProvider;
        }

        private byte[] symmetric_key_from_password(string password)
        {
            return KeyDerivation.Pbkdf2(
                password: password,
                salt: new byte[] { 0xdd, 0x04, 0xee, 0x6a, 0x8a, 0xd6, 0x46, 0x71, 0x9b, 0x4f, 0x9a, 0xfb, 0xc7, 0xf6, 0x73, 0xf8 },
                prf: KeyDerivationPrf.HMACSHA512,
                iterationCount: 1000,
                numBytesRequested: 32);
        }

        private RSACryptoServiceProvider decrypt_private_key(byte[] private_key, string password)
        {
            var key = symmetric_key_from_password(password);

            byte[] key_der;
            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = key;
                aesAlg.IV = new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };

                ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);
                using (MemoryStream msDecrypt = new MemoryStream(private_key))
                {
                    using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                    {
                        using (MemoryStream srDecrypt = new MemoryStream())
                        {
                            csDecrypt.CopyTo(srDecrypt);
                            key_der = srDecrypt.ToArray();
                        }
                    }
                }
            }

            return private_key_from_der(key_der);
        }
        internal static RSACryptoServiceProvider public_key_from_der(byte[] public_key)
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

        internal static RSACryptoServiceProvider private_key_from_der(byte[] key_der)
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

        private static byte[] sha256(byte[] data)
        {
            using (var provider = SHA256.Create())
            {
                return provider.ComputeHash(data);
            }
        }
        private static byte[] sha256(byte[] data, int size)
        {
            using (var provider = SHA256.Create())
            {
                return provider.ComputeHash(data, 0, size);
            }
        }

        private static string user_credentials_to_key(string password)
        {
            var password_hash = Convert.ToBase64String(sha256(Encoding.UTF8.GetBytes(password)));
            return password_hash.Length.ToString() + "." + password_hash;
        }

        private static byte[] ToBytes(int i)
        {
            byte[] bts = BitConverter.GetBytes(i);
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(bts);
            }
            return bts;
        }

        public void Dispose()
        {
            if(null != this.client_)
            {
                this.client_.Dispose();
            }
        }

        public async Task<ChannelMessage> UploadFiles(string channel_id, string message, FileUploadStream[] inputFiles)
        {
            var chunkSize = 67108864;
            var chunk = new byte[chunkSize];
            var files = new List<UploadedFile>();
            foreach (var inputFile in inputFiles)
            {
                var blocks = new List<FileBlock>();
                using (var provider = SHA256.Create())
                {
                    int offset = 0;
                    long size = 0;
                    for (; ; )
                    {
                        var readed = inputFile.Stream.Read(chunk, offset, chunkSize - offset);
                        if (0 < readed)
                        {
                            provider.TransformBlock(chunk, offset, readed, null, 0);
                            offset += readed;
                            size += readed;

                            if (offset == chunkSize)
                            {
                                blocks.Add(await save_block(chunk, chunkSize));
                                offset = 0;
                            }
                            continue;
                        }
                        if (0 < offset)
                        {
                            blocks.Add(await save_block(chunk, offset));
                        }
                        break;
                    }
                    provider.TransformFinalBlock(chunk, 0, 0);

                    files.Add(new UploadedFile
                    {
                        file_name = inputFile.Name,
                        mime_type = "application/octet-stream",
                        file_size = size,
                        file_id = provider.Hash,
                        file_blocks = blocks.ToArray()
                    });
                }
            }
            return await this.client_.call<ChannelMessage>(
                "broadcast",
                Convert.ToBase64String(channel_encrypt(channel_id, 
                    new ChannelMessageTransaction
                    {
                        message = string.IsNullOrWhiteSpace(message) ? "{\"$type\":\"SimpleMessage\"}" : message,
                        files = files.ToArray()
                    }.Serialize())));
        }

        private byte[] channel_encrypt(string channel_id, byte[] data)
        {
            throw new NotImplementedException();
        }

        private async Task<FileBlock> save_block(byte[] data, int size)
        {
            var key_data = sha256(data, size);
            var iv_data = new byte[] { 0xa5, 0xbb, 0x9f, 0xce, 0xc2, 0xe4, 0x4b, 0x91, 0xa8, 0xc9, 0x59, 0x44, 0x62, 0x55, 0x90, 0x24 };

            var key_data2 = sha256(encrypt_by_aes_256_cbc(key_data, iv_data, data));
            var zipped = deflate(data, size);
            var crypted_data = encrypt_by_aes_256_cbc(key_data2, iv_data, zipped);
            var result = await this.client_.call<BlockInfo>("upload", Convert.ToBase64String(crypted_data));
            return new FileBlock(
                key_data,
                key_data2,
                result.replicas.Select(x => Convert.FromBase64String(x)).ToArray(),
                size
            );
        }

        private byte[] inflate(byte[] data)
        {
            using (var ms = new MemoryStream())
            {
                using (var compressionStream = new DeflateStream(ms, CompressionMode.Decompress))
                {
                    compressionStream.Write(data, 0, data.Length);
                    compressionStream.Close();
                }
                return ms.ToArray();
            }
        }

        private byte[] deflate(byte[] data)
        {
            using (var ms = new MemoryStream())
            {
                using (var compressionStream = new DeflateStream(ms, CompressionMode.Compress))
                {
                    compressionStream.Write(data, 0, data.Length);
                    compressionStream.Close();
                }
                return ms.ToArray();
            }
        }

        private byte[] deflate(byte[] data, int size)
        {
            using (var ms = new MemoryStream())
            {
                using (var compressionStream = new DeflateStream(ms, CompressionMode.Compress))
                {
                    compressionStream.Write(data, 0, size);
                    compressionStream.Close();
                }
                return ms.ToArray();
            }
        }
    }
}
