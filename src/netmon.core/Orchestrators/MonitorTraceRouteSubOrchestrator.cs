using Microsoft.Extensions.Logging;
using netmon.core.Data;
using netmon.core.Interfaces;
using netmon.core.Messaging;
using netmon.core.Models;
using System.Net;

namespace netmon.core.Orchestrators
{
    public class MonitorTraceRouteSubOrchestrator : IMonitorSubOrchestrator
    {
        private readonly IStorageOrchestrator<PingResponseModel> _pingResponseModelStorageOrchestrator;
        private readonly ITraceRouteOrchestrator _traceRouteOrchestrator;
        private readonly ILogger<MonitorTraceRouteSubOrchestrator> _logger;

        public event EventHandler<SubOrchestratorEventArgs>? Reset; // TODO, Address this perhaps as another interface

        public MonitorTraceRouteSubOrchestrator(IStorageOrchestrator<PingResponseModel> pingResponseModelStorageOrchestrator,
                ITraceRouteOrchestrator traceRouteOrchestrator,
                ILogger<MonitorTraceRouteSubOrchestrator> logger)
        {
            _pingResponseModelStorageOrchestrator = pingResponseModelStorageOrchestrator;
            _traceRouteOrchestrator = traceRouteOrchestrator;
            _logger = logger;
        }

        public async Task Handle(List<IPAddress> addressesToMonitor, TimeSpan until, CancellationToken cancellationToken)
        {
            _traceRouteOrchestrator.Results += StoreResutlsAsTheyComeIn;

            while (!cancellationToken.IsCancellationRequested)
            {
                _ = await ValidateAddressesByTraceRoute(addressesToMonitor, cancellationToken);
            }

            _traceRouteOrchestrator.Results -= StoreResutlsAsTheyComeIn;
        }


        private void StoreResutlsAsTheyComeIn(object? source, PingResponseModelEventArgs? e)
        {
            if (e == null) return;

            _pingResponseModelStorageOrchestrator.Store(e.Model).Wait();
        }
        private async Task<List<IPAddress>> ValidateAddressesByTraceRoute(List<IPAddress> requestedAddresses, CancellationToken cancellationToken)
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
        private async Task<List<IPAddress>> GetAddressesToMonitorFromTraceRoute(IPAddress addressToTrace, CancellationToken cancellationToken)
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