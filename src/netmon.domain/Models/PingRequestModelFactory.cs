using netmon.domain.Configuration;
using netmon.domain.Interfaces;
using System.Net;
using netmon.domain.Data;

namespace netmon.domain.Models
{
    public class PingRequestModelFactory : IPingRequestModelFactory
    {
        private readonly PingHandlerOptions _pingHandlerOptions;

        public PingRequestModelFactory(PingHandlerOptions pingHandlerOptions)
        {
            _pingHandlerOptions = pingHandlerOptions;
        }

        public PingRequestModel Create(IPAddress target)
        {
            return new PingRequestModel()
            {
                Options = _pingHandlerOptions,
                Address = target,
                Origin = target.GetActualLocalIPAddress()
            };
        }
    }

}