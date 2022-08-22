using Microsoft.Extensions.Logging;
using netmon.core.Interfaces;
using netmon.core.Models;
using System.Net;

namespace netmon.core.Orchestrators
{

    /// <summary>
    /// Continuously pings the specified addresses until cancelled or the specified period has passsed. Recording results via the Storage orchestrator.
    /// </summary>
    public class MonitorPingSubOrchestrator : IMonitorSubOrchestrator
    {
        private readonly IPingOrchestrator _pingOrchestrator;
        private readonly IStorageOrchestrator<PingResponseModel> _pingResponseModelStorageOrchestrator;
        private readonly ILogger<MonitorPingSubOrchestrator> _logger;

        public MonitorPingSubOrchestrator(IStorageOrchestrator<PingResponseModel> pingResponseModelStorageOrchestrator,
                IPingOrchestrator pingOrchestrator,
               ILogger<MonitorPingSubOrchestrator> logger)
        {
            _pingResponseModelStorageOrchestrator = pingResponseModelStorageOrchestrator;
            _pingOrchestrator = pingOrchestrator;
            _logger = logger;
        }

        public event EventHandler<SubOrchestratorEventArgs>? Reset; // TODO, Address this perhaps as another interface

        public async Task Handle(List<IPAddress> addressesToMonitor, TimeSpan until, CancellationToken cancellationToken)
        {
            if (addressesToMonitor.Any() && !cancellationToken.IsCancellationRequested)
            {
                _pingOrchestrator.Results += StoreResutlsAsTheyComeIn;
                try
                {
                    _ = await _pingOrchestrator.PingUntil(addressesToMonitor.ToArray(), until, cancellationToken);
                }
                catch { }
                _pingOrchestrator.Results -= StoreResutlsAsTheyComeIn;
            }
        }

        private void StoreResutlsAsTheyComeIn(object? source, PingResponseModelEventArgs? e)
        {
            if (e == null) return;

            _pingResponseModelStorageOrchestrator.Store(e.Model).Wait();
        }

    }
}