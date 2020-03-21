using System.ComponentModel;

namespace IVySoft.VDS.Client.UI.Logic.Model
{
    public class ChannalMessageFileInfo : INotifyPropertyChanged
    {
        private string name_;
        private long lenght_;
        private bool inProgress_;
        private int progress_;

        public string Name
        {
            get => name_; set
            {
                if (this.name_ != value)
                {
                    this.name_ = value;
                    if (null != this.PropertyChanged)
                    {
                        this.PropertyChanged(this, new PropertyChangedEventArgs(nameof(Name)));
                    }
                }
            }
        }
        public long Lenght
        {
            get => lenght_; set
            {
                if (this.lenght_ != value)
                {
                    this.lenght_ = value;
                    if (null != this.PropertyChanged)
                    {
                        this.PropertyChanged(this, new PropertyChangedEventArgs(nameof(Lenght)));
                    }
                }
            }
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