using System.Diagnostics.CodeAnalysis;
using System.Net;

namespace netmon.domain.Configuration
{
    /// <summary>
    /// The options for monitoring a range of <see cref="IPAddress"/>'s.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class MonitorOptions
    {
        /// <summary>
        /// The interval in milliseconds between consecutive network bandwidth tests.
        /// </summary>
        public int BandwidthTestInterval { get; set; } = 60000; // every hour

    }
}
