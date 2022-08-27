using Microsoft.Extensions.Logging;
using netmon.domain.Configuration;
using netmon.domain.Data;
using netmon.domain.Interfaces;
using netmon.domain.Messaging;
using netmon.domain.Models;
using System.Net;
using System.Net.NetworkInformation;

namespace netmon.domain.Orchestrators
{

    /// <summary>
    /// Orchestrates a traceroute using an instance of  <see cref="IPingHandler"/> and obtains raw response data.
    /// </summary>
    public class TraceRouteOrchestrator : ITraceRouteOrchestrator
    {
        private readonly IPingHandler _pingHandler;
        private readonly TraceRouteOrchestratorOptions _options;
        private readonly IPingRequestModelFactory _requestModelFactory;
        private readonly ILogger<TraceRouteOrchestrator> _logger;

        public event EventHandler<PingResponseModelEventArgs>? Results;

        public TraceRouteOrchestrator(IPingHandler pingHandler, TraceRouteOrchestratorOptions options, IPingRequestModelFactory requestModelFactory,
            ILogger<TraceRouteOrchestrator> logger)
        {
            _pingHandler = pingHandler;
            _options = options;
            _requestModelFactory = requestModelFactory;
            _logger=logger;
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
            _logger.LogTrace("Tracing route to {iPAddress}", iPAddress);

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

                    
                    RecordResultIfNotNull(responses, pingResponse);

                    var reply = ReponseIsOfInterest(pingResponse);
                    if (reply != null)
                    {
                        var hopAddress = reply.Address;
                        await GetPingStatisticsForAddress(responses, hop, hopAddress, cancellationToken);
                        if (reply.Status == IPStatus.Success) break;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError("{handler}{method} Exception encountered and ignored: {message}", nameof(TraceRouteOrchestrator), nameof(Execute) , ex.Message);

                    RecordResultIfNotNull(responses, new PingResponseModel() { Request = pingRequest, Exception = ex });

                }
            }
            return responses;
        }

        private async Task GetPingStatisticsForAddress(PingResponses responses, int hop, IPAddress hopAddress, CancellationToken cancellationToken)
        {
            for (var attempt = 1; attempt <= _options.MaxAttempts; attempt++)
            {
                if (cancellationToken.IsCancellationRequested) break;

                var pingRequest = _requestModelFactory.Create();
                pingRequest.Ttl = Defaults.Ttl;
                pingRequest.Address = hopAddress;


                var pingResponse = await _pingHandler.Execute(pingRequest, cancellationToken);

                SetAttempt(pingResponse, attempt, _options.MaxAttempts, hop);

                RecordResultIfNotNull(responses, pingResponse);
            }

        }

        /// <summary>
        /// Return the orchestrator response reply from the ping, if it exists, and if it is either TtlExpired or Success status.
        /// </summary>
        /// <param name="pingResponse"></param>
        /// <returns></returns>
        private static PingReplyModel? ReponseIsOfInterest(PingResponseModel pingResponse)
        {
            if (pingResponse == null) return null;
            if (pingResponse.Response == null) return null;
            if (pingResponse.Response.Status != IPStatus.TtlExpired && pingResponse.Response.Status != IPStatus.Success) return null;
            return pingResponse.Response;
        }

        private static void SetAttempt(PingResponseModel pingResponse, int attempt, int maxAttempts, int hop)
        {
            pingResponse.Attempt = attempt;
            pingResponse.Hop = hop;
            pingResponse.MaxAttempts = maxAttempts;
        }

        private void RecordResultIfNotNull(PingResponses responses, PingResponseModel pingResponse)
        {
            if (pingResponse == null) return;
            Results?.Invoke(this, new PingResponseModelEventArgs(pingResponse));

            responses.TryAdd(new(pingResponse.Start, pingResponse.Request.Address), pingResponse);
        }

    }
}
