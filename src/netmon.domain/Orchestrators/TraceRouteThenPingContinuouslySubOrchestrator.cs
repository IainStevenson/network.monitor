using Microsoft.Extensions.Logging;
using netmon.domain.Data;
using netmon.domain.Interfaces;
using netmon.domain.Messaging;
using System.Net;

namespace netmon.domain.Orchestrators
{
    public class TraceRouteThenPingContinuouslyOrchestrator : IMonitorModeOrchestrator
    {

        private const long ResetThresholdCount = 10;
        private readonly IPingOrchestrator _pingOrchestrator;
        private readonly ITraceRouteOrchestrator _traceRouteOrchestrator;
        private readonly ILogger<TraceRouteThenPingContinuouslyOrchestrator> _logger;

        public TraceRouteThenPingContinuouslyOrchestrator(
                ITraceRouteOrchestrator traceRouteOrchestrator,
                IPingOrchestrator pingOrchestrator,
               ILogger<TraceRouteThenPingContinuouslyOrchestrator> logger)
        {
            _traceRouteOrchestrator = traceRouteOrchestrator;
            _pingOrchestrator = pingOrchestrator;
            _logger = logger;
        }
        
        public async  Task Execute(List<IPAddress> addressesToMonitor, TimeSpan until, CancellationToken cancellationToken)
        {
            addressesToMonitor = await GetPingAddressesByTraceRoute(addressesToMonitor, cancellationToken);

            _ = await _pingOrchestrator.PingManyUntil(addressesToMonitor.ToArray(),
                                                 until,
                                                 cancellationToken);
           
        }

        private async Task<List<IPAddress>> GetPingAddressesByTraceRoute(List<IPAddress> requestedAddresses, CancellationToken cancellationToken)
        {

            _logger.LogTrace("Validating [{count}] addresses", requestedAddresses.Count);

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