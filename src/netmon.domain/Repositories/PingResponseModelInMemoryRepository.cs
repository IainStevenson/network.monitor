using DnsClient.Internal;
using Microsoft.Extensions.Logging;
using netmon.domain.Interfaces.Repositories;
using netmon.domain.Models;
using System.Collections.Concurrent;

namespace netmon.domain.Storage
{

    /// <summary>
    /// Implements and in memory store of <see cref="PingRequestModel"/>. 
    /// Stores the resusts of pings of target addresses.
    /// </summary>
    public class PingResponseModelInMemoryRepository :
            IStorageRepository<Guid, PingResponseModel>,
            IRetrieveRepository<Guid, PingResponseModel>,
            IDeletionRepository<Guid, PingResponseModel>, IRepository
    {
        public RepositoryCapabilities Capabilities => (RepositoryCapabilities.Store | RepositoryCapabilities.Retrieve | RepositoryCapabilities.Delete);
        private ILogger<PingResponseModelInMemoryRepository> _logger;
        private readonly ConcurrentDictionary<Guid, PingResponseModel> _storage;



        public PingResponseModelInMemoryRepository(
            ConcurrentDictionary<Guid, PingResponseModel> storage, 
            ILogger<PingResponseModelInMemoryRepository> logger)
        {
            _storage = storage;
            _logger = logger;
        }

        public Task DeleteAsync(Guid id)
        {
            _logger.LogTrace("Removing item {identifier} ", id);
            _ = _storage.Remove(id, out _);
            return Task.FromResult(0);
        }

        public Task<PingResponseModel?> RetrieveAsync(Guid id)
        {
            _logger.LogTrace("Retrieving item {identifier}", id);
            PingResponseModel? response = null;
            if (_storage.ContainsKey(id))
                response = _storage[id];
            return Task.FromResult(response);
        }

        public Task StoreAsync(PingResponseModel item)
        {
            _logger.LogTrace("Storing item {start} {address} {response} {identifier}", item.Start, item.Request.Address.ToString(), item.Response?.RoundtripTime ?? 0, item.Id);
            _storage.AddOrUpdate(key: item.Id, addValue: item, updateValueFactory: (k, i) => i = item);
            return Task.FromResult(0);
        }
    }
}
