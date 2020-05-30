using IVySoft.VDS.Client.Api;
using IVySoft.VDS.Client.UI.Logic;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Documents;

namespace IVySoft.VDS.Client.UI.WPF.Monitor
{
    public class ServerStatisticRow
    {
        private readonly string service_uri;
        private readonly string node_id_;

        public string ServiceUri { get => service_uri; }
        public string NodeId { get => node_id_; }

        public ObservableCollection<ServerSessionStatistic> Sessions { get; } = new ObservableCollection<ServerSessionStatistic>();
        public ObservableCollection<ServerRouteStatistic> Route { get; } = new ObservableCollection<ServerRouteStatistic>();

        public ServerStatisticRow(string service_uri, ServerStatistic stat)
        {
            this.service_uri = service_uri;
            this.node_id_ = stat.route.node_id;
            
            foreach(var session in stat.session.items)
            {
                this.Sessions.Add(new ServerSessionStatistic(session));
            }
            foreach (var route in stat.route.items)
            {
                this.Route.Add(new ServerRouteStatistic(route));
            }
        }

        internal void Update(ServerStatistic stat)
        {
            CollectionUtils.Update(
                this.Sessions,
                stat.session.items,
                (x, y) => x.Partner == y.partner,
                (x, y) => x.Update(y),
                x => new ServerSessionStatistic(x));

            CollectionUtils.Update(
                this.Route,
                stat.route.items,
                (x, y) => x.NodeId == y.node_id && x.Proxy == y.proxy,
                (x, y) => x.Update(y),
                x => new ServerRouteStatistic(x));
        }
    }
}