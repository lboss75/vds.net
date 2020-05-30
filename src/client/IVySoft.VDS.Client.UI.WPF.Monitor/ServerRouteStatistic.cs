using IVySoft.VDS.Client.Api;
using System;
using System.ComponentModel;

namespace IVySoft.VDS.Client.UI.WPF.Monitor
{
    public class ServerRouteStatistic : INotifyPropertyChanged
    {
        private readonly string node_id_;
        private readonly string proxy_;

        private long pinged_;
        private long hops_;


        public string NodeId { get => node_id_; }
        public string Proxy { get => proxy_; }

        public long Pinged
        {
            get => pinged_;
            set
            {
                if(this.pinged_ != value)
                {
                    this.pinged_ = value;
                    this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Pinged)));
                }
            }
        }
        public long Hops
        {
            get => hops_;
            set
            {
                if (this.hops_ != value)
                {
                    this.hops_ = value;
                    this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Hops)));
                }
            }
        }

        public ServerRouteStatistic(RouteStatisticItem route)
        {
            this.node_id_ = route.node_id;
            this.proxy_ = route.proxy;

            this.Update(route);
        }

        internal void Update(RouteStatisticItem route)
        {
            this.Pinged = route.pinged;
            this.Hops = route.hops;
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}