using System.Net;

namespace netmon.core.Interfaces
{
    public interface IMonitorSubOrchestrator
    {
        Task Handle(List<IPAddress> addressesToMonitor, TimeSpan until, CancellationToken cancellationToken);
    }
}