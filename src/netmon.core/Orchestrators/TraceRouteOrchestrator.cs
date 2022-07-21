using netmon.core.Configuration;
using netmon.core.Data;
using netmon.core.Handlers;
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
        /// </summary>
        /// <param name="iPAddress">The network address to trace.</param>
        /// <param name="cancellationToken">the asunc cancellation token.</param>
        /// <returns></returns>
        public async Task<PingResponses> Execute(IPAddress iPAddress, CancellationToken cancellationToken)
        {
            var responses = new PingResponses();
            _pingHandler.Options.Ttl = 1;
            
            for (var hop = 1; hop <= _options.MaxHops; hop++)
            {
                if (cancellationToken.IsCancellationRequested) break;

                var pingRequest = _requestModelFactory.Create(_pingHandler.Options);

                pingRequest.Address = iPAddress;
                
                for (var attempt = 1; attempt <= _options.MaxAttempts; attempt++)
                {
                    if (cancellationToken.IsCancellationRequested) break;

                    try
                    {

                        PingResponseModel pingResponse = await AttemptRequest(pingRequest, attempt, hop, cancellationToken);

                        responses.TryAdd(new Tuple<DateTimeOffset, IPAddress>(pingResponse.Start, pingResponse.Request.Address), pingResponse);

                      
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Trace.WriteLine($"{nameof(TraceRouteOrchestrator)}.{nameof(Execute)} Exception encountered and ignored: {ex.Message}");
                    }

                }
                _pingHandler.Options.Ttl += 1;
            }
            _pingHandler.Options.Ttl = Defaults.Ttl;
            return responses;
        }

        private async Task<PingResponseModel> AttemptRequest(PingRequestModel pingRequest, int attempt, int hop, CancellationToken cancellationToken)
        {
            var pingResponse = await _pingHandler.Execute(pingRequest, cancellationToken);

            pingResponse.Attempt = attempt;
            pingResponse.Hop = hop;
            pingResponse.MaxAttempts = _options.MaxAttempts;
            pingResponse.Ttl = _pingHandler.Options.Ttl;


            return pingResponse;
        }
    }
}
