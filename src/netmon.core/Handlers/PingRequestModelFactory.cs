using netmon.core.Configuration;
using netmon.core.Interfaces;
using netmon.core.Models;

namespace netmon.core.Handlers
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