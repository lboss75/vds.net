using IVySoft.VDS.Client.Api;
using System;
using System.ComponentModel;

namespace IVySoft.VDS.Client.UI.WPF.Monitor
{
    public class ServerSessionTrafficStatistic : INotifyPropertyChanged
    {
        private string from_;
        private string to_;
        private string msg_;
        private long sent_;
        private long sent_count_;
        private long rcv_good_;
        private long rcv_good_count_;
        private long rcv_bad_;
        private long rcv_bad_count_;

        public string From
        {
            get => this.from_;
            set
            {
                if(this.from_ != value)
                {
                    this.from_ = value;
                    this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(From)));
                }
            }
        }
        public string To
        {
            get => this.to_;
            set
            {
                if (this.to_ != value)
                {
                    this.to_ = value;
                    this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(To)));
                }
            }
        }
        public string Msg { get => msg_;
            set
            {
                if (this.msg_ != value)
                {
                    this.msg_ = value;
                    this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Msg)));
                }
            }
        }
        public long Sent
        {
            get => sent_;
            set
            {
                if (this.sent_ != value)
                {
                    this.sent_ = value;
                    this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Sent)));
                }
            }
        }

        public long SentCount
        {
            get => sent_count_;
            set
            {
                if (this.sent_count_ != value)
                {
                    this.sent_count_ = value;
                    this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SentCount)));
                }
            }
        }
        public long RcvGood { get => rcv_good_;
            set
            {
                if (this.rcv_good_ != value)
                {
                    this.rcv_good_ = value;
                    this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(RcvGood)));
                }
            }
        }
        public long RcvGoodCount { get => rcv_good_count_;
            set
            {
                if (this.rcv_good_count_ != value)
                {
                    this.rcv_good_count_ = value;
                    this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(RcvGoodCount)));
                }
            }
        }
        public long RcvBad { get => rcv_bad_;
            set
            {
                if (this.rcv_bad_ != value)
                {
                    this.rcv_bad_ = value;
                    this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(RcvBad)));
                }
            }
        }
        public long RcvBadCount { get => rcv_bad_count_;
            set
            {
                if (this.rcv_bad_count_ != value)
                {
                    this.rcv_bad_count_ = value;
                    this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(RcvBadCount)));
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}