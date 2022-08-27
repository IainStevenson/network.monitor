using netmon.domain.Configuration;
using netmon.domain.Data;
using netmon.domain.Serialisation;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Text.Json.Serialization;

namespace netmon.domain.Models
{

    [ExcludeFromCodeCoverage]
    public class PingRequestModel
    {
       
        /// <summary>
        /// The address to ping. Default is the loopback address.
        /// </summary>
        [JsonConverter(typeof(IPAddressConverter))]
        public IPAddress Address { get; set; } = Defaults.LoopbackAddress;

        public PingHandlerOptions Options { get; set; } = new PingHandlerOptions();

        /// <summary>
        /// The data buffer to send. Which is 32 bytes long.
        /// </summary>
        public byte[] Buffer { get; set; } = Defaults.RandomBuffer;
        /// <summary>
        /// The time to live for this request. 
        /// When set to less than the default this is to facilitate a traceroute operation.
        /// </summary>
        public int Ttl { get; set; } = 128;
    }

}