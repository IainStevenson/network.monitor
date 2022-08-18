using netmon.core.Interfaces;
using netmon.core.Models;
using System.Collections.Concurrent;

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
        private readonly ConcurrentDictionary<Guid, PingResponseModel> _storage = new();

        public Task DeleteAsync(Guid id)
        {
            
            if (_storage.ContainsKey(id))
            {
                _storage.Remove(id, out _);
            }
            return Task.FromResult(0);
        }

        public RepositoryCapabilities Capabilities =>
            RepositoryCapabilities.Store ^
            RepositoryCapabilities.Retrieve ^
            RepositoryCapabilities.Delete;


        public Task<PingResponseModel?> RetrieveAsync(Guid id)
        {
            if (_storage.ContainsKey(id))
            {
                Task.FromResult(_storage[id]);
            }
            return Task.FromResult(null as PingResponseModel);
        }


        public Task StoreAsync(PingResponseModel item)
        {
            if (_storage.ContainsKey(item.Id))
            {
                _storage[item.Id] = item;
            }
            else
            {
                _storage.TryAdd(item.Id, item);
            }
            return Task.FromResult(0);
        }

       
    }

}
