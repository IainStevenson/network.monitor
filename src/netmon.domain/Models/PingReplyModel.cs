using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.NetworkInformation;

namespace netmon.domain.Models
{
    /// <summary>
    /// A property based abstraction of the <see cref="System.Net.NetworkInformation.PingReply"/> class.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class PingReplyModel
    {
        public PingReplyModel()
        {
            Address = IPAddress.Parse("127.0.0.1");
            Buffer = Array.Empty<byte>();
        }
        //
        // Summary:
        //     Gets the address of the host that sends the Internet Control Message Protocol
        //     (ICMP) echo reply.
        //
        // Returns:
        //     An System.Net.IPAddress containing the destination for the ICMP echo message.
        public IPAddress Address { get; set;}
        //
        // Summary:
        //     Gets the buffer of data received in an Internet Control Message Protocol (ICMP)
        //     echo reply message.
        //
        // Returns:
        //     A System.Byte array containing the data received in an ICMP echo reply message,
        //     or an empty array, if no reply was received.
        public byte[] Buffer { get; set; }
        //
        // Summary:
        //     Gets the options used to transmit the reply to an Internet Control Message Protocol
        //     (ICMP) echo request.
        //
        // Returns:
        //     A System.Net.NetworkInformation.PingOptions object that contains the Time to
        //     Live (TTL) and the fragmentation directive used for transmitting the reply if
        //     System.Net.NetworkInformation.PingReply.Status is System.Net.NetworkInformation.IPStatus.Success;
        //     otherwise, null.
        public PingOptions? Options { get; set; }
        //
        // Summary:
        //     Gets the number of milliseconds taken to send an Internet Control Message Protocol
        //     (ICMP) echo request and receive the corresponding ICMP echo reply message.
        //
        // Returns:
        //     An System.Int64 that specifies the round trip time, in milliseconds.
        public long RoundtripTime { get; set; }
        //
        // Summary:
        //     Gets the status of an attempt to send an Internet Control Message Protocol (ICMP)
        //     echo request and receive the corresponding ICMP echo reply message.
        //
        // Returns:
        //     An System.Net.NetworkInformation.IPStatus value indicating the result of the
        //     request.
        public IPStatus Status { get; set; }
    }
}
