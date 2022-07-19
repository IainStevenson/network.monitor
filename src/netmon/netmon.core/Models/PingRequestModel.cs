using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;

namespace netmon.core.Models
{
    [ExcludeFromCodeCoverage]
    public class PingRequestModel
    {
        /// <summary>
        /// the address to ping. Default is the lopback address.
        /// </summary>
        public IPAddress Address { get; internal set; } = new IPAddress(new byte[] { 0x7f, 0x0, 0x0, 0x1 });
        public PingOptions Options { get; internal set; } = new PingOptions() { DontFragment = true };

        /// <summary>
        /// Allowed milliseconds to complete, default is 1 second
        /// </summary>
        public int Timeout { get; set; } = 1000;
        public byte[] Buffer { get {

                string data = "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa";
                byte[] buffer = Encoding.ASCII.GetBytes(data); 
                return buffer;
            }
            }
    }

}