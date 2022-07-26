﻿using Microsoft.Extensions.Logging;
using netmon.domain.Configuration;
using netmon.domain.Interfaces;
using netmon.domain.Models;
using System.Net.NetworkInformation;

namespace netmon.domain.Handlers
{

    /// <inheritdoc/>
    public class PingHandler : IPingHandler
    {
        private readonly PingHandlerOptions _pingOptions;
        private readonly ILogger<PingHandler> _logger;

        public PingHandler(PingHandlerOptions pingOptions, ILogger<PingHandler> logger)
        {
            _pingOptions = pingOptions;
            _logger = logger;
        }
        /// <inheritdoc/>
        public Task<PingResponseModel> Ping(PingRequestModel request, CancellationToken cancellationToken)
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

                    _logger.LogTrace(
                        "{class}.{method} PING request  {address}, Timeout: {timeout}, TTL {timetolive}", 
                        nameof(PingHandler), nameof(Ping), request.Address, _pingOptions.Timeout, request.Ttl);

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
                    _logger.LogTrace("{class}.{method} PING response {duration} ms, Status {status},  TTL {timetolive}", 
                        nameof(PingHandler), nameof(Ping), response.Duration.TotalMilliseconds, response.Response?.Status, response.Response?.Options?.Ttl);
                }
                return response;
            });
        }
    }
}