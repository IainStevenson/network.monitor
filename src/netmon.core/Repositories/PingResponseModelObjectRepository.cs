using netmon.core.Models;
using MongoDB.Driver;
using netmon.core.Interfaces.Repositories;
using MongoDB.Bson;

namespace netmon.core.Storage
{
    /// <summary>
    /// Implements Store and retrieval operations for <see cref="PingResponseModel"/> via MongoDB object storage.
    /// </summary>
    public class PingResponseModelObjectRepository : IStorageRepository<Guid, PingResponseModel>, IRepository
    {
        public RepositoryCapabilities Capabilities => RepositoryCapabilities.Store;

        private readonly bool? _byPassDocumentValidation = true;
        private readonly MongoClient _dbClient;
        private readonly IMongoCollection<PingResponseModel> _collection;

        public PingResponseModelObjectRepository(string connectionString, string databaseName, string collectionName)
        {
            _dbClient = new MongoClient(connectionString);
            _collection = _dbClient.GetDatabase(databaseName).GetCollection<PingResponseModel>(collectionName);
        }

        public async Task StoreAsync(PingResponseModel item)
        {
            var exists = await _collection.FindAsync<PingResponseModel>(new ExpressionFilterDefinition<PingResponseModel>(i => i.Id == item.Id));

            if (exists.Current.Any())
            {
                await _collection.ReplaceOneAsync(r => r.Id == item.Id, item);
            }
            else
            {
                await _collection.InsertOneAsync(item, new InsertOneOptions
                {
                    BypassDocumentValidation = _byPassDocumentValidation
                });
            }

        }
    }
}
