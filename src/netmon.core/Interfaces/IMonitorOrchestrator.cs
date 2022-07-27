using System.Net;

namespace netmon.core.Interfaces
{
    public interface IMonitorOrchestrator
    {
        Task<List<IPAddress>> Execute(List<IPAddress> addressesToMonitor, TimeSpan until, bool pingOnly, CancellationToken cancellationToken);
    }
}