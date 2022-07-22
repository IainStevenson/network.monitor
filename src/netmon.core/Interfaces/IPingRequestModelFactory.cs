using netmon.core.Configuration;
using netmon.core.Models;

namespace netmon.core.Interfaces
{
    public interface IPingRequestModelFactory
    {
        
        PingRequestModel Create();
    }
}