using Microsoft.Extensions.Logging;
using netmon.core.Configuration;
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
        private readonly ILogger<PingHandler> _logger;

        public PingHandler(PingHandlerOptions pingOptions, ILogger<PingHandler> logger)
        {
            _pingOptions = pingOptions;
            _logger = logger;
        }

        /// <summary>
        /// Asnychronously emit a ping to an address and return the response.
        /// </summary>
        /// <param name="action"></param>
        /// <returns>An instance of <see cref="Task"/> delivering an instance of <see cref="PingResponseModel"/></returns>
        public Task<PingResponseModel> Execute(PingRequestModel request, CancellationToken cancellationToken)
        {
            var response = new PingResponseModel();

            return Task.Run(() =>
            {
                using (Ping pingSender = new())
                {
                    response.Request = request;

                    var pingOptions = new PingOptions()
                    {
                        DontFragment = _pingOptions.DontFragment,
                        Ttl = request.Ttl
                    };


                    _logger.LogTrace($"{nameof(PingHandler)}.{nameof(Execute)} PING request  {request.Address}, Timeout: {_pingOptions.Timeout}, TTL {request.Ttl}");

                    response.Start = DateTimeOffset.UtcNow;
                    
                    PingReply reply = pingSender.Send(request.Address,
                                                        _pingOptions.Timeout,
                                                        request.Buffer,
                                                        pingOptions);

                    response.Finish = DateTimeOffset.UtcNow;
                    
                    if (reply != null)
                    {
                        response.Response = new PingReplyModel()
                        {
                            Address = reply.Address,
                            Buffer = reply.Buffer,
                            Options = reply.Options,
                            RoundtripTime = reply.RoundtripTime,
                            Status = reply.Status
                        };
                    }
                    _logger.LogTrace($"{nameof(PingHandler)}.{nameof(Execute)} PING response {response.Duration.TotalMilliseconds} ms, Status {response.Response?.Status},  TTL {response.Response?.Options?.Ttl}");


                }
                return response;
            });
        }
    }
}