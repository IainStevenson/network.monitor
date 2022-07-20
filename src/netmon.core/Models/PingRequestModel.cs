using netmon.core.Configuration;
using netmon.core.Data;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.NetworkInformation;
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

        public PingHandlerOptions Options { get; set; }

        public byte[] Buffer
        {
            get
            {

                string data = "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa";
                byte[] buffer = Encoding.ASCII.GetBytes(data);
                return buffer;
            }
        }
     
    }

}