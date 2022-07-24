using netmon.core.Models;

namespace netmon.core.Interfaces
{
    /// <summary>
    /// Abstracts handling  Internet Control Message Protocol (ICMP) echo request message via the <see cref="System.Net.NetworkInformation.Ping"/> class.
    ///  
    /// </summary>
    public interface IPingHandler
    {
        /// <summary>
        /// Asnychronously emit a ping to an address and return the response.
        /// </summary>
        /// <param name="request">An instance of <see cref="PingRequestModel"/> defining the ping.</param>
        /// <param name="cancellationToken">An instance of <see cref="CancellationToken"/> allowing asynchronous cancellation.</param>
        /// <returns>An instance of <see cref="Task"/> deliverig an instance of <see cref="PingResponseModel"/></returns>
        Task<PingResponseModel> Execute(PingRequestModel request, CancellationToken cancellationToken);
    }
}