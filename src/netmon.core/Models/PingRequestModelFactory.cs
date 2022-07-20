using netmon.core.Configuration;

namespace netmon.core.Models
{
    public class PingRequestModelFactory : IPingRequestModelFactory
    {
        public PingRequestModel Create(PingHandlerOptions options)
        {
            return new PingRequestModel() { Options = options };
        }
    }

}