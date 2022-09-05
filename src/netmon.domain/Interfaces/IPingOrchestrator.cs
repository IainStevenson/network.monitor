using netmon.domain.Messaging;
using netmon.domain.Models;
using System.Net;

namespace netmon.domain.Interfaces
{
    public interface IPingOrchestrator
    {
        Task<PingResponseModels> PingManyUntil(IPAddress[] addresses, TimeSpan until, CancellationToken cancellation);
        Task<PingResponseModels> PingMany(IPAddress[] addresses, CancellationToken cancellation);
        Task<PingResponseModel> PingOne(PingRequestModel pingRequest, CancellationToken cancellationToken);
    }
}
