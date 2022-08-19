using Microsoft.Extensions.Logging;
using netmon.core.Interfaces;
using netmon.core.Models;
using System.Net;

namespace netmon.core.Orchestrators
{
    public class MonitorTraceRouteSubOrchestrator : MonitorBaseSubOrchestrator
    {

        public MonitorTraceRouteSubOrchestrator(IStorageOrchestrator<PingResponseModel> pingResponseModelStorageOrchestrator,
                ITraceRouteOrchestrator traceRouteOrchestrator,
                IPingOrchestrator pingOrchestrator,
               ILogger<MonitorBaseSubOrchestrator> logger) : base(pingResponseModelStorageOrchestrator, traceRouteOrchestrator, pingOrchestrator, logger)
        { }

        public async override Task Handle(List<IPAddress> addressesToMonitor, TimeSpan until, CancellationToken cancellationToken)
        {
            _traceRouteOrchestrator.Results += StoreResutlsAsTheyComeIn;

            while (!cancellationToken.IsCancellationRequested)
            {
                _ = await ValidateAddressesByTraceRoute(addressesToMonitor,  cancellationToken);
            }

            _traceRouteOrchestrator.Results -= StoreResutlsAsTheyComeIn;
        }

    }
}