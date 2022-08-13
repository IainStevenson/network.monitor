using netmon.core.Configuration;
using System.Net;

namespace netmon.core.Interfaces
{
    public interface IMonitorOrchestrator
    {
        Task Execute(MonitorModes mode, List<IPAddress> addressesToMonitor, TimeSpan until,  CancellationToken cancellationToken);
    }
}