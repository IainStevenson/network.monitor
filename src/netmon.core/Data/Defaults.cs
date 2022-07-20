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
        public static IPAddress LoopbackAddress = IPAddress.Parse("127.0.0.1");

        /// <summary>
        /// Default Ping options.
        /// </summary>
        public static PingOptions PingOptions = new PingOptions() { DontFragment = true, Ttl =128};

        public static int Ttl = 128;
    }
}