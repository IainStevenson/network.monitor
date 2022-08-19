using DnsClient.Internal;
using Microsoft.Extensions.Logging;
using netmon.core.Configuration;
using netmon.core.Data;
using netmon.core.Interfaces;
using netmon.core.Messaging;
using netmon.core.Models;
using System.Net;

namespace netmon.core.Orchestrators
{
    public abstract class MonitorBaseSubOrchestrator : IMonitorSubOrchestrator
    {
        protected readonly IPingOrchestrator _pingOrchestrator;
        protected readonly IStorageOrchestrator<PingResponseModel> _pingResponseModelStorageOrchestrator;
        protected readonly ITraceRouteOrchestrator _traceRouteOrchestrator;
        protected readonly ILogger<MonitorBaseSubOrchestrator> _logger;
        public MonitorBaseSubOrchestrator(IStorageOrchestrator<PingResponseModel> pingResponseModelStorageOrchestrator,
                ITraceRouteOrchestrator traceRouteOrchestrator,
                IPingOrchestrator pingOrchestrator,
               ILogger<MonitorBaseSubOrchestrator> logger)
        {
            _pingResponseModelStorageOrchestrator = pingResponseModelStorageOrchestrator;
            _traceRouteOrchestrator = traceRouteOrchestrator;
            _pingOrchestrator = pingOrchestrator;
            _logger = logger;
        }

        public virtual Task Handle(List<IPAddress> addressesToMonitor, TimeSpan until, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        protected void StoreResutlsAsTheyComeIn(object? source, PingResponseModelEventArgs? e)
        {
            if (e == null) return;

            _pingResponseModelStorageOrchestrator.Store(e.Model).Wait();
        }
        protected async Task<List<IPAddress>> ValidateAddressesByTraceRoute(List<IPAddress> requestedAddresses, CancellationToken cancellationToken)
        {

            _logger.LogTrace("Validating [{count}] addresses", requestedAddresses.Count);

            var addressesToMonitor = requestedAddresses.Distinct().ToArray().ToList();


            if (!addressesToMonitor.Any())
            {
                _logger.LogTrace("No addresses specified, initialising to default monitoring address {address}", Defaults.DefaultMonitoringDestination);
                addressesToMonitor = await GetAddressesToMonitorFromTraceRoute(Defaults.DefaultMonitoringDestination, cancellationToken);
            }

            _logger.LogTrace("Tracing route to specified addresses {count}", addressesToMonitor.Count);
            List<IPAddress> tracedAddresses = new();
            foreach (var address in addressesToMonitor)
            {
                var routeaddresses = await GetAddressesToMonitorFromTraceRoute(address, cancellationToken);
                tracedAddresses.AddRange(routeaddresses);
            }
            addressesToMonitor.AddRange(tracedAddresses);
            addressesToMonitor = addressesToMonitor.Distinct().ToList();


            return addressesToMonitor;
        }
        protected async Task<List<IPAddress>> GetAddressesToMonitorFromTraceRoute(IPAddress addressToTrace, CancellationToken cancellationToken)
        {

            PingResponses tracedRoutes = await _traceRouteOrchestrator.Execute(addressToTrace, cancellationToken);

            var validHosts = tracedRoutes.AsOrderedList()
                                        .Where(w => w.Response != null && w.Response.Status == System.Net.NetworkInformation.IPStatus.Success)
                                        .Select(s => s.Response?.Address ?? IPAddress.Loopback)
                                        .Distinct()
                                        .ToList();

            return validHosts;
        }

    }
}