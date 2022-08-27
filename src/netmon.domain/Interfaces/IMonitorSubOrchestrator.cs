using System.Net;

namespace netmon.domain.Interfaces
{
    public interface IMonitorSubOrchestrator
    {
        event EventHandler<SubOrchestratorEventArgs>? Reset;
        Task Execute(List<IPAddress> addressesToMonitor, TimeSpan until, CancellationToken cancellationToken);
    }
}