using netmon.core.Interfaces;
using netmon.core.Models;
using MongoDB.Driver;
using MongoDB.Bson;

namespace netmon.core.Storage
{
    /// <summary>
    /// Implements Store and retrieval operations for <see cref="PingResponseModel"/> via MongoDB object storage.
    /// </summary>
    public class PingResponseModelObjectRepository : IStorageRepository<Guid, PingResponseModel>, IRepository
    {
        private readonly bool? _byPassDocumentValidation = true;
        private MongoClient dbClient;
        private IMongoCollection<PingResponseModel> _collection;

        public PingResponseModelObjectRepository(string connectionString, string databaseName, string collectionName)
        {
            dbClient = new MongoClient(connectionString);
            _collection = dbClient.GetDatabase(databaseName)
                .GetCollection<PingResponseModel>(collectionName);
        }
        public RepositoryCapabilities Capabilities =>
           RepositoryCapabilities.Store;

        public async Task StoreAsync(PingResponseModel item)
        {

            await _collection.InsertOneAsync(item, new InsertOneOptions
            {
                BypassDocumentValidation = _byPassDocumentValidation
            });

        }
    }
}
