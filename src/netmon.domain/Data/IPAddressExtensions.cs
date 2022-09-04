using System.Net;
using System.Net.Sockets;

namespace netmon.domain.Data
{
    public static class IPAddressExtensions
    {
        ///// <summary>
        ///// Adapted from answer in: https://stackoverflow.com/questions/6803073/get-local-ip-address
        ///// </summary>
        ///// <returns></returns>

        public static IPAddress? GetActualLocalIPAddress(this IPAddress forAddress)
        {
            using Socket socket = new(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.IP);
            socket.Connect(forAddress, 65530);
            return (socket.LocalEndPoint as IPEndPoint)?.Address;
        }
    }

}