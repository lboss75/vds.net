using IVySoft.VDS.Client.Api;
using IVySoft.VDS.Client.UI.Logic;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
        private long other_;
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
        public long Other
        {
            get => other_; private set
            {
                if (other_ != value)
                {
                    other_ = value;
                    this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Other)));
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

        public ObservableCollection<ServerSessionTrafficStatistic> DirectTraffic { get; } = new ObservableCollection<ServerSessionTrafficStatistic>();
        public ObservableCollection<ServerSessionTrafficStatistic> ExternalTraffic { get; } = new ObservableCollection<ServerSessionTrafficStatistic>();
        public ObservableCollection<ServerSessionTrafficStatistic> ProxyTraffic { get; } = new ObservableCollection<ServerSessionTrafficStatistic>();
        public ObservableCollection<ServerSessionTrafficStatistic> RestTraffic { get; } = new ObservableCollection<ServerSessionTrafficStatistic>();

        public ServerSessionStatistic(string server_id, SessionStatisticRow session)
        {
            this.partner_ = session.partner;
            this.address_ = session.address;

            this.Update(server_id, session);
        }

        public void Update(string server_id, SessionStatisticRow session)
        {
            var last_record = session.metrics.OrderByDescending(x => x.finish).FirstOrDefault();


            this.MTU = session.metrics.AverageOrDefault(x => x.mtu);
            this.OutputQueue = session.metrics.AverageOrDefault(x => x.output_queue);
            this.InputQueue = session.metrics.AverageOrDefault(x => x.input_queue);
            this.Idle = session.metrics.AverageOrDefault(x => x.idle);
            this.Delay = session.metrics.AverageOrDefault(x => x.delay);
            this.Service = session.metrics.AverageOrDefault(x => x.service);
            this.Other = session.metrics
                .Sum(x => x.traffic
                .SumOrDefault(y => y.to
                .Where(z => y.from != server_id || z.to != session.partner).SumOrDefault(z => z.messages
                .SumOrDefault(m => m.sent + m.rcv_good + m.rcv_bad))));
            if (null != last_record)
            {
                this.Data10s = last_record.traffic.SumOrDefault(x => x.to.Sum(y => y.messages.SumOrDefault(z => z.sent)));
                this.Data1m = session.metrics.Where(x => x.finish > last_record.finish - 60).SumOrDefault(m => m.traffic.SumOrDefault(x => x.to.SumOrDefault(y => y.messages.Sum(z => z.sent))));
                this.Data10m = session.metrics.Where(x => x.finish > last_record.finish - 10 * 60).SumOrDefault(m => m.traffic.Sum(x => x.to.SumOrDefault(y => y.messages.Sum(z => z.sent))));
            }
            Logic.CollectionUtils.Update(
                this.DirectTraffic,
                session.metrics
                .SelectMany(x => x.traffic)
                .SelectMany(x => x.to.Where(c => (x.from == server_id && c.to == session.partner) || (c.to == server_id && x.from == session.partner))
                .SelectMany(y => y.messages.Select(z => new { from = x.from, to = y.to, message = z })))
                .GroupBy(x => new { x.from, x.to, x.message.msg })
                .Select(x => new ServerSessionTrafficStatistic
                {
                    From = x.Key.from,
                    To = x.Key.to,
                    Msg = x.Key.msg,
                    Sent = x.Sum(y => y.message.sent),
                    SentCount = x.Sum(y => y.message.sent_count),
                    RcvGood = x.Sum(y => y.message.rcv_good),
                    RcvGoodCount = x.Sum(y => y.message.rcv_good_count),
                    RcvBad = x.Sum(y => y.message.rcv_bad),
                    RcvBadCount = x.Sum(y => y.message.rcv_bad_count),
                }),
                (x, y) => x.From == y.From && x.To == y.To && x.Msg == y.Msg,
                (x, y) =>
                {
                    x.From = y.From;
                    x.To = y.To;
                    x.Msg = y.Msg;
                    x.Sent = y.Sent;
                    x.SentCount = y.SentCount;
                    x.RcvGood = y.RcvGood;
                    x.RcvGoodCount = y.RcvGoodCount;
                    x.RcvBad = y.RcvBad;
                    x.RcvBadCount = y.RcvBadCount;

                },
                (y) => new ServerSessionTrafficStatistic
                {
                    From = y.From,
                    To = y.To,
                    Msg = y.Msg,
                    Sent = y.Sent,
                    SentCount = y.SentCount,
                    RcvGood = y.RcvGood,
                    RcvGoodCount = y.RcvGoodCount,
                    RcvBad = y.RcvBad,
                    RcvBadCount = y.RcvBadCount
                });

            Logic.CollectionUtils.Update(
                this.ExternalTraffic,
                session.metrics
                .SelectMany(x => x.traffic)
                .SelectMany(x => x.to.Where(c => (x.from == server_id && c.to != session.partner) || (c.to == server_id && x.from != session.partner))
                .SelectMany(y => y.messages.Select(z => new { from = x.from, to = y.to, message = z })))
                .GroupBy(x => new { x.from, x.to, x.message.msg })
                .Select(x => new ServerSessionTrafficStatistic
                {
                    From = x.Key.from,
                    To = x.Key.to,
                    Msg = x.Key.msg,
                    Sent = x.Sum(y => y.message.sent),
                    SentCount = x.Sum(y => y.message.sent_count),
                    RcvGood = x.Sum(y => y.message.rcv_good),
                    RcvGoodCount = x.Sum(y => y.message.rcv_good_count),
                    RcvBad = x.Sum(y => y.message.rcv_bad),
                    RcvBadCount = x.Sum(y => y.message.rcv_bad_count),
                }),
                (x, y) => x.From == y.From && x.To == y.To && x.Msg == y.Msg,
                (x, y) =>
                {
                    x.From = y.From;
                    x.To = y.To;
                    x.Msg = y.Msg;
                    x.Sent = y.Sent;
                    x.SentCount = y.SentCount;
                    x.RcvGood = y.RcvGood;
                    x.RcvGoodCount = y.RcvGoodCount;
                    x.RcvBad = y.RcvBad;
                    x.RcvBadCount = y.RcvBadCount;

                },
                (y) => new ServerSessionTrafficStatistic
                {
                    From = y.From,
                    To = y.To,
                    Msg = y.Msg,
                    Sent = y.Sent,
                    SentCount = y.SentCount,
                    RcvGood = y.RcvGood,
                    RcvGoodCount = y.RcvGoodCount,
                    RcvBad = y.RcvBad,
                    RcvBadCount = y.RcvBadCount
                });
            Logic.CollectionUtils.Update(
                this.ProxyTraffic,
                session.metrics
                .SelectMany(x => x.traffic)
                .SelectMany(x => x.to.Where(c => (x.from != server_id && c.to == session.partner) || (c.to != server_id && x.from == session.partner))
                .SelectMany(y => y.messages.Select(z => new { from = x.from, to = y.to, message = z })))
                .GroupBy(x => new { x.from, x.to, x.message.msg })
                .Select(x => new ServerSessionTrafficStatistic
                {
                    From = x.Key.from,
                    To = x.Key.to,
                    Msg = x.Key.msg,
                    Sent = x.Sum(y => y.message.sent),
                    SentCount = x.Sum(y => y.message.sent_count),
                    RcvGood = x.Sum(y => y.message.rcv_good),
                    RcvGoodCount = x.Sum(y => y.message.rcv_good_count),
                    RcvBad = x.Sum(y => y.message.rcv_bad),
                    RcvBadCount = x.Sum(y => y.message.rcv_bad_count),
                }),
                (x, y) => x.From == y.From && x.To == y.To && x.Msg == y.Msg,
                (x, y) =>
                {
                    x.From = y.From;
                    x.To = y.To;
                    x.Msg = y.Msg;
                    x.Sent = y.Sent;
                    x.SentCount = y.SentCount;
                    x.RcvGood = y.RcvGood;
                    x.RcvGoodCount = y.RcvGoodCount;
                    x.RcvBad = y.RcvBad;
                    x.RcvBadCount = y.RcvBadCount;

                },
                (y) => new ServerSessionTrafficStatistic
                {
                    From = y.From,
                    To = y.To,
                    Msg = y.Msg,
                    Sent = y.Sent,
                    SentCount = y.SentCount,
                    RcvGood = y.RcvGood,
                    RcvGoodCount = y.RcvGoodCount,
                    RcvBad = y.RcvBad,
                    RcvBadCount = y.RcvBadCount
                });

            Logic.CollectionUtils.Update(
                this.RestTraffic,
                session.metrics
                .SelectMany(x => x.traffic)
                .SelectMany(x => x.to.Where(c => (x.from != server_id && c.to == session.partner))
                .SelectMany(y => y.messages.Select(z => new { from = x.from, to = y.to, message = z })))
                .GroupBy(x => new { x.from, x.to, x.message.msg })
                .Select(x => new ServerSessionTrafficStatistic
                {
                    From = x.Key.from,
                    To = x.Key.to,
                    Msg = x.Key.msg,
                    Sent = x.Sum(y => y.message.sent),
                    SentCount = x.Sum(y => y.message.sent_count),
                    RcvGood = x.Sum(y => y.message.rcv_good),
                    RcvGoodCount = x.Sum(y => y.message.rcv_good_count),
                    RcvBad = x.Sum(y => y.message.rcv_bad),
                    RcvBadCount = x.Sum(y => y.message.rcv_bad_count),
                }),
                (x, y) => x.From == y.From && x.To == y.To && x.Msg == y.Msg,
                (x, y) =>
                {
                    x.From = y.From;
                    x.To = y.To;
                    x.Msg = y.Msg;
                    x.Sent = y.Sent;
                    x.SentCount = y.SentCount;
                    x.RcvGood = y.RcvGood;
                    x.RcvGoodCount = y.RcvGoodCount;
                    x.RcvBad = y.RcvBad;
                    x.RcvBadCount = y.RcvBadCount;

                },
                (y) => new ServerSessionTrafficStatistic
                {
                    From = y.From,
                    To = y.To,
                    Msg = y.Msg,
                    Sent = y.Sent,
                    SentCount = y.SentCount,
                    RcvGood = y.RcvGood,
                    RcvGoodCount = y.RcvGoodCount,
                    RcvBad = y.RcvBad,
                    RcvBadCount = y.RcvBadCount
                });
        }
    }
}