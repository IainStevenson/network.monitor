using netmon.core.Interfaces.Repositories;
using netmon.core.Models;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace netmon.core.Storage
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

        public Task<PingResponseModel> RetrieveAsync(Guid id)
        {
            return Task.FromResult(_storage.GetValueOrDefault(id, new PingResponseModel() { Id = Guid.Empty }));
        }

        public Task StoreAsync(PingResponseModel item)
        {
            _storage.AddOrUpdate(key: item.Id, addValue: item, updateValueFactory: (k, i) => i = item);
            return Task.FromResult(0);
        }
    }
}
