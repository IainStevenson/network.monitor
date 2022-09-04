using netmon.domain.Messaging;
using System.Net;

namespace netmon.domain.Interfaces
{
    public interface ITraceRouteOrchestrator
    {
        Task<PingResponseModels> Execute(IPAddress iPAddress, CancellationToken cancellationToken);
    }
}
