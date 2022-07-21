using netmon.core.Data;
using netmon.core.Models;
using System.Net;

namespace netmon.core.Handlers
{
    /// <summary>
    /// With reference to: https://en.wikipedia.org/wiki/Reserved_IP_addresses
    /// </summary>
    public class HostAddressTypeHandler : IHostAddressTypeHandler
    {

        /// <summary>
        /// Determines if the <see cref="IPAddress"/> is a private address or not.
        /// </summary>
        /// <param name="address">The address to test.</param>
        /// <param name="addresses"></param>
        /// <returns></returns>
        public HostTypes GetPrivateHostType(IPAddress address)
        {

            var isPrivateAddress = IsAPrivateAddress(address);  
            return (isPrivateAddress ? HostTypes.Private : HostTypes.Public);
        }

        public HostTypes GetPublicHostType(IPAddress address, IPAddress[] addresses)
        {
            bool isIsp = false;
                isIsp = addresses.Where(x => !IsAPrivateAddress(x))
                .FirstOrDefault() == address; // first after the local addresses in the list.
           

            return isIsp? HostTypes.Isp: IsAPrivateAddress(address) ? HostTypes.Private : HostTypes.Public;
        }


        private static bool IsAPrivateAddress(IPAddress address)  => 
            address.IsInSubnet("192.0.0.0/24")
                        || address.IsInSubnet("192.168.0.0/16")
                        //|| address.IsInSubnet("198.18.0.0/15")
                        || address.IsInSubnet("172.16.0/12")
                        //|| address.IsInSubnet("100.64.0.0/10")
                        || address.IsInSubnet("10.0.0.0/8")
                        || address.IsInSubnet("0.0.0.0/8")
                            ;
        
    }
}