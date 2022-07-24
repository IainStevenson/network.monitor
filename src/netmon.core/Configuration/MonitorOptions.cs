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
        /// The interval in milliseconds between consecutive network bandwidth tests.
        /// </summary>
        public int BandwidthTestIntereval {  get;set;} = 60000; // every hour

    }
}
