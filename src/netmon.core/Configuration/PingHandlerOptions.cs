using System.Net.NetworkInformation;

namespace netmon.core.Configuration
{
    /// <summary>
    /// operating options for the <see cref="Handlers.PingHandler"/>
    /// </summary>
    public class PingHandlerOptions
    {
        /// <summary>
        /// Data should not be fragmented. Note: as the default content size is 32 Bytes this should not happen anyway. Some routers may reset this value on reply.
        /// </summary>
        public bool DontFragment { get; set; } = true;
        
        /// <summary>
        /// Millseconds allowed for a response from each router, before a Status of <see cref="IPStatus.TimedOut"/> occurs.
        /// </summary>
        public int Timeout { get; set; } = 1000;

    }
}
