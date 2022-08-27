using netmon.domain.Messaging;
using netmon.domain.Models;
using System.Net;

namespace netmon.domain.Interfaces
{
    public interface ITraceRouteOrchestrator
    {
        event EventHandler<PingResponseModelEventArgs> Results;
        Task<PingResponses> Execute(IPAddress iPAddress, CancellationToken cancellationToken);
    }
}
