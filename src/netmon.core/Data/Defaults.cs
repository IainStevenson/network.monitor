using netmon.core.Serialisation;
using System.Net;
using System.Net.NetworkInformation;

namespace netmon.core.Data
{

    /// <summary>
    /// Solution default values.
    /// </summary>
    public static class Defaults
    {
        /// <summary>
        /// The standard loopback IP address which always means 'this' host.
        /// NOTE: If you set this to <see cref="IPAddress.Loopback"/> then it will not serialize due to ScopeId. 
        /// Is this a bug in <see cref="IPAddressConverter"/> ???
        /// </summary>
        public readonly static IPAddress LoopbackAddress =  IPAddress.Parse("127.0.0.1");

        /// <summary>
        /// Default Ping options.
        /// </summary>
        public readonly static PingOptions PingOptions = new() { DontFragment = true, Ttl =128};

        public const int Ttl = 128;

        public readonly static IPAddress DefaultMonitoringDestination = IPAddress.Parse("8.8.8.8");

    }
}