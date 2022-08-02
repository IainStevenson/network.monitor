using System.Diagnostics;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;

namespace netmon.core.Handlers
{
    
    public static class RawSocketPermissions
    {
        private static readonly Lazy<bool> s_canUseRawIPv4Sockets = new(() => CheckRawSocketPermissions(AddressFamily.InterNetwork));
        private static readonly Lazy<bool> s_canUseRawIPv6Sockets = new(() => CheckRawSocketPermissions(AddressFamily.InterNetworkV6));

        /// <summary>
        /// Returns whether or not the current user has the necessary permission to open raw sockets.
        /// </summary>
        public static bool CanUseRawSockets(AddressFamily addressFamily) =>
            addressFamily == AddressFamily.InterNetworkV6 ?
                s_canUseRawIPv6Sockets.Value :
                s_canUseRawIPv4Sockets.Value;

        public static bool CheckRawSocketPermissions(AddressFamily addressFamily)
        {
            try
            {
                new Socket(addressFamily, SocketType.Raw, addressFamily == AddressFamily.InterNetwork ? ProtocolType.Icmp : ProtocolType.IcmpV6).Dispose();
                return true;
            }
            catch
            {
                return false;
            }
        }

        
    }

}