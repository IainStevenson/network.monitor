using Microsoft.Extensions.Logging;
using netmon.core.Interfaces;
using netmon.core.Models;
using System.Net;

namespace netmon.core.Orchestrators
{
    public class MonitorPingSubOrchestrator : MonitorBaseSubOrchestrator
    {
        public MonitorPingSubOrchestrator(IStorageOrchestrator<PingResponseModel> pingResponseModelStorageOrchestrator,
                ITraceRouteOrchestrator traceRouteOrchestrator,
                IPingOrchestrator pingOrchestrator,
               ILogger<MonitorBaseSubOrchestrator> logger) : base(pingResponseModelStorageOrchestrator, traceRouteOrchestrator, pingOrchestrator, logger)
        { }

        public async override Task Handle(List<IPAddress> addressesToMonitor, TimeSpan until, CancellationToken cancellationToken)
        {
            _pingOrchestrator.Results += StoreResutlsAsTheyComeIn;
            while (!cancellationToken.IsCancellationRequested)
            {
                _ = await _pingOrchestrator.PingUntil(addressesToMonitor.ToArray(), until, cancellationToken);
            }
            _pingOrchestrator.Results -= StoreResutlsAsTheyComeIn;
        }


    }
}