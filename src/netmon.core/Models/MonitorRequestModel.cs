using netmon.core.Data;
using netmon.core.Serialisation;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Text.Json.Serialization;

namespace netmon.core.Models
{
    [ExcludeFromCodeCoverage]
    public class MonitorRequestModel
    {
        [JsonConverter(typeof(IPAddressConverter))] 
        public IPAddress Destination { get; set; } = Defaults.DefaultMonitoringDestination;
        [JsonConverter(typeof(HostAdddresAndTypeConverter))]
        public Dictionary<IPAddress, HostTypes> Hosts { get; set; } = new Dictionary<IPAddress, HostTypes>();
        [JsonConverter(typeof(IPAddressConverter))]
        public List<IPAddress> LocalHosts { get; set; } = new List<IPAddress>();
    }
}
