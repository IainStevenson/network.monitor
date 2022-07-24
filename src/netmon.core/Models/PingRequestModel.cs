using netmon.core.Configuration;
using netmon.core.Data;
using netmon.core.Serialisation;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Text;
using System.Text.Json.Serialization;

namespace netmon.core.Models
{

    [ExcludeFromCodeCoverage]
    public class PingRequestModel
    {
        /// <summary>
        /// The address to ping. Default is the loopback address.
        /// </summary>
        [JsonConverter(typeof(IPAddressConverter))]
        public IPAddress Address { get; internal set; } = Defaults.LoopbackAddress;

        public PingHandlerOptions Options { get; set; } = new PingHandlerOptions();

        /// <summary>
        /// The data buffer to send. Which is 32 bytes long.
        /// </summary>
        public static byte[] Buffer
        {
            get
            {
                string data = "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa";
                byte[] buffer = Encoding.ASCII.GetBytes(data);
                return buffer;
            }
        }
        /// <summary>
        /// The time to live for this request. 
        /// When set to less than the default this is to facilitate a traceroute operation.
        /// </summary>
        public int Ttl { get;  set; } = 128;
    }

}