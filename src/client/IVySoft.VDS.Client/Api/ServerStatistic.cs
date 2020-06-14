using System;
using System.Collections.Generic;
using System.Text;

namespace IVySoft.VDS.Client.Api
{
    public class ServerStatistic
    {
        public int db_queue_length { get; set; }
        public string current_user_folder { get; set; }
        public string local_machine_folder { get; set; }
        public RouteStatistic route { get; set; }
        public SessionStatistic session { get; set; }

    }
}
