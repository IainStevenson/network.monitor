using netmon.domain.Messaging;
using netmon.domain.Models;
using System.Net;

namespace netmon.domain.Interfaces
{
    /// <summary>
    /// Handles complex ping tasks via the <see cref="PingHandler"/> and and returns the results  as <see cref="PingResponses"/>.
    /// </summary>
    public interface IPingOrchestrator
    {
        /// <summary>
        /// Until the <see cref="TimeSpan"/> has expired, relative to <see cref="DateTimeOffset.UtcNow"/>, will continualy PING all of the specified <see cref="IPAddress"/> followed by the configured pause..
        /// Calls <see cref="ProcessResults"/> to store each response in the available repositories and adds the responses to an instance of <see cref="PingResponseModels"/>.
        /// </summary>
        /// <param name="addresses">An array of <see cref="IPAddress"/> as target addreses</param>
        /// <param name="cancellationToken">An instance of  <see cref="CancellationToken"/>.</param>
        /// <returns>An instance of <see cref="PingResponseModels"/>.</returns>
        Task<PingResponseModels> PingManyUntil(IPAddress[] addresses, TimeSpan until, CancellationToken cancellation);
        /// <summary>
        /// Emits an ICMP PING to each of the addresses and returns the responses. 
        /// Calls <see cref="ProcessResults"/> to store each response in the available repositories and adds the responses to an instance of <see cref="PingResponseModels"/>.
        /// </summary>
        /// <param name="addresses">An array of <see cref="IPAddress"/> as target addreses</param>
        /// <param name="cancellationToken">An instance of  <see cref="CancellationToken"/>.</param>
        /// <returns>An instance of <see cref="PingResponseModels"/>.</returns>
        Task<PingResponseModels> PingMany(IPAddress[] addresses, CancellationToken cancellation);
        /// <summary>
        /// Emits an ICMP PING to the address in the request and returns the response. No other processing is performed.
        /// </summary>
        /// <param name="request">An instance of <see cref="PingRequestModel">.</see></param>
        /// <param name="cancellationToken">An instance fo <see cref="CancellationToken"/>.</param>
        /// <returns>An isntance of <see cref="Task"/> wrapping an instance of <see cref="PingRequestModel"/>.</returns>
        Task<PingResponseModel> PingOne(PingRequestModel pingRequest, CancellationToken cancellationToken);
    }
}
