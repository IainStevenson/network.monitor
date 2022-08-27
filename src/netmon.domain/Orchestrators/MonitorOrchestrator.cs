using Microsoft.Extensions.Logging;
using netmon.domain.Configuration;
using netmon.domain.Interfaces;
using System.Net;

namespace netmon.domain.Orchestrators
{
    /// <summary>
    /// Monitor the network every interval according to the list of <see cref="IPAddress">. 
    /// Observe the emitted data and store it in the repository for future use.</see>
    /// </summary>
    public class MonitorOrchestrator : IMonitorOrchestrator
    {
        private readonly Dictionary<MonitorModes, IMonitorSubOrchestrator> _monitors;
        private readonly ILogger<MonitorOrchestrator> _logger;

        public MonitorOrchestrator(Dictionary<MonitorModes, IMonitorSubOrchestrator> monitors,
        ILogger<MonitorOrchestrator> logger)
        {
            _monitors = monitors;
            _logger = logger;

        }

        /// <summary>
        /// Handle the requested mode for the monitor session.
        /// </summary>
        /// <param name="mode">The monitoring mode requried.</param>
        /// <param name="addressesToMonitor">The addresses to ping.</param>
        /// <param name="until">The time span to continue monitoring over. 
        /// should use this for continuous use: var forEver = new TimeSpan(DateTimeOffset.MaxValue.Ticks - DateTimeOffset.UtcNow.Ticks);
        /// </param>
        /// <param name="pingOnly">Only works when addresses are supplied. If true then will not peform a trace on the supplied addresses first and then monitor all of them, returning the complete list.</param>
        /// <param name="cancellationToken">The asnyc cacnellaction token for earyl termination.</param>
        /// <returns>An instance of <see cref="Task"/>.</returns>
        public async Task Execute(MonitorModes mode, List<IPAddress> addressesToMonitor, TimeSpan until, CancellationToken cancellationToken)
        {
            _logger.LogTrace("Handling call with orchestrator for {mode}", mode);
            await _monitors[mode].Execute(addressesToMonitor, until, cancellationToken);
        }
    }
}