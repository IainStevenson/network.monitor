using netmon.domain.Models;
using System.Net;

namespace netmon.domain.Interfaces
{
    public interface IHostAddressTypeHandler
    {
        HostTypes GetPrivateHostType(IPAddress address);
        HostTypes GetPublicHostType(IPAddress address, IPAddress[] addresses);
    }
}