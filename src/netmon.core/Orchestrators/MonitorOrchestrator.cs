using Microsoft.Extensions.Logging;
using netmon.core.Configuration;
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
        // private readonly IStorage<PingResponseModel> _pingResponseStorage;
        private readonly IPingResponseModelStorageOrchestrator _pingResponseModelStorageOrchestrator;
        private readonly ILogger<MonitorOrchestrator> _logger;

        public MonitorOrchestrator(ITraceRouteOrchestrator traceRouteOrchestrator,
            IPingOrchestrator pingOrchestrator,
         //   IStorage<PingResponseModel> pingResponseStorage,
         IPingResponseModelStorageOrchestrator pingResponseModelStorageOrchestrator,
        ILogger<MonitorOrchestrator> logger)
        {
            _traceRouteOrchestrator = traceRouteOrchestrator;
            _pingOrchestrator = pingOrchestrator;
            // _pingResponseStorage = pingResponseStorage;
            _pingResponseModelStorageOrchestrator = pingResponseModelStorageOrchestrator;
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
        public async Task Execute(MonitorModes mode, List<IPAddress> addressesToMonitor, TimeSpan until, CancellationToken cancellationToken)
        {            
            
            switch (mode)// TODO: Write an OO way of doing this to remove the switch
            {
                case MonitorModes.TraceRoute:
                    _traceRouteOrchestrator.Results += StoreResutlsAsTheyComeIn;

                    while (!cancellationToken.IsCancellationRequested)
                    {
                        _ = await ValidateAddressesByTraceRoute(addressesToMonitor, false, cancellationToken);
                    }
                    _traceRouteOrchestrator.Results -= StoreResutlsAsTheyComeIn;

                    break;
                case MonitorModes.TraceRouteThenPing:

                    _pingOrchestrator.Results += StoreResutlsAsTheyComeIn;

                    addressesToMonitor = await ValidateAddressesByTraceRoute(addressesToMonitor, false, cancellationToken);

                    _ = await _pingOrchestrator.PingUntil(addressesToMonitor.ToArray(),
                                                         until,
                                                         cancellationToken);

                    _pingOrchestrator.Results -= StoreResutlsAsTheyComeIn;


                    break;
                case MonitorModes.PingOnly:
                    _pingOrchestrator.Results += StoreResutlsAsTheyComeIn;

                    _ = await _pingOrchestrator.PingUntil(addressesToMonitor.ToArray(),
                                                         until,
                                                         cancellationToken);
                    _pingOrchestrator.Results -= StoreResutlsAsTheyComeIn;

                    break;
            }
           
        }

        private async Task<List<IPAddress>> ValidateAddressesByTraceRoute(List<IPAddress> requestedAddresses, bool pingOnly, CancellationToken cancellationToken)
        {

            _logger.LogTrace("Validating [{count}] addresses", requestedAddresses.Count);

            var addressesToMonitor = requestedAddresses.Distinct().ToArray().ToList();


            if (!addressesToMonitor.Any())
            {
                _logger.LogTrace("No addresses specified, initialising to default monitoring address {address}", Defaults.DefaultMonitoringDestination);
                addressesToMonitor = await GetAddressesToMonitorFromTraceRoute(Defaults.DefaultMonitoringDestination, cancellationToken);
            }
            else if (!pingOnly)
            {
                _logger.LogTrace("Tracing route to specified addresses {count}", addressesToMonitor.Count);
                List<IPAddress> tracedAddresses = new();
                foreach (var address in addressesToMonitor)
                {
                    var routeaddresses = await GetAddressesToMonitorFromTraceRoute(address, cancellationToken);
                    tracedAddresses.AddRange(routeaddresses);
                }
                addressesToMonitor.AddRange(tracedAddresses);
                addressesToMonitor = addressesToMonitor.Distinct().ToList();
            }

            return addressesToMonitor;
        }

        void StoreResutlsAsTheyComeIn(object? source, PingResponseModelEventArgs? e)
        {
            if (e == null) return;

            _pingResponseModelStorageOrchestrator.Store(e.Model).Wait();
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