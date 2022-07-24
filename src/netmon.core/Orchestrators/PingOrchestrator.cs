using netmon.core.Configuration;
using netmon.core.Handlers;
using netmon.core.Interfaces;
using netmon.core.Messaging;
using netmon.core.Models;
using System.Net;

namespace netmon.core.Orchestrators
{
    /// <summary>
    /// Handles complex ping tasks and recording results via the <see cref="PingHandler"/> and <see cref="PingResponses"/>.
    /// </summary>
    public class PingOrchestrator
    {
        private readonly IPingHandler _pingHandler;
        private readonly IPingRequestModelFactory _pingRequestModelFactory;
        private readonly PingOrchestratorOptions _options;

        public PingOrchestrator(IPingHandler pingHandler, IPingRequestModelFactory pingRequestModelFactory, PingOrchestratorOptions options)
        {
            _pingHandler = pingHandler;
            _pingRequestModelFactory = pingRequestModelFactory;  
            _options = options;
        }

        public EventHandler<PingResponseModelEventArgs> Results ;

        public async Task<PingResponses> PingUntil(IPAddress[] addresses, TimeSpan until, CancellationToken cancellation)
        {
            var pauseTimeBetweenInstances = new TimeSpan( _options.MillisecondsBetweenPings * 10000);
            var end = DateTimeOffset.UtcNow.Add(until);
            var responses = new PingResponses();

            while (DateTimeOffset.UtcNow < end && !cancellation.IsCancellationRequested)
            {
                var looptime = DateTimeOffset.UtcNow.Add(pauseTimeBetweenInstances);
                var parallelTasks = new List<Task<PingResponseModel>>(addresses.Length);

                for (int i = 0; i < addresses.Length; i++)
                {
                    var request = _pingRequestModelFactory.Create();
                    request.Address = addresses[i];
                    Task<PingResponseModel> task = _pingHandler.Execute(request, cancellation);
                    parallelTasks.Add(task);
                }

                await Task.WhenAll(parallelTasks);

                var results = parallelTasks.Select(x => x.Result);
                foreach (var result in results)
                {
                    responses.TryAdd(new Tuple<DateTimeOffset, IPAddress>(result.Start, result.Request.Address), result);
                    Results?.Invoke(this, new PingResponseModelEventArgs(result));
                }
                if (!cancellation.IsCancellationRequested)
                {
                    while (DateTimeOffset.UtcNow < looptime)
                    {
                        Thread.Sleep((int)(pauseTimeBetweenInstances.Milliseconds / 10));
                    }
                }
            }

            return responses;
        }

    }
}
