using netmon.core.Configuration;
using netmon.core.Data;
using netmon.core.Interfaces;
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
        /// <returns>An instance of <see cref="Task"/> deliverig an instance of <see cref="PingResponseModel"/></returns>
        public Task<PingResponseModel> Execute(PingRequestModel request, CancellationToken cancellationToken)
        {
            var response = new PingResponseModel();

            return Task.Run(() =>
            {
                using (Ping pingSender = new())
                {
                    response.Request = request;
                   
                    var pingOptions = new PingOptions() { 
                        DontFragment = _pingOptions.DontFragment, 
                        Ttl =request.Ttl 
                    };


                    System.Diagnostics.Trace.WriteLine($"{nameof(PingHandler)}.{nameof(Execute)} PING request  {request.Address}, Timeout: {_pingOptions.Timeout}, TTL {request.Ttl}");
                    
                    response.Start = DateTimeOffset.UtcNow;
                    PingReply reply = pingSender.Send(
                        request.Address, 
                        _pingOptions.Timeout,
                        PingRequestModel.Buffer, 
                        pingOptions);
                    response.Finish = DateTimeOffset.UtcNow;
                    
                    response.Response = reply;

                    System.Diagnostics.Trace.WriteLine($"{nameof(PingHandler)}.{nameof(Execute)} PING response {response.Duration.TotalMilliseconds} ms, Status {response.Response.Status},  TTL {response.Response.Options?.Ttl}");


                }
                return response;
            });
        }
    }
}