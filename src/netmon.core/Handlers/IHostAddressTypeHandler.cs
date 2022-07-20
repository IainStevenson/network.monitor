using netmon.core.Models;
using System.Net;

namespace netmon.core.Handlers
{
    public interface IHostAddressTypeHandler
    {
        HostTypes GetPrivateHostType(IPAddress address);
        HostTypes GetPublicHostType(IPAddress address, IPAddress[] addresses);
    }
}