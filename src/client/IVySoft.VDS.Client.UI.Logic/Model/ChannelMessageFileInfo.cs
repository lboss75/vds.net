using System.ComponentModel;

namespace IVySoft.VDS.Client.UI.Logic.Model
{
    public class ChannelMessageFileInfo : INotifyPropertyChanged
    {
        private Transactions.FileInfo info_;
        private bool inProgress_;
        private int progress_;

        public ChannelMessageFileInfo(Transactions.FileInfo fi)
        {
            this.InProgress = false;
            this.info_ = fi;

        }

        public string Name
        {
            get => this.info_.Name;
        }

        public long Lenght
        {
            get => this.info_.Size;
        }

        public Transactions.FileInfo Info
        {
            get => this.info_;
            set => this.info_ = value;
        }

        public bool InProgress
        {
            get => inProgress_; set
            {
                if (this.inProgress_ != value)
                {
                    this.inProgress_ = value;
                    if (null != this.PropertyChanged)
                    {
                        this.PropertyChanged(this, new PropertyChangedEventArgs(nameof(InProgress)));
                    }
                }
            }
        }

        public int Progress
        {
            get => progress_; set
            {
                if (this.progress_ != value)
                {
                    this.progress_ = value;
                    if (null != this.PropertyChanged)
                    {
                        this.PropertyChanged(this, new PropertyChangedEventArgs(nameof(Progress)));
                    }
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}