using System.Net;

namespace netmon.domain.Interfaces
{
    public interface IMonitorModeOrchestrator
    {
        Task Execute(List<IPAddress> addressesToMonitor, TimeSpan until, CancellationToken cancellationToken);
    }

}