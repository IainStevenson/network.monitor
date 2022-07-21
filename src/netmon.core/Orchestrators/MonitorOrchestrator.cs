using netmon.core.Configuration;
using netmon.core.Data;
using netmon.core.Handlers;
using netmon.core.Models;
using System.Linq;
using System.Net;

namespace netmon.core.Orchestrators
{
    /// <summary>
    /// Monitor the network according to <see cref="MonitorOptions"/> and <see cref="MonitorRequestModel"/> data. 
    /// If no model is provided or is empty, then perform a traceroute to the specified default destination option <see cref="IPAddress"/> and use the <see cref="TraceRouteOrchestrator"/>results to create a configuration to use.
    /// Will ping the route addresses to the destination once every period defined in the configuration to log access and response times.
    /// Every bandwidth test period the external utility will be called to determine the actual badnwidth avaiable at the time.
    /// </summary>
    public class MonitorOrchestrator
    {
        private readonly MonitorOptions _monitorOptions;
        private readonly TraceRouteOrchestrator _routeOrchestrator;
        private readonly PingOrchestrator _pingOrchestrator;
        private readonly IHostAddressTypeHandler _hostAddressTypeHandler;

        public MonitorOrchestrator(TraceRouteOrchestrator traceRouteOrchestrator, PingOrchestrator pingOrchestrator, MonitorOptions monitorOptions, IHostAddressTypeHandler hostAddressTypeHandler)
        {
            _routeOrchestrator = traceRouteOrchestrator;
            _pingOrchestrator = pingOrchestrator;
            _monitorOptions = monitorOptions;
            _hostAddressTypeHandler = hostAddressTypeHandler;
        }

        public async Task Configure(MonitorRequestModel monitor, CancellationToken cancellationToken)
        {
            var tracedRoutes = await _routeOrchestrator.Execute(monitor.Destination, cancellationToken);

            var validHosts = tracedRoutes
                                .AsOrderedList()
                                        .Where(w => w.Response?.Status == System.Net.NetworkInformation.IPStatus.Success ||
                                                    w.Response?.Status == System.Net.NetworkInformation.IPStatus.TtlExpired)
                                    ;
            // the first address on the list will be your default gateway and is therefore Local (unless you are on a phone or mobile device)

            if (!_monitorOptions.Roaming)
            {
#pragma warning disable CS8602 // Dereference of a possibly null reference. By now we have a response due to Status = Success
                monitor.LocalHosts.Add(validHosts.Select(s => s.Response.Address).First());
#pragma warning restore CS8602 // Dereference of a possibly null reference. By now we have a response due to Status = Success
            }

            foreach (var host in validHosts.Where(w => w.Attempt == 1))
            {
#pragma warning disable CS8602 // Dereference of a possibly null reference. By now we have a response due to Status = Success
                monitor.Hosts.Add(host.Response.Address, _hostAddressTypeHandler.GetPrivateHostType(host.Response.Address as IPAddress));
#pragma warning restore CS8602 // Dereference of a possibly null reference. By now we have a response due to Status = Success
            }

            foreach (var host in monitor.LocalHosts)
            {
                monitor.Hosts[host] = HostTypes.Local;
            }
        }

        /// <summary>
        /// Monitor according to the configuration, build configuration as needed. Continue until period or told to stop.
        /// </summary>
        /// <param name="monitorModel">The configuration model to process.</param>
        /// <param name="until">The time span to continue moitoring over.</param>
        /// <param name="cancellationToken">The asnyc cacnellaction token for earyl termination.</param>
        /// <returns>An instance of <see cref="Task"/> delivering an isntance of <see cref="PingResponses"/></returns>
        public async Task<MonitorRespones> Execute(MonitorRequestModel monitorModel, TimeSpan until, CancellationToken cancellationToken)
        {
            if (!monitorModel.Hosts.Any())
            {
                await Configure(monitorModel, cancellationToken);
            }

            MonitorRespones responses = new();

            responses.AddRange((await _pingOrchestrator.PingUntil(
                     monitorModel.Hosts.Select(x => x.Key).ToArray(),
                     until,
                     cancellationToken)).Select(s => s.Value).ToList());

            return responses;
        }
    }
}