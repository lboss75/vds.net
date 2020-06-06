using System.ComponentModel;

namespace IVySoft.VDS.Client.UI.Logic.Model
{
    public class WalletBalance : INotifyPropertyChanged
    {
        private string id_;
        private string name_;
        private long balance_;
        private long proposedBalance_;
        private string currency_;
        private string issuer_;

        public string Id
        {
            get => id_;
            set
            {
                if (id_ != value)
                {
                    id_ = value;
                    this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Id)));
                }
            }
        }
        public string Name { get => name_;
            set
            {
                if (name_ != value)
                {
                    name_ = value;
                    this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Name)));
                }
            }
        }
        public long Balance { get => balance_;
            set
            {
                if (balance_ != value)
                {
                    balance_ = value;
                    this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Balance)));
                    this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(DeltaBalance)));
                }
            }
        }
        public long ProposedBalance { get => proposedBalance_;
            set
            {
                if (proposedBalance_ != value)
                {
                    proposedBalance_ = value;
                    this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ProposedBalance)));
                    this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(DeltaBalance)));
                }
            }
        }
        public string Currency { get => currency_;
            set
            {
                if (currency_ != value)
                {
                    currency_ = value;
                    this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Currency)));
                }
            }
        }
        public string Issuer { get => issuer_;
            set
            {
                if (issuer_ != value)
                {
                    issuer_ = value;
                    this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Issuer)));
                }
            }
        }

        public long DeltaBalance { get => this.ProposedBalance - this.Balance; }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}