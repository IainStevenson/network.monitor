using netmon.core.Data;
using netmon.core.Serialisation;
using System.Net;
using System.Text.Json.Serialization;

namespace netmon.core.Configuration
{
    public class MonitorOptions
    {
        /// <summary>
        /// The monitoring target. Defaults to dns.google.com
        /// </summary>
        [JsonConverter(typeof(IPAddressConverter))] public IPAddress Destination { get; set; } = Defaults.DefaultMonitoringDestination;
        /// <summary>
        /// The interval in milliseconds between consecutive ping requests for any host.
        /// </summary>
        public int PingInterval { get; set; } = 5000; // every 5 seconds
        /// <summary>
        /// The interval in milliseconds between consecutive network bandwidth tests.
        /// </summary>
        public int BandwidthIntereval {  get;set;} = 60000; // every hour

        /// <summary>
        /// Declares the session as Roaming. 
        /// If this has changed from the last monitoring session it invalidates the previous monitor configuration and re-configures using trace route
        /// </summary>
        public bool Roaming { get;  set; }
    }
}
