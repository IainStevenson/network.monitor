using netmon.core.Messaging;
using netmon.core.Models;
using System.Net;

namespace netmon.core.Interfaces
{
    public interface ITraceRouteOrchestrator
    {
        event EventHandler<PingResponseModelEventArgs> Results;
        Task<PingResponses> Execute(IPAddress iPAddress, CancellationToken cancellationToken);
    }
}
