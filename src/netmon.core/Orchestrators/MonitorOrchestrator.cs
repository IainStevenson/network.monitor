using netmon.core.Configuration;
using netmon.core.Interfaces;
using netmon.core.Messaging;
using netmon.core.Models;
using System.Linq;
using System.Net;
using System.Net.Sockets;

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



        private void AddValidHosts(MonitorRequestModel monitor, List<PingResponseModel> validHosts)
        {

            foreach (var host in validHosts.Where(w => w.Attempt == 1))
            {
                var respdondingAddress = host.Response?.Address;
                if (respdondingAddress != null)
                {
                    if (!monitor.Hosts.ContainsKey(respdondingAddress))
                    {
                        monitor.Hosts.Add(respdondingAddress, _hostAddressTypeHandler.GetPrivateHostType(respdondingAddress));
                    }
                }
            }

        }



#pragma warning disable CS8602 // Dereference of a possibly null reference. Defended due to Status = Success
        public async Task Configure(MonitorRequestModel monitorRequestModel, CancellationToken cancellationToken)
        {

            PingResponses tracedRoutes = await _routeOrchestrator.Execute(monitorRequestModel.Destination, cancellationToken);

            var validHosts = tracedRoutes.AsOrderedList()
                                        .Where(w => w.Response?.Status == System.Net.NetworkInformation.IPStatus.Success ||
                                                    w.Response?.Status == System.Net.NetworkInformation.IPStatus.TtlExpired)
                                        .ToList()
                                    ;

            AddFirstAddressAsLocalHostType(monitorRequestModel, validHosts);

            AddValidHosts(monitorRequestModel, validHosts);

            MarkLocalHosts(monitorRequestModel);

            monitorRequestModel.Data = tracedRoutes;
        }

        private static void AddFirstAddressAsLocalHostType(MonitorRequestModel monitorRequestModel, List<PingResponseModel> validHosts)
        {
            var firstAddress = validHosts.Select(s => s.Response.Address).FirstOrDefault();
            if (firstAddress != null)
            {
                if (!monitorRequestModel.LocalHosts.Contains(firstAddress))
                    monitorRequestModel.LocalHosts.Add(firstAddress);
            }
        }

        private static void MarkLocalHosts(MonitorRequestModel monitorRequestModel)
        {
            foreach (IPAddress host in monitorRequestModel.LocalHosts)
            {
                monitorRequestModel.Hosts[host] = HostTypes.Local;
            }
        }
#pragma warning restore CS8602 // Dereference of a possibly null reference. By now we have a response due to Status = Success

        /// <summary>
        /// Monitor according to the configuration, build configuration as needed. Continue until period or told to stop.
        /// </summary>
        /// <param name="monitorRequestModel">The configuration model to process.</param>
        /// <param name="until">The time span to continue moitoring over.</param>
        /// <param name="cancellationToken">The asnyc cacnellaction token for earyl termination.</param>
        /// <returns>An instance of <see cref="Task"/> delivering an isntance of <see cref="PingResponses"/></returns>
        public async Task<MonitorResponses> Execute(MonitorRequestModel monitorRequestModel, TimeSpan until, CancellationToken cancellationToken)
        {
            if (
               !Dns.GetHostEntry(Dns.GetHostName())
                       .AddressList.Contains(_monitorOptions.BaseAddress)
                       )
            {
                await Configure(monitorRequestModel, cancellationToken);
            }

            MonitorResponses responses = new();

            responses.AddRange((await _pingOrchestrator.PingUntil(monitorRequestModel.Hosts.Select(x => x.Key).ToArray(),
                                                                     until,
                                                                     cancellationToken))
                                                                     .Select(s => s.Value)
                                                                     .ToList());

            return responses;
        }
    }
}