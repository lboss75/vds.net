namespace IVySoft.VDS.Client.Transactions.Data
{
    internal class CryptedChannelMessage
    {
        public string read_id { get; set; }
        public string crypted_key { get; set; }
        public string crypted_data { get; set; }
    }
}