using netmon.core.Data;
using netmon.core.Serialisation;
using System.Net;
using System.Text.Json.Serialization;

namespace netmon.core.Configuration
{
    /// <summary>
    /// The options for monitoring a range of <see cref="IPAddress"/>'s.
    /// </summary>
    public class MonitorOptions
    {
        /// <summary>
        /// The monitoring target. Defaults to dns.google.com
        /// </summary>
        [JsonConverter(typeof(IPAddressConverter))] 
        public IPAddress Destination { get; set; } = Defaults.DefaultMonitoringDestination;
        /// <summary>
        /// The interval in milliseconds between consecutive ping requests for any host.
        /// </summary>
        public int PingInterval { get; set; } = 5000; // every 5 seconds
        /// <summary>
        /// The interval in milliseconds between consecutive network bandwidth tests.
        /// </summary>
        public int BandwidthTestIntereval {  get;set;} = 60000; // every hour

        /// <summary>
        /// Declares the current base address to determine if the next session is roaming or not.
        /// </summary>
        public IPAddress BaseAddress {  get;set; }  = IPAddress.Parse("127.0.0.1");
    }
}
