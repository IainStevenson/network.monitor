using Microsoft.Extensions.Logging;
using netmon.core.Data;
using netmon.core.Interfaces;
using netmon.core.Messaging;
using netmon.core.Models;
using System.Net;

namespace netmon.core.Orchestrators
{
    /// <summary>
    /// Monitor the network every interval according to the list of <see cref="IPAddress">. 
    /// Observe the emitted data and store it in the repository for future use.</see>
    /// </summary>
    public class MonitorOrchestrator : IMonitorOrchestrator
    {
        //private readonly MonitorOptions _monitorOptions;
        private readonly ITraceRouteOrchestrator _traceRouteOrchestrator;
        private readonly IPingOrchestrator _pingOrchestrator;
        private readonly IStorage<PingResponseModel> _pingResponseStorage;
        private readonly ILogger<MonitorOrchestrator> _logger;

        public MonitorOrchestrator(ITraceRouteOrchestrator traceRouteOrchestrator,
            IPingOrchestrator pingOrchestrator,
            IStorage<PingResponseModel> pingResponseStorage,
            ILogger<MonitorOrchestrator> logger)
        {
            _traceRouteOrchestrator = traceRouteOrchestrator;
            _pingOrchestrator = pingOrchestrator;
            _pingResponseStorage = pingResponseStorage;
            _logger = logger;
        }

        /// <summary>
        /// Continuously ping the addresses until the time has expired.
        /// 
        /// </summary>
        /// <param name="addressesToMonitor">The addresses to ping.</param>
        /// <param name="until">The time span to continue monitoring over. 
        /// should use this for continuous use: var forEver = new TimeSpan(DateTimeOffset.MaxValue.Ticks - DateTimeOffset.UtcNow.Ticks);
        /// </param>
        /// <param name="pingOnly">Only works when addresses are supplied. If true then will not peform a trace on the supplied addresses first and then monitor all of them, returning the complete list.</param>
        /// <param name="cancellationToken">The asnyc cacnellaction token for earyl termination.</param>
        /// <returns>An instance of <see cref="Task"/> delivering an list of all the <see cref="IPAddresses"/> which were pinged, and that ever responded.</returns>
        public async Task<List<IPAddress>> Execute(List<IPAddress> addressesToMonitor, TimeSpan until, bool pingOnly, CancellationToken cancellationToken)
        {

            addressesToMonitor = await ValidateAddresses(addressesToMonitor, pingOnly, cancellationToken);

            MonitorResponses responses = new();

            _pingOrchestrator.Results += StoreResutlsAsTheyComeIn;

            var pingResults = await _pingOrchestrator.PingUntil(addressesToMonitor.ToArray(),
                                                                     until,
                                                                     cancellationToken);

            responses.AddRange(pingResults.Select(s => s.Value).ToList());

            _pingOrchestrator.Results -= StoreResutlsAsTheyComeIn;
            return responses
                .Where(x => x.Response?.Status == System.Net.NetworkInformation.IPStatus.Success)
                .Select(s => s.Response?.Address ?? IPAddress.Loopback)
                .Distinct()
                .ToList();
        }

        private async Task<List<IPAddress>> ValidateAddresses(List<IPAddress> addressesToMonitor, bool skipTrace, CancellationToken cancellationToken)
        {
            _logger.LogTrace("Validating [{count}] addresses", addressesToMonitor.Count);

            List<IPAddress> discoveredAddresses = new();

            if (!addressesToMonitor.Any())
            {
                //discoveredAddresses = await GetAddressesToMonitorFromTraceRoute(Defaults.DefaultMonitoringDestination, cancellationToken);
                _logger.LogTrace("Adding default monitor address {addressesToMonitor}", addressesToMonitor);
                addressesToMonitor.Add(Defaults.DefaultMonitoringDestination);
            }
            
            if (!skipTrace)
            {
                List<IPAddress> tracedAddresses = new();
                foreach (var address in addressesToMonitor)
                {
                    var routeaddresses = await GetAddressesToMonitorFromTraceRoute(address, cancellationToken);
                    tracedAddresses.AddRange(routeaddresses);
                }
                discoveredAddresses.AddRange(tracedAddresses);
            }
            return discoveredAddresses.Distinct().ToList();
        }

        void StoreResutlsAsTheyComeIn(object? source, PingResponseModelEventArgs? e)
        {
            if (e == null) return;

            _pingResponseStorage.Store(e.Model).Wait();
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