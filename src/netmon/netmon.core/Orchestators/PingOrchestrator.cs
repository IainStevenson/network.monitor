using netmon.core.Data;
using netmon.core.Handlers;
using netmon.core.Models;
using System.Collections.Concurrent;
using System.Net;

namespace netmon.core.Orchestators
{
    /// <summary>
    /// Handles complex ping tasks and recording results via the <see cref="PingHandler"/> and <see cref="PingResponses"/>.
    /// </summary>
    public class PingOrchestrator
    {
        private TimeSpan _pauseTimeBetweenInstances = new TimeSpan(0, 0, 1);

        public async Task<PingResponses> PingManyUntil(IPAddress[] addresses, TimeSpan until, CancellationToken cancellation)
        {

            var end  = DateTimeOffset.UtcNow.Add(until);
            var responses = new PingResponses();

            while (DateTimeOffset.UtcNow < end)
            {
                var looptime = DateTimeOffset.UtcNow.Add(_pauseTimeBetweenInstances);
                var parallelTasks = new List<Task<PingResponseModel>>(addresses.Length);
                
                for (int i = 0; i < addresses.Length; i++)
                {
                    var request = new PingRequestModel() { Address = addresses[i] };
                    Task<PingResponseModel> task = new PingHandler().Execute(request, cancellation);
                    parallelTasks.Add(task);
                }

                await Task.WhenAll(parallelTasks);

                var results = parallelTasks.Select(x => x.Result);
                foreach (var result in results)
                {
                    responses.TryAdd(new Tuple<DateTimeOffset, IPAddress>( result.Start, result.Request.Address), result);
                }
                while (DateTimeOffset.UtcNow < looptime)
                {
                    Thread.Sleep((int)(_pauseTimeBetweenInstances.Milliseconds / 10));
                }
            }
            return responses;
        }
    }
}