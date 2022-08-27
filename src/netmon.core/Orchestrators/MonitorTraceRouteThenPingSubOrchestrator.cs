using Microsoft.Extensions.Logging;
using netmon.core.Data;
using netmon.core.Interfaces;
using netmon.core.Messaging;
using netmon.core.Models;
using System.Collections.Concurrent;
using System.Net;

namespace netmon.core.Orchestrators
{
    public class MonitorTraceRouteThenPingSubOrchestrator : IMonitorSubOrchestrator
    {

        private const long ResetThresholdCount = 10;
        private readonly IPingOrchestrator _pingOrchestrator;
        private readonly IStorageOrchestrator<PingResponseModel> _pingResponseModelStorageOrchestrator;
        private readonly ITraceRouteOrchestrator _traceRouteOrchestrator;
        private readonly ILogger<MonitorTraceRouteThenPingSubOrchestrator> _logger;

        public MonitorTraceRouteThenPingSubOrchestrator(IStorageOrchestrator<PingResponseModel> pingResponseModelStorageOrchestrator,
                ITraceRouteOrchestrator traceRouteOrchestrator,
                IPingOrchestrator pingOrchestrator,
               ILogger<MonitorTraceRouteThenPingSubOrchestrator> logger)
        {
            _pingResponseModelStorageOrchestrator = pingResponseModelStorageOrchestrator;
            _traceRouteOrchestrator = traceRouteOrchestrator;
            _pingOrchestrator = pingOrchestrator;
            _logger = logger;
        }
        
        public async  Task Execute(List<IPAddress> addressesToMonitor, TimeSpan until, CancellationToken cancellationToken)
        {
            _traceRouteOrchestrator.Results += StoreResutlsAsTheyComeIn;
            addressesToMonitor = await ValidateAddressesByTraceRoute(addressesToMonitor, cancellationToken);
            _traceRouteOrchestrator.Results -= StoreResutlsAsTheyComeIn;

            _pingOrchestrator.Results += StoreResutlsAsTheyComeIn;
            _ = await _pingOrchestrator.PingUntil(addressesToMonitor.ToArray(),
                                                 until,
                                                 cancellationToken);
            _pingOrchestrator.Results -= StoreResutlsAsTheyComeIn;

        }

        private readonly ConcurrentDictionary<IPAddress, List<long>> _addressrsponses = new();

        public event EventHandler<SubOrchestratorEventArgs>? Reset;

        private void StoreResutlsAsTheyComeIn(object? source, PingResponseModelEventArgs e)
        {
            if (e == null) return;

            _pingResponseModelStorageOrchestrator.StoreAsync(e.Model).Wait();

            // track the recent history of response times for this address for reset triggering.
            UpdateResponseHistoryForAddress(e);

            
        }

        private void ResetWhenTimeoutThresholdExceeded(PingResponseModelEventArgs e)
        {
            // is threshold of timeout exceeded for this address, if so trigger a reset and restart the process with a new route
            // this prevents endless timeout data when a route change has clearly occured.
            // in cases of total loss of link then a reset is usefull aswell.
            if (_addressrsponses[e.Model.Request.Address].Count == ResetThresholdCount &&
                _addressrsponses[e.Model.Request.Address].Max() == 0)
            {
                Reset?.Invoke(this, new SubOrchestratorEventArgs(this.GetType()));
            }
        }

        private void UpdateResponseHistoryForAddress(PingResponseModelEventArgs e)
        {
            _addressrsponses.AddOrUpdate(
                                e.Model.Request.Address,
                                new List<long>() { e.Model.Response?.RoundtripTime ?? 0 },
                                (address, list) =>
                                {
                                    list.Add(e.Model.Response?.RoundtripTime ?? 0);
                                    while (list.Count > ResetThresholdCount) list.RemoveAt(0);
                                    return list;

                                });
            ResetWhenTimeoutThresholdExceeded(e);
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