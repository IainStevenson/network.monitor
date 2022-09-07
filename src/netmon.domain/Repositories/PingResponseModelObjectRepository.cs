using netmon.domain.Models;
using MongoDB.Driver;
using netmon.domain.Interfaces.Repositories;
using Microsoft.Extensions.Logging;

namespace netmon.domain.Storage
{
    /// <summary>
    /// Implements Store and retrieval operations for <see cref="PingResponseModel"/> via MongoDB object storage.
    /// </summary>
    public class PingResponseModelObjectRepository : IStorageRepository<Guid, PingResponseModel>, IRepository
    {
        public RepositoryCapabilities Capabilities => RepositoryCapabilities.Store;

        private ILogger<PingResponseModelObjectRepository> _logger;
        private readonly bool? _byPassDocumentValidation = true;
        private readonly IMongoCollection<PingResponseModel> _collection;

        public PingResponseModelObjectRepository(IMongoCollection<PingResponseModel> collection, ILogger<PingResponseModelObjectRepository> logger)
        {            
            _collection = collection;
            _logger = logger;   
        }

        public async Task StoreAsync(PingResponseModel item)
        {
            _logger.LogTrace("Storing item {start} {address} {response} {identifier}", item.Start, item.Request.Address.ToString(), item.Response?.RoundtripTime ?? 0, item.Id);
            var filter = new ExpressionFilterDefinition<PingResponseModel>(i => i.Id == item.Id);

            var options = new ReplaceOptions
            {
                BypassDocumentValidation = _byPassDocumentValidation,
                IsUpsert = true
            };

            await  _collection.ReplaceOneAsync(filter, item, options);

        }
    }
}
