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
    public class MonitorOrchestrator
    {
        //private readonly MonitorOptions _monitorOptions;
        private readonly TraceRouteOrchestrator _routeOrchestrator;
        private readonly PingOrchestrator _pingOrchestrator;
        private readonly IStorage<PingResponseModel> _pingResponseStorage;
        public MonitorOrchestrator(TraceRouteOrchestrator traceRouteOrchestrator, 
            PingOrchestrator pingOrchestrator,
            IStorage<PingResponseModel> pingResponseStorage)
        {
            _routeOrchestrator = traceRouteOrchestrator;
            _pingOrchestrator = pingOrchestrator;
            _pingResponseStorage = pingResponseStorage;
        }

        /// <summary>
        /// Continuously ping the addresses until the time has expired.
        /// would use this for continuous use: var forEver = new TimeSpan(DateTimeOffset.MaxValue.Ticks - DateTimeOffset.UtcNow.Ticks);
        /// </summary>
        /// <param name="addressesToMonitor">The addresses to ping.</param>
        /// <param name="until">The time span to continue monitoring over.</param>
        /// <param name="cancellationToken">The asnyc cacnellaction token for earyl termination.</param>
        /// <returns>An instance of <see cref="Task"/> delivering an list of all the <see cref="IPAddresses"/> which were pinged, and that ever responded.</returns>
        public async Task<List<IPAddress>> Execute(List<IPAddress> addressesToMonitor, TimeSpan until, CancellationToken cancellationToken)
        {

            if (!addressesToMonitor.Any())
            {
                addressesToMonitor = await GetAddressesToMonitor(Defaults.DefaultMonitoringDestination, cancellationToken);
            }

            MonitorResponses responses = new();

            _pingOrchestrator.Results += StoreResutlsAsTheyComeIn;

            responses.AddRange((await _pingOrchestrator.PingUntil(addressesToMonitor.ToArray(),
                                                                     until,
                                                                     cancellationToken))
                                                                     .Select(s => s.Value)
                                                                     .ToList());

            _pingOrchestrator.Results -= StoreResutlsAsTheyComeIn;
            return responses
                .Where(x => x.Response?.Status == System.Net.NetworkInformation.IPStatus.Success)
                .Select(s => s.Response?.Address?? IPAddress.Loopback) 
                .Distinct()
                .ToList();
        }

        void StoreResutlsAsTheyComeIn(object? source, PingResponseModelEventArgs? e)
        {
            if (e == null ) return;

            _pingResponseStorage.Store(e.Model).Wait();
        }

        private async Task<List<IPAddress>> GetAddressesToMonitor(IPAddress defaultMonitoringDestination, CancellationToken cancellationToken)
        {
            PingResponses tracedRoutes = await _routeOrchestrator.Execute(defaultMonitoringDestination, cancellationToken);
            var validHosts = tracedRoutes.AsOrderedList()
                                        .Where(w => w.Response != null && w.Response.Status == System.Net.NetworkInformation.IPStatus.Success)
                                        .Select(s => s.Response?.Address ?? IPAddress.Loopback)
                                        .Distinct()
                                        .ToList();

            return validHosts;
        }
    }
}