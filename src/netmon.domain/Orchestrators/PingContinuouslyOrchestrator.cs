using DnsClient.Internal;
using Microsoft.Extensions.Logging;
using netmon.domain.Configuration;
using netmon.domain.Interfaces;
using System.Net;

namespace netmon.domain.Orchestrators
{

    /// <summary>
    /// Continuously pings the specified addresses until cancelled or the specified period has passsed. 
    /// Recording results via events by passing them to the Storage orchestrator.
    /// </summary>
    public class PingContinuouslyOrchestrator : IMonitorModeOrchestrator
    {
        private readonly IPingOrchestrator _pingOrchestrator;
        private readonly ILogger<PingContinuouslyOrchestrator> _logger;
        private readonly PingOrchestratorOptions _pingOrchestratorOptions;

        public PingContinuouslyOrchestrator(
                IPingOrchestrator pingOrchestrator,
                PingOrchestratorOptions pingOrchestratorOptions,
                ILogger<PingContinuouslyOrchestrator> logger)
        {
            _pingOrchestrator = pingOrchestrator;
            _logger = logger;
            _pingOrchestratorOptions = pingOrchestratorOptions;
        }


        public async Task Execute(List<IPAddress> addressesToMonitor, TimeSpan until, CancellationToken cancellationToken)
        {
            _logger.LogTrace("PINGing {count} addresses until {until}.", addressesToMonitor.Count, until);
            try
            {
                _ = await _pingOrchestrator.PingUntil(addressesToMonitor.ToArray(), until, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogTrace("PING thre exception {ex}.", ex);
            }
        }
    }
}