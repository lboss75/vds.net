using IVySoft.VDS.Client.Dto;
using System;
using System.Collections.Generic;
using System.Text;

namespace IVySoft.VDS.Client.Api
{
    public class WalletBalance
    {
        private readonly string issuer_;
        private readonly long balance_;
        private readonly long proposed_balance_;
        private readonly string currency_;

        public string Issuer { get => this.issuer_; }
        public long Balance { get => this.balance_; }
        public long DeltaBalance { get => this.ProposedBalance - this.Balance; }
        public long ProposedBalance { get => this.proposed_balance_; }
        public string Currency { get => this.currency_; }

        internal WalletBalance(WalletBalanceMessage x)
        {
            this.issuer_ = x.issuer;
            this.balance_ = x.confirmed_balance;
            this.proposed_balance_ = x.proposed_balance;
            this.currency_ = x.currency;
        }
    }
}
