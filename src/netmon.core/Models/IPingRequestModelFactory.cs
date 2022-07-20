using netmon.core.Configuration;

namespace netmon.core.Models
{
    public interface IPingRequestModelFactory
    {
        PingRequestModel Create(PingHandlerOptions options);
    }
}