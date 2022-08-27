using netmon.domain.Configuration;
using netmon.domain.Interfaces;
using netmon.domain.Models;

namespace netmon.domain.Handlers
{
    public class PingRequestModelFactory : IPingRequestModelFactory
    {
        private readonly PingHandlerOptions _pingHandlerOptions;

        public PingRequestModelFactory(PingHandlerOptions pingHandlerOptions)
        {
            _pingHandlerOptions = pingHandlerOptions;
        }

        public PingRequestModel Create()
        {
            return new PingRequestModel() { Options = _pingHandlerOptions };
        }
    }

}