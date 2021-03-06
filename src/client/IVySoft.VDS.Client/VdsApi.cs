﻿using IVySoft.VDS.Client.Api;
using IVySoft.VDS.Client.Transactions;
using IVySoft.VDS.Client.Transactions.Data;
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
using System.IO.Compression;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace IVySoft.VDS.Client
{
    public class VdsApi : IDisposable
    {
        private VdsApiClient client_;
        private readonly VdsApiClientOptions options_;

        public VdsApi(VdsApiConfig config)
        {
            this.options_ = new VdsApiClientOptions
            {
                ServiceUri = new Uri(config.ServiceUri),
            };
        }

        public async Task<string> CreateUser(System.Threading.CancellationToken token, string login, string password)
        {
            var client = await this.get_client(token);
            var user_key = new RSACryptoServiceProvider(4096);
            
            var profile_data = new UserProfile
            {
                password_hash = Crypto.CryptoUtils.sha256(Encoding.UTF8.GetBytes(password)),
                user_private_key = Crypto.CryptoUtils.private_key_to_der(user_key, password)
            }.Serialize();
            var profile_id = await client.call<BlockInfo>(token, "upload", Convert.ToBase64String(profile_data));

            using (var ms = new System.IO.MemoryStream())
            {
                new Transactions.StoreBlockTransaction(
                    Convert.FromBase64String(profile_id.hash),
                    profile_data.Length,
                    profile_id.replica_size,
                    profile_id.replicas.Select(x => Convert.FromBase64String(x)).ToArray(),
                    user_key
                ).Serialize(ms);

                new Transactions.CreateUserTransaction
                {
                    user_email = login,
                    user_name = login,
                    user_profile_id = Convert.FromBase64String(profile_id.hash),
                    user_public_key = Crypto.CryptoUtils.public_key_to_der(user_key)
                }.Serialize(ms);

                return await this.client_.call<string>(
                    token,
                    "broadcast",
                    Convert.ToBase64String(ms.ToArray()));
            }
        }

        public async Task<SyncStatistic[]> GetDistributionMap(CancellationToken token, byte[][] hashes)
        {
            var client = await this.get_client(token);
            return await client.call<SyncStatistic[]>(
                token,
                "distribution_map",
                new object[] { hashes.Select(x => Convert.ToBase64String(x)).ToArray() });
        }

        public async Task<ServerStatistic> GetStatistics(System.Threading.CancellationToken token)
        {
            var client = await this.get_client(token);
            return await this.client_.call<ServerStatistic>(token, "statistics");
        }

        public async Task<IEnumerable<Wallet>> GetWallets(System.Threading.CancellationToken token, ThisUser user)
        {
            var result = new List<Wallet>();
            var client = await this.get_client(token);
            var messages = await client.call<CryptedChannelMessage[]>(token, "get_channel_messages", user.Id);
            foreach(var message in messages.Select(x => user.PersonalChannel.decrypt(x)))
            {
                switch (message)
                {
                    case Transactions.CreateWalletMessage msg:
                        {
                            result.Add(new Wallet(msg));
                            break;
                        }
                }
            }

            return result;
        }
        public async Task<IEnumerable<WalletBalance>> GetBalance(System.Threading.CancellationToken token, Wallet wallet)
        {
            var messages = await this.client_.call<Dto.WalletBalanceMessage[]>(token, "balance", wallet.Id);
            return messages.Select(x => new WalletBalance(x));
        }

        public async Task<string[]> get_sync_state(System.Threading.CancellationToken token)
        {
            var client = await this.get_client(token);
            return await client.call<string[]>(token, "log_head");
        }

        public async Task<Channel[]> GetChannels(System.Threading.CancellationToken token, Api.ThisUser user)
        {
            var messages = await this.client_.call<CryptedChannelMessage[]>(token, "get_channel_messages", user.Id);
            return messages.Select(x => user.PersonalChannel.decrypt(x)).OfType<ChannelCreateTransaction>().Select(x => new Channel(x)).ToArray();
        }

        public async Task<Api.ChannelMessage[]> GetChannelMessages(System.Threading.CancellationToken token, Api.Channel channel)
        {
            var client = await this.get_client(token);
            var messages = await client.call<CryptedChannelMessage[]>(token, "get_channel_messages", channel.Id);
            return messages.Select(x => channel.decrypt(x)).OfType<UserMessageTransaction>().Select(x => new Api.ChannelMessage(x)).ToArray();
        }

        //public async Task<ChannelMessage[]> GetChannelMessages(Api.Channel channel)
        //{
        //    var client = await this.get_client();
        //    var messages = await client.call<CryptedChannelMessage[]>("get_channel_messages", channel.Id);
        //    return messages.Select(x => channel.decrypt(x)).ToArray();
        //}

        public async Task<byte[]> AllocateStorage(System.Threading.CancellationToken token, Api.ThisUser user, string folder, long size, string usage_type)
        {
            System.IO.Directory.CreateDirectory(folder);

            var body = JsonConvert.SerializeObject(new { vds = "0.1", name = user.Id, size = size.ToString(), usage = usage_type });
            var sig = user.PersonalChannel.WriteKey.PrivateKey.SignData(System.Text.Encoding.UTF8.GetBytes(body), new SHA256CryptoServiceProvider());
            System.IO.File.WriteAllText(
                System.IO.Path.Combine(folder, ".vds_storage.json"),
                JsonConvert.SerializeObject(new
                {
                    vds = "0.1",
                    name = user.Id,
                    size = size.ToString(),
                    usage = usage_type,
                    sign = Convert.ToBase64String(sig)
                }));

            var der = Crypto.CryptoUtils.public_key_to_der(user.PersonalChannel.WriteKey.PublicKey);
            return Convert.FromBase64String(await this.client_.call<string>(token, "allocate_storage", Convert.ToBase64String(der), folder));
        }

        public async Task<StorageInfo[]> GetStorage(System.Threading.CancellationToken token, Api.ThisUser user)
        {
            var client = await this.get_client(token);
            return await client.call<StorageInfo[]>(token, "devices", user.Id);
        }

        public Task<byte[]> Download(System.Threading.CancellationToken token, ChannelMessageFileBlock block)
        {
            return this.Download(token, block.Data);
        }

        private async Task<byte[]> Download(System.Threading.CancellationToken token, FileBlock file_block)
        {
            var client = await this.get_client(token);
            var replicas = await client.call<LookingBlockResponse>(token, "looking_block", file_block.BlockId);
            var result = await client.call<string>(token, "download", replicas.replicas);
            return decrypt_file_block(file_block, Convert.FromBase64String(result));
        }

        private byte[] decrypt_file_block(FileBlock file_block, byte[] data)
        {
            var iv_data = new byte[] { 0xa5, 0xbb, 0x9f, 0xce, 0xc2, 0xe4, 0x4b, 0x91, 0xa8, 0xc9, 0x59, 0x44, 0x62, 0x55, 0x90, 0x24 };
            var zipped = Crypto.CryptoUtils.decrypt_by_aes_256_cbc(file_block.BlockKey, iv_data, data);
            var original_data = inflate(zipped);
            var sig = Crypto.CryptoUtils.sha256(original_data);
            if (!file_block.BlockId.SequenceEqual(sig))
            {
                throw new Exception("Data is corrupted");
            }

            return original_data;
        }


        public async Task<Api.ThisUser> Login(System.Threading.CancellationToken token, string login, string password)
        {
            var client = await this.get_client(token);
            var keys = await client.call<LoginResponse>(token, "login", login);
            var user_profile_replicas = await client.call<LookingBlockResponse>(token, "looking_block", Convert.FromBase64String(keys.user_profile_id));
            var user_profile_data = await client.call<string>(token, "download", user_profile_replicas.replicas);
            var user_profile = UserProfile.Deserialize(new System.IO.MemoryStream(Convert.FromBase64String(user_profile_data)));

            if(Convert.ToBase64String(user_profile.password_hash) != Convert.ToBase64String(Crypto.CryptoUtils.sha256(Encoding.UTF8.GetBytes(password))))
            {
                throw new Exception("Invalid password");
            }

            var private_key = Crypto.CryptoUtils.decrypt_private_key(user_profile.user_private_key, password);
            var public_key = Crypto.CryptoUtils.parse_public_key(keys.public_key);

            return new Api.ThisUser(public_key, private_key);
        }

        private async Task<VdsApiClient> get_client(System.Threading.CancellationToken token)
        {
            if(null == this.client_)
            {
                var client = new VdsApiClient();
                await client.Connect(token, this.options_);
                this.client_ = client;
            }

            return this.client_;
        }


        public void Dispose()
        {
            if(null != this.client_)
            {
                this.client_.Dispose();
            }
        }

        public async Task<string> UploadFiles(
            System.Threading.CancellationToken token,
            Api.Channel channel,
            string message,
            FileUploadStream[] inputFiles)
        {
            var chunkSize = 67108864;
            var chunk = new byte[chunkSize];
            var files = new List<FileInfo>();
            var playload = new System.IO.MemoryStream();
            foreach (var inputFile in inputFiles)
            {
                var blocks = new List<FileBlock>();
                using (var provider = SHA256.Create())
                {
                    long size = 0;
                    using (var f = new System.IO.FileStream(inputFile.SystemPath, System.IO.FileMode.Open, System.IO.FileAccess.Read, System.IO.FileShare.Read))
                    {
                        int offset = 0;
                        for (; ; )
                        {
                            var readed = f.Read(chunk, offset, chunkSize - offset);
                            if (0 < readed)
                            {
                                provider.TransformBlock(chunk, offset, readed, null, 0);
                                offset += readed;
                                size += readed;

                                if (offset == chunkSize)
                                {
                                    var block_info = await save_block(token, chunk, chunkSize);
                                    new Transactions.StoreBlockTransaction(
                                    
                                        block_info.Key.BlockId,
                                        chunkSize,
                                        block_info.Value.replica_size,
                                        block_info.Value.replicas.Select(x => Convert.FromBase64String(x)).ToArray(),
                                        channel.WriteKey.PrivateKey
                                        ).Serialize(playload);

                                    blocks.Add(block_info.Key);
                                    offset = 0;
                                    if (null != inputFile.ProgressCallback)
                                    {
                                        if (!inputFile.ProgressCallback((int)(100 * size / f.Length)))
                                        {
                                            return string.Empty;
                                        }
                                    }
                                }
                                continue;
                            }
                            if (0 < offset)
                            {
                                var block_info = await save_block(token, chunk, offset);
                                new Transactions.StoreBlockTransaction(

                                    block_info.Key.BlockId,
                                    offset,
                                    block_info.Value.replica_size,
                                    block_info.Value.replicas.Select(x => Convert.FromBase64String(x)).ToArray(),
                                    channel.WriteKey.PrivateKey
                                    ).Serialize(playload);

                                blocks.Add(block_info.Key);
                                if(null != inputFile.ProgressCallback)
                                {
                                    if(!inputFile.ProgressCallback((int)(100 * size / f.Length)))
                                    {
                                        return string.Empty;
                                    }
                                }
                            }
                            break;
                        }
                        provider.TransformFinalBlock(chunk, 0, 0);
                    }

                    if (inputFile.FileHash != null && !inputFile.FileHash.SequenceEqual(provider.Hash))
                    {
                        throw new Exception($"File {inputFile.SystemPath} has been changed during upload process");
                    }

                    var info = new FileInfo(
                        inputFile.Name,
                        "application/octet-stream",
                        size,
                        provider.Hash,
                        blocks.ToArray()
                    );

                    if (null != inputFile.UploadedCallback)
                    {
                        inputFile.UploadedCallback(new ChannelMessageFileInfo(info));
                    }

                    files.Add(info);
                }
            }

            var cripted_data = channel.channel_encrypt(
                    new Transactions.UserMessageTransaction(
                        string.IsNullOrWhiteSpace(message) ? "{\"$type\":\"SimpleMessage\"}" : message,
                        files.ToArray()
                    ).Serialize());
            playload.Write(cripted_data, 0, cripted_data.Length);

            return await this.client_.call<string>(
                token,
                "broadcast",
                Convert.ToBase64String(playload.ToArray()));
        }

        public async Task<Api.Channel> create_channel(
            System.Threading.CancellationToken token,
            Api.ThisUser user,
            string channel_type,
            string channel_name) {

            var channel_read_key = new RSACryptoServiceProvider(4096);
            var channel_write_key = new RSACryptoServiceProvider(4096);
            var channel_admin_key = new RSACryptoServiceProvider(4096);

            var playload = new System.IO.MemoryStream();
            var channel = new Transactions.ChannelCreateTransaction
            (
                channel_type: channel_type,
                channel_name: channel_name,
                read_public_key: Crypto.CryptoUtils.public_key_to_der(channel_read_key),
                read_private_key: Crypto.CryptoUtils.private_key_to_der(channel_read_key),
                write_public_key: Crypto.CryptoUtils.public_key_to_der(channel_write_key),
                write_private_key: Crypto.CryptoUtils.private_key_to_der(channel_write_key),
                admin_public_key: Crypto.CryptoUtils.public_key_to_der(channel_admin_key),
                admin_private_key: Crypto.CryptoUtils.private_key_to_der(channel_admin_key)
            );

            byte[] message;
            using(var ms = new System.IO.MemoryStream())
            {
                channel.Serialize(ms);
                message = ms.ToArray();

            }
            var cripted_data = user.PersonalChannel.channel_encrypt(message);
            playload.Write(cripted_data, 0, cripted_data.Length);

            var client = await this.get_client(token);
            var transaction_id = await client.call<string>(
                token,
                "broadcast",
                Convert.ToBase64String(playload.ToArray()));

            return new Channel(channel);
        }

        private async Task<KeyValuePair<FileBlock, BlockInfo>> save_block(System.Threading.CancellationToken token, byte[] data, int size)
        {
            var client = await this.get_client(token);
            var key_data = Crypto.CryptoUtils.sha256(data, size);
            var iv_data = new byte[] { 0xa5, 0xbb, 0x9f, 0xce, 0xc2, 0xe4, 0x4b, 0x91, 0xa8, 0xc9, 0x59, 0x44, 0x62, 0x55, 0x90, 0x24 };

            var key_data2 = Crypto.CryptoUtils.sha256(Crypto.CryptoUtils.encrypt_by_aes_256_cbc(key_data, iv_data, data));
            var zipped = deflate(data, size);
            var crypted_data = Crypto.CryptoUtils.encrypt_by_aes_256_cbc(key_data2, iv_data, zipped);
            var result = await client.call<BlockInfo>(token, "upload", Convert.ToBase64String(crypted_data));
            return new KeyValuePair<FileBlock, BlockInfo>(
                new FileBlock(
                    key_data,
                    key_data2,
                    size
                ),
                result);
        }

        private static byte[] inflate(byte[] data)
        {
            using (var ms = new System.IO.MemoryStream(data))
            {
                using (var compressionStream = new DeflateStream(ms, CompressionMode.Decompress))
                {
                    using (var result = new System.IO.MemoryStream())
                    {
                        compressionStream.CopyTo(result);
                        return result.ToArray();
                    }
                }
            }
        }

        private static byte[] deflate(byte[] data)
        {
            using (var ms = new System.IO.MemoryStream())
            {
                using (var compressionStream = new DeflateStream(ms, CompressionMode.Compress))
                {
                    compressionStream.Write(data, 0, data.Length);
                    compressionStream.Close();
                }
                return ms.ToArray();
            }
        }

        private static byte[] deflate(byte[] data, int size)
        {
            using (var ms = new System.IO.MemoryStream())
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
