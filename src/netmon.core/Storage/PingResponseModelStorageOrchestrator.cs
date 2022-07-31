using Microsoft.Extensions.Logging;
using netmon.core.Interfaces;
using netmon.core.Models;

namespace netmon.core.Storage
{
    /// <summary>
    /// Handles a variety of storage options for <see cref="PingResponseModel"/>.
    /// </summary>
    public class PingResponseModelStorageOrchestrator : IPingResponseModelStorageOrchestrator
    {
        private readonly IEnumerable<IStorage<PingResponseModel>> _repositories;
        private readonly ILogger<PingResponseModelStorageOrchestrator> _logger;

        public PingResponseModelStorageOrchestrator(
                IEnumerable<IStorage<PingResponseModel>> repositories,
                ILogger<PingResponseModelStorageOrchestrator> logger)
        {
            _repositories = repositories;
            _logger = logger;
        }
        public Task Store(PingResponseModel item)
        {
            _logger.LogTrace("Storing item {start} {address}", item.Start, item.Request.Address.ToString());
            var tasks = _repositories.Select(async (repository) => await repository.Store(item)).ToArray();
            Task.WaitAll(tasks);
            return Task.FromResult(0);
        }
    }

}
