using netmon.domain.Messaging;
using System.Net;

namespace netmon.domain.Interfaces
{
    public interface IPingOrchestrator
    {
        Task<PingResponseModels> PingUntil(IPAddress[] addresses, TimeSpan until, CancellationToken cancellation);
        Task<PingResponseModels> Ping(IPAddress[] addresses, CancellationToken cancellation);
    }
}
