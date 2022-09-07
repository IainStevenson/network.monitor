using netmon.domain.Configuration;
using System.Net;

namespace netmon.domain.Interfaces
{
    public interface IMonitorOrchestrator
    {
        Task Execute(MonitorModes mode, List<IPAddress> addressesToMonitor, TimeSpan until,  CancellationToken cancellationToken);
    }

}