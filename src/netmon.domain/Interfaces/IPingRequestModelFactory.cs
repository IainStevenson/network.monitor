using netmon.domain.Models;
using System.Net;

namespace netmon.domain.Interfaces
{
    public interface IPingRequestModelFactory
    {        
        PingRequestModel Create(IPAddress target);
    }
}