using Microsoft.Extensions.Logging;
using netmon.core.Interfaces;
using netmon.core.Interfaces.Repositories;
using netmon.core.Models;
using Newtonsoft.Json;

namespace netmon.core.Orchestrators
{
    /// <summary>
    /// Handles a variety of storage options for <see cref="PingResponseModel"/>.
    /// </summary>
    public class PingResponseModelStorageOrchestrator : IStorageOrchestrator<PingResponseModel>
    {
        private readonly IEnumerable<IRepository> _repositories;
        private readonly ILogger<PingResponseModelStorageOrchestrator> _logger;
        private readonly JsonSerializerSettings _jsonSerializerSettings;


        public PingResponseModelStorageOrchestrator(
                IEnumerable<IRepository> repositories,
                ILogger<PingResponseModelStorageOrchestrator> logger,
                JsonSerializerSettings jsonSerializerSettings)
        {
            _repositories = repositories;
            _logger = logger;
            _jsonSerializerSettings = jsonSerializerSettings;
        }

        /// <summary>
        /// Store the item in all of the storage capable repositories.
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public Task Store(PingResponseModel item)
        {
            _logger.LogTrace("Storing item {start} {address} {response} {identifier}", item.Start, item.Request.Address.ToString(), item.Response?.RoundtripTime ?? 0, item.Id);
            var tasks = _repositories
                    .Where(w => (RepositoryCapabilities.Store | w.Capabilities) == RepositoryCapabilities.Store)
                    .Select(async (repository) => await ((IStorageRepository<Guid, PingResponseModel>)repository).StoreAsync(item)).ToArray();
            Task.WaitAll(tasks);
            return Task.FromResult(0);
        }
    }
}
