using netmon.core.Models;
using MongoDB.Driver;
using netmon.core.Interfaces.Repositories;

namespace netmon.core.Storage
{
    /// <summary>
    /// Implements Store and retrieval operations for <see cref="PingResponseModel"/> via MongoDB object storage.
    /// </summary>
    public class PingResponseModelObjectRepository : IStorageRepository<Guid, PingResponseModel>, IRepository
    {
        public RepositoryCapabilities Capabilities => RepositoryCapabilities.Store;

        private readonly bool? _byPassDocumentValidation = true;
        private readonly IMongoCollection<PingResponseModel> _collection;

        public PingResponseModelObjectRepository(IMongoCollection<PingResponseModel> collection)
        {            
            _collection = collection;
        }

        public async Task StoreAsync(PingResponseModel item)
        {
            var filter = new ExpressionFilterDefinition<PingResponseModel>(i => i.Id == item.Id);

            var options = new ReplaceOptions
            {
                BypassDocumentValidation = _byPassDocumentValidation,
                IsUpsert = true,
            };

            await  _collection.ReplaceOneAsync(filter, item, options);


        }
    }
}
