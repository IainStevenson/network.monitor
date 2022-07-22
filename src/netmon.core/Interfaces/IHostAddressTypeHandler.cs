using netmon.core.Data;
using netmon.core.Models;
using System.Net;

namespace netmon.core.Interfaces
{
    public interface IHostAddressTypeHandler
    {
        HostTypes GetPrivateHostType(IPAddress address);
        HostTypes GetPublicHostType(IPAddress address, IPAddress[] addresses);
    }
}