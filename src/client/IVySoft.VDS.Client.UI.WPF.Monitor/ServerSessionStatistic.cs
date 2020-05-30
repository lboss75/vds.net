using IVySoft.VDS.Client.Api;
using System;
using System.ComponentModel;
using System.Linq;

namespace IVySoft.VDS.Client.UI.WPF.Monitor
{
    public class ServerSessionStatistic : INotifyPropertyChanged
    {
        private readonly string partner_;
        private readonly string address_;
        private int mtu_;
        private int outputQueue_;
        private int inputQueue_;
        private int idle_;
        private int delay_;
        private int service_;
        private long data_10m_;
        private long data_1m_;
        private long data_10s_;
        private long other_traffic_;

        public event PropertyChangedEventHandler PropertyChanged;

        public string Partner => partner_;
        public string Address => address_;
        public int MTU
        {
            get => mtu_; private set
            {
                if (mtu_ != value)
                {
                    this.mtu_ = value;
                    this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(MTU)));
                }
            }
        }
        public int OutputQueue
        {
            get => outputQueue_; private set
            {
                if (outputQueue_ != value)
                {
                    outputQueue_ = value;
                    this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(OutputQueue)));
                }
            }
        }
        public int InputQueue
        {
            get => inputQueue_; private set
            {
                if (inputQueue_ != value)
                {
                    inputQueue_ = value;
                    this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(InputQueue)));
                }
            }
        }
        public int Idle
        {
            get => idle_; private set
            {
                if (idle_ != value)
                {
                    idle_ = value;
                    this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Idle)));
                }
            }
        }
        public int Delay
        {
            get => delay_; private set
            {
                if (delay_ != value)
                {
                    delay_ = value;
                    this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Delay)));
                }
            }
        }
        public int Service
        {
            get => service_; private set
            {
                if (service_ != value)
                {
                    service_ = value;
                    this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Service)));
                }
            }
        }
        public long OtherTraffic
        {
            get => other_traffic_; private set
            {
                if (other_traffic_ != value)
                {
                    other_traffic_ = value;
                    this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(OtherTraffic)));
                }
            }
        }
        public long Data10m
        {
            get => data_10m_; private set
            {
                if (data_10m_ != value)
                {
                    data_10m_ = value;
                    this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Data10m)));
                }
            }
        }
        public long Data1m
        {
            get => data_1m_; private set
            {
                if (data_1m_ != value)
                {
                    data_1m_ = value;
                    this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Data1m)));
                }
            }
        }
        public long Data10s
        {
            get => data_10s_; private set
            {
                if (data_10s_ != value)
                {
                    data_10s_ = value;
                    this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Data10s)));
                }
            }
        }

        public ServerSessionStatistic(SessionStatisticRow session)
        {
            this.partner_ = session.partner;
            this.address_ = session.address;

            this.Update(session);
        }

        public void Update(SessionStatisticRow session)
        {
            foreach (var m in session.metrics.OrderByDescending(x => x.finish))
            {
                this.MTU = m.mtu;
                this.OutputQueue = m.output_queue;
                this.InputQueue = m.input_queue;
                this.Idle = m.idle;
                this.Delay = m.delay;
                this.Service = m.service;
                this.Data10s = m.traffic.Sum(x => x.to.Sum(y => y.messages.Sum(z => z.sent)));
                this.Data1m = session.metrics.Where(x => x.finish > m.finish - 60).Sum(m => m.traffic.Sum(x => x.to.Sum(y => y.messages.Sum(z => z.sent))));
                this.Data10m = session.metrics.Where(x => x.finish > m.finish - 10 * 60).Sum(m => m.traffic.Sum(x => x.to.Sum(y => y.messages.Sum(z => z.sent))));
                break;
            }
        }
    }
}