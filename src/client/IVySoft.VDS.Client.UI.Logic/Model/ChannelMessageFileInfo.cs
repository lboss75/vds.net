using IVySoft.VDS.Client.Transactions.Data;
using System.ComponentModel;

namespace IVySoft.VDS.Client.UI.Logic.Model
{
    public class ChannelMessageFileInfo : INotifyPropertyChanged
    {
        private Api.ChannelMessageFileInfo info_;
        private string name_;
        private long length_;
        private bool inProgress_;
        private int progress_;

        public ChannelMessageFileInfo(Api.ChannelMessageFileInfo fi)
        {
            this.InProgress = false;
            this.info_ = fi;
            this.name_ = fi.Name;
            this.length_ = fi.Size;
        }
        public ChannelMessageFileInfo(string name, long length)
        {
            this.InProgress = false;
            this.name_ = name;
            this.length_ = length;
        }

        public string Name
        {
            get => this.name_;
        }

        public long Length
        {
            get => this.length_;
        }

        public Api.ChannelMessageFileInfo Info
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