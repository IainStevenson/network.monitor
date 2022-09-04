using MongoDB.Driver;
using netmon.domain.Configuration;
using netmon.domain.Handlers;
using netmon.domain.Interfaces;
using netmon.domain.Interfaces.Repositories;
using netmon.domain.Messaging;
using netmon.domain.Models;
using System.Net;

namespace netmon.domain.Orchestrators
{

    /// <summary>
    /// Handles complex ping tasks via the <see cref="PingHandler"/> and and returns the results  as <see cref="PingResponses"/>.
    /// </summary>
    public class PingOrchestrator : IPingOrchestrator
    {
        private readonly IPingHandler _pingHandler;
        private readonly IPingRequestModelFactory _pingRequestModelFactory;
        private readonly PingOrchestratorOptions _options;
        private readonly IEnumerable<IRepository> _repositories;
        public PingOrchestrator(
            IPingHandler pingHandler,
            IPingRequestModelFactory pingRequestModelFactory,
            PingOrchestratorOptions options,
             IEnumerable<IRepository> repositories)
        {
            _pingHandler = pingHandler;
            _pingRequestModelFactory = pingRequestModelFactory;
            _options = options;
            _repositories = repositories;
        }

        public async Task<PingResponseModels> Ping(IPAddress[] addresses, CancellationToken cancellation)
        {
            var pauseTimeBetweenInstances = new TimeSpan(_options.MillisecondsBetweenPings * 10000);
            var parallelTasks = new List<Task<PingResponseModel>>(addresses.Length);
            var responses = new PingResponseModels();

            for (int i = 0; i < addresses.Length; i++)
            {
                var request = _pingRequestModelFactory.Create(addresses[i]);

                Task<PingResponseModel> task = _pingHandler.Execute(request, cancellation);
                parallelTasks.Add(task);
            }

            if (cancellation.IsCancellationRequested) return new PingResponseModels();

            await Task.WhenAll(parallelTasks);

            var results = parallelTasks.Select(x => x.Result);

            ProcessResults(responses, results);

            return responses;
        }

        private void ProcessResults(PingResponseModels responses, IEnumerable<PingResponseModel> results)
        {
            
            foreach (var result in results)
            {
                responses.TryAdd(new Tuple<DateTimeOffset, IPAddress>(result.Start, result.Request.Address), result);
                var tasks = _repositories
                                 .Where(w => w.Capabilities.HasFlag(RepositoryCapabilities.Store))
                                 .Select(async (repository) => await ((IStorageRepository<Guid, PingResponseModel>)repository).StoreAsync(result)).ToArray();
                Task.WaitAll(tasks);
            }
           
        }

        public async Task<PingResponseModels> PingUntil(IPAddress[] addresses, TimeSpan until, CancellationToken cancellation)
        {
            var pauseTimeBetweenInstances = new TimeSpan(_options.MillisecondsBetweenPings * 10000);
            var end = DateTimeOffset.UtcNow.Add(until);
            var responses = new PingResponseModels();

            while (DateTimeOffset.UtcNow < end && !cancellation.IsCancellationRequested)
            {
                if (!cancellation.IsCancellationRequested)
                {
                    var looptime = DateTimeOffset.UtcNow.Add(pauseTimeBetweenInstances);
                    while (responses.Any() && DateTimeOffset.UtcNow < looptime)
                    {
                        Thread.Sleep((int)(pauseTimeBetweenInstances.Milliseconds / 10));
                    }
                }
                var parallelTasks = new List<Task<PingResponseModel>>(addresses.Length);

                for (int i = 0; i < addresses.Length; i++)
                {
                    var request = _pingRequestModelFactory.Create(addresses[i]);


                    Task<PingResponseModel> task = _pingHandler.Execute(request, cancellation);
                    parallelTasks.Add(task);
                }

                await Task.WhenAll(parallelTasks);

                var results = parallelTasks.Select(x => x.Result);

                ProcessResults(responses, results);

            }

            return responses;
        }

    }
}
