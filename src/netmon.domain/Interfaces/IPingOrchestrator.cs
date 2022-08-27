using netmon.domain.Messaging;
using netmon.domain.Models;
using System.Net;

namespace netmon.domain.Interfaces
{
    public interface IPingOrchestrator
    {
        event EventHandler<PingResponseModelEventArgs> Results;
        Task<PingResponses> PingUntil(IPAddress[] addresses, TimeSpan until, CancellationToken cancellation);
    }
}
