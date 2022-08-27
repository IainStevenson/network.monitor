using netmon.domain.Interfaces.Repositories;
using netmon.domain.Models;
using System.Collections.Concurrent;
using System.Collections.Generic;

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
        public RepositoryCapabilities Capabilities => RepositoryCapabilities.Store ^ RepositoryCapabilities.Retrieve ^ RepositoryCapabilities.Delete;

        private readonly ConcurrentDictionary<Guid, PingResponseModel> _storage;

        public PingResponseModelInMemoryRepository(ConcurrentDictionary<Guid, PingResponseModel> storage)
        {
            _storage = storage;
        }

        public Task DeleteAsync(Guid id)
        {
            _ = _storage.Remove(id, out _);
            return Task.FromResult(0);
        }

        public Task<PingResponseModel?> RetrieveAsync(Guid id)
        {
            var result = _storage.GetValueOrDefault(id, null);
            return Task.FromResult(result);
        }

        public Task StoreAsync(PingResponseModel item)
        {
            _storage.AddOrUpdate(key: item.Id, addValue: item, updateValueFactory: (k, i) => i = item);
            return Task.FromResult(0);
        }
    }
}
