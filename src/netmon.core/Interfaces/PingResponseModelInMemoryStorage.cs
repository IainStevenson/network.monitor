using netmon.core.Models;
using System.Collections.Concurrent;
using System.Net;

namespace netmon.core.Interfaces
{
    /// <summary>
    /// Implements and in memory store of <see cref="PingRequestModel"/>. 
    /// Stores the resusts of pings of target addresses.
    /// </summary>
    public class PingResponseModelInMemoryStorage : IStorage<PingResponseModel>
    {
        private readonly ConcurrentDictionary<long, PingResponseModel> _storage = new();

        public int Count()
        {
            return _storage.Count;
        }

        public Task<IEnumerable<PingResponseModel>> Retrieve(IEnumerable<IPAddress> keys)
        {
            return Task.FromResult(_storage.Values.Where( w=>keys.Contains(w.Response.Address)).AsEnumerable());
        }

        public Task<IEnumerable<PingResponseModel>> Retrieve(Func<PingResponseModel, bool> predicate)
        {
            var results = _storage.Values.Where(predicate).AsEnumerable();
            return Task.FromResult(results);
        }

        public Task Store(PingResponseModel item)
        {
            _storage.TryAdd(DateTimeOffset.UtcNow.Ticks, item);
            return Task.FromResult(0);
        }
    }

}
