using netmon.core.Configuration;
using netmon.core.Data;
using netmon.core.Models;
using System.Net.NetworkInformation;

namespace netmon.core.Handlers
{

    /// <summary>
    /// Handles <see cref="Ping"/> tasks.
    /// </summary>
    public class PingHandler : IPingHandler
    {
        private readonly PingHandlerOptions _pingOptions;
        public PingHandlerOptions Options {  get {  return  _pingOptions; } }

        public PingHandler(PingHandlerOptions pingOptions)     
        {
            _pingOptions = pingOptions;
        }

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
                //request.Options = _pingOptions; // record the options currently working with so the client does not have to
                using (Ping pingSender = new())
                {
                    response.Request = request;
                    var pingOptions = new PingOptions() { 
                        DontFragment = _pingOptions.DontFragment, 
                        Ttl =_pingOptions.Ttl 
                    };

                    System.Diagnostics.Trace.WriteLine($"{nameof(PingHandler)}.{nameof(Execute)} PING {request.Address}, Timeout: {_pingOptions.Timeout}, TTL {request.Options.Ttl}");

                    response.Start = DateTimeOffset.UtcNow;
                    PingReply reply = pingSender.Send(
                        request.Address, 
                        _pingOptions.Timeout, 
                        request.Buffer, 
                        pingOptions);
                    response.Finish = DateTimeOffset.UtcNow;
                    
                    response.Response = reply;

                    System.Diagnostics.Trace.WriteLine($"{nameof(PingHandler)}.{nameof(Execute)} PING response {response.Duration} ms, Status {response.Response.Status}");


                }
                return response;
            });
        }
    }
}