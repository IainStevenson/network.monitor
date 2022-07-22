using netmon.core.Configuration;
using netmon.core.Data;
using netmon.core.Interfaces;
using netmon.core.Messaging;
using netmon.core.Models;
using System.Net;
using System.Net.NetworkInformation;

namespace netmon.core.Orchestrators
{
    /// <summary>
    /// Orchestrates a traceroute using an instance of  <see cref="IPingHandler"/> and obtains raw response data.
    /// </summary>
    public class TraceRouteOrchestrator
    {
        private readonly IPingHandler _pingHandler;
        private readonly TraceRouteOrchestratorOptions _options;
        private readonly IPingRequestModelFactory _requestModelFactory;

        public TraceRouteOrchestrator(IPingHandler pingHandler, TraceRouteOrchestratorOptions options, IPingRequestModelFactory requestModelFactory)
        {
            _pingHandler = pingHandler;
            _options = options;
            _requestModelFactory = requestModelFactory;
        }

        /// <summary>
        /// Executes an asynchronous trace route to the specified <see cref="IPAddress"/>.
        /// Walks a ping for the same address one TTL at a time starting from one, which returns the non target address and either TtlExpired meaning the address returned is the next on the route or timeout which means it does not rspond to ICMP messages.
        /// For each address that responds with any status are re-pinged directly in the normal way as the direct destination with a TTl of 128 3 more times to get a statistical view of how it is responding.
        /// if an address continues to timeout it is reported if it does not it is reported with its address.
        /// </summary>
        /// <param name="iPAddress">The network address to trace.</param>
        /// <param name="cancellationToken">the asunc cancellation token.</param>
        /// <returns></returns>
        public async Task<PingResponses> Execute(IPAddress iPAddress, CancellationToken cancellationToken)
        {
            var responses = new PingResponses();

            for (var hop = 1; hop <= _options.MaxHops; hop++)
            {
                var pingRequest = _requestModelFactory.Create();
                pingRequest.Ttl = hop;
                pingRequest.Address = iPAddress;

                if (cancellationToken.IsCancellationRequested) break;

                try
                {

                    var pingResponse = await _pingHandler.Execute(pingRequest, cancellationToken);

                    RecordResult(responses, pingResponse);

                    if (
                            pingResponse != null
                            && (
                                pingResponse?.Response?.Status == IPStatus.TtlExpired
                                || pingResponse?.Response?.Status == IPStatus.Success
                                )
                        )
                    {
                        // it responded
                        var hopAddress = pingResponse.Response.Address;
                        var exitOnCompletion = pingResponse.Response.Status == IPStatus.Success;
                        for (var attempt = 1; attempt <= _options.MaxAttempts; attempt++)
                        {
                            if (cancellationToken.IsCancellationRequested) break;
                            pingRequest = _requestModelFactory.Create();
                            pingRequest.Ttl = Defaults.Ttl;
                            pingRequest.Address = hopAddress;
                            

                            pingResponse = await _pingHandler.Execute(pingRequest, cancellationToken);

                            SetAttempt(pingResponse, attempt, _options.MaxAttempts, hop);

                            RecordResult(responses, pingResponse);
                        }
                        if (exitOnCompletion) break;
                    }

                }
                catch (Exception ex)
                {
                    System.Diagnostics.Trace.WriteLine($"{nameof(TraceRouteOrchestrator)}.{nameof(Execute)} Exception encountered and ignored: {ex.Message}");
                }
            }
            return responses;
        }

        private static void SetAttempt(PingResponseModel pingResponse, int attempt, int maxAttempts, int hop)
        {
            pingResponse.Attempt = attempt;
            pingResponse.Hop = hop;
            pingResponse.MaxAttempts = maxAttempts;
        }

        private static void RecordResult(PingResponses responses, PingResponseModel pingResponse)
        {
            responses.TryAdd(new(pingResponse.Start, pingResponse.Request.Address), pingResponse);
        }

    }
}
