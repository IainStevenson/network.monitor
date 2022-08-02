using netmon.core.Messaging;
using netmon.core.Models;
using System.Net;

namespace netmon.core.Interfaces
{
    public interface IPingOrchestrator
    {
        event EventHandler<PingResponseModelEventArgs> Results;
        Task<PingResponses> PingUntil(IPAddress[] addresses, TimeSpan until, CancellationToken cancellation);
    }
}
