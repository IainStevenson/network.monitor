using netmon.core.Data;
using System.Net;

namespace netmon.core.Models
{
    public class MonitorModel
    {
        public IPAddress Destination {  get;set;} = Defaults.DefaultMonitoringDestination;
        public Dictionary<IPAddress, HostTypes> Hosts { get; set; } = new Dictionary<IPAddress, HostTypes>();
        public List<IPAddress> LocalHosts { get; set; } = new List<IPAddress>();
    }
}
