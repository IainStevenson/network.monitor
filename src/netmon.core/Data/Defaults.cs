using netmon.core.Serialisation;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;

namespace netmon.core.Data
{

    /// <summary>
    /// Solution default values.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public static class Defaults
    {
        /// <summary>
        /// The standard loopback IP address which always means 'this' host.
        /// NOTE: If you set this to <see cref="IPAddress.Loopback"/> then it will not serialize due to ScopeId. 
        /// Is this a bug in <see cref="IPAddressConverter"/> ???
        /// </summary>
        public readonly static IPAddress LoopbackAddress =  IPAddress.Parse("127.0.0.1");       

        /// <summary>
        /// Allow up 127 router traversals.
        /// </summary>
        public readonly static int Ttl = 128;

        /// <summary>
        /// The default address to monitor if no addresses are nominated. 
        /// This is the google dns address which is known to respond to ICMP messages.
        /// </summary>
        public readonly static IPAddress DefaultMonitoringDestination = IPAddress.Parse("8.8.8.8");


        /// <summary>
        /// The data buffer to send. Which is 32 bytes long.
        /// </summary>
        public static byte[] RandomBuffer
        {
            get
            {
                string data = Guid.NewGuid().ToString().Replace("-", ""); ;
                byte[] buffer = Encoding.ASCII.GetBytes(data);
                return buffer;
            }
        }
    }
}