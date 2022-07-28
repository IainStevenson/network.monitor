using netmon.core.Messaging;
using System.Net;

namespace netmon.core.Interfaces
{
    public interface ITraceRouteOrchestrator
    {
        Task<PingResponses> Execute(IPAddress iPAddress, CancellationToken cancellationToken);
    }
}
