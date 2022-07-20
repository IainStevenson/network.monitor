using netmon.core.Models;
using System.Collections.Concurrent;
using System.Linq;
using System.Net;

namespace netmon.core.Handlers
{

    public class InMemoryStorage
    {
        private readonly BlockingCollection<PingResponseModel> _storage = new BlockingCollection<PingResponseModel>();

        public int Count()
        {
            return _storage.Count;
        }

        //public Task<IEnumerable<PingResponseModel>> Retrieve(IEnumerable<IPAddress> keys)
        //{
        //    return Task.FromResult(_storage.Where(x => keys.Contains(x.Response.Address)).AsEnumerable());
        //}

        //public Task<IEnumerable<PingResponseModel>> Retrieve(Func<PingResponseModel, bool> predicate)
        //{
        //    var results = _storage.Where<PingResponseModel>(predicate);
        //    return Task.FromResult(results);
        //}

        public Task Store( PingResponseModel item)
        {
            _storage.Add(item);
            return Task.FromResult(0);
        }
    }

}
