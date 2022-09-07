using Microsoft.Extensions.Logging;
using netmon.domain.Data;
using netmon.domain.Interfaces;
using netmon.domain.Messaging;
using System.Net;

namespace netmon.domain.Orchestrators
{
    public class TraceRouteContinuouslyOrchestrator : IMonitorModeOrchestrator
    {
        private readonly ITraceRouteOrchestrator _traceRouteOrchestrator;
        private readonly ILogger<TraceRouteContinuouslyOrchestrator> _logger;
        
        public TraceRouteContinuouslyOrchestrator(
                ITraceRouteOrchestrator traceRouteOrchestrator,
                ILogger<TraceRouteContinuouslyOrchestrator> logger)
        {
            _traceRouteOrchestrator = traceRouteOrchestrator;
            _logger = logger;
        }

        public async Task Execute(List<IPAddress> addressesToMonitor, TimeSpan until, CancellationToken cancellationToken)
        {
            var untilThen = DateTime.UtcNow.Add(until);

            while (!cancellationToken.IsCancellationRequested && untilThen > DateTimeOffset.UtcNow )
            {
                _ = await ValidateAddressesByTraceRoute(addressesToMonitor, cancellationToken);
                Thread.Sleep(10000);
            }
        }

        private async Task<List<IPAddress>> ValidateAddressesByTraceRoute(List<IPAddress> requestedAddresses, CancellationToken cancellationToken)
        {            
            var addressesToMonitor = requestedAddresses.Distinct().ToArray().ToList();

            if (!addressesToMonitor.Any())
            {
                _logger.LogTrace("No addresses specified, initialising to default monitoring address {address}", Defaults.DefaultMonitoringDestination);
                addressesToMonitor.Add(Defaults.DefaultMonitoringDestination);
            }

            _logger.LogTrace("Tracing route to specified addresses {count}", addressesToMonitor.Count);

            List<IPAddress> tracedAddresses = new();
            foreach (var address in addressesToMonitor)
            {
                var routeaddresses = await TraceRoute(address, cancellationToken);
                tracedAddresses.AddRange(routeaddresses);
            }

            addressesToMonitor.AddRange(tracedAddresses);
            addressesToMonitor = addressesToMonitor.Distinct().ToList();

            return addressesToMonitor;
        }
        private async Task<List<IPAddress>> TraceRoute(IPAddress addressToTrace, CancellationToken cancellationToken)
        {

            PingResponseModels tracedRoutes = await _traceRouteOrchestrator.Execute(addressToTrace, cancellationToken);

            var validHosts = tracedRoutes.AsOrderedList()
                                        .Where(w => w.Response != null && w.Response.Status == System.Net.NetworkInformation.IPStatus.Success)
                                        .Select(s => s.Response?.Address ?? IPAddress.Loopback)
                                        .Distinct()
                                        .ToList();

            return validHosts;
        }


    }
}