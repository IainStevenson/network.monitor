using Microsoft.Extensions.Logging;
using netmon.core.Configuration;
using netmon.core.Interfaces;
using System.Net;

namespace netmon.core.Orchestrators
{
    /// <summary>
    /// Monitor the network every interval according to the list of <see cref="IPAddress">. 
    /// Observe the emitted data and store it in the repository for future use.</see>
    /// </summary>
    public class MonitorOrchestrator : IMonitorOrchestrator
    {
        private Dictionary<MonitorModes, IMonitorSubOrchestrator> _monitors;
        private readonly ILogger<MonitorOrchestrator> _logger;

        public MonitorOrchestrator(Dictionary<MonitorModes, IMonitorSubOrchestrator> monitors,
        ILogger<MonitorOrchestrator> logger)
        {
            _monitors = monitors;
            _logger = logger;

        }

        /// <summary>
        /// Continuously ping the addresses until the time has expired.
        /// </summary>
        /// <param name="addressesToMonitor">The addresses to ping.</param>
        /// <param name="until">The time span to continue monitoring over. 
        /// should use this for continuous use: var forEver = new TimeSpan(DateTimeOffset.MaxValue.Ticks - DateTimeOffset.UtcNow.Ticks);
        /// </param>
        /// <param name="pingOnly">Only works when addresses are supplied. If true then will not peform a trace on the supplied addresses first and then monitor all of them, returning the complete list.</param>
        /// <param name="cancellationToken">The asnyc cacnellaction token for earyl termination.</param>
        /// <returns>An instance of <see cref="Task"/>.</returns>
        public async Task Execute(MonitorModes mode, List<IPAddress> addressesToMonitor, TimeSpan until, CancellationToken cancellationToken)
        {
            await _monitors[mode].Handle(addressesToMonitor, until, cancellationToken);
        }
    }
}