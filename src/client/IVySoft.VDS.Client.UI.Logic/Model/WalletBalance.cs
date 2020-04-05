namespace IVySoft.VDS.Client.UI.Logic.Model
{
    public class WalletBalance
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public long Balance { get; set; }
        public long ProposedBalance { get; set; }
        public string Currency { get; set; }
        public string Issuer { get; set; }
        public long DeltaBalance { get => this.ProposedBalance - this.Balance; }
    }
}