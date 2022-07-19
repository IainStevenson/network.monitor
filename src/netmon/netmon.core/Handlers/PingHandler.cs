using netmon.core.Models;
using System.Net.NetworkInformation;
using System.Text;

namespace netmon.core.Handlers
{

    /// <summary>
    /// Handles <see cref="Ping"/> tasks.
    /// </summary>
    public class PingHandler : IPingHandler
    {
        /// <summary>
        /// Asnychronously emit a ping to an address and return the response.
        /// </summary>
        /// <param name="action"></param>
        /// <returns></returns>
        public Task<PingResponseModel> Execute(PingRequestModel request, CancellationToken cancellationToken)
        {
            var response = new PingResponseModel();

            return Task.Run(() =>
            {
                using (Ping pingSender = new())
                {
                    // Create a buffer of 32 bytes of data to be transmitted.


                    response.Request = request;
                    response.Start = DateTimeOffset.UtcNow;
                    PingReply reply = pingSender.Send(request.Address, request.Timeout, request.Buffer, request.Options);
                    response.Finish = DateTimeOffset.UtcNow;
                    response.Response = reply;

                    //if (reply.Status == IPStatus.Success)
                    //{
                    //    response.RespondingAddress = reply.Address;
                    //    response.RoundTripTime = reply.RoundtripTime;
                    //    response.Options.Ttl = reply.Options?.Ttl ?? request.Options.Ttl;
                    //    response.Options.DontFragment = reply.Options?.DontFragment ?? true;
                    //    response.BufferSize = reply.Buffer.Length;
                    //}

                }
                return response;
            });
        }
    }
}