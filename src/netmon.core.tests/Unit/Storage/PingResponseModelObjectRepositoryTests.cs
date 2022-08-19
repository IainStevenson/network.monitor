using MongoDB.Driver;
using netmon.core.Models;
using netmon.core.Storage;
using NSubstitute;

namespace netmon.core.tests.Integration.Storage
{
    [TestFixture]
    public class PingResponseModelObjectRepositoryTests : TestBase<PingResponseModelObjectRepository>
    {
        private IMongoCollection<PingResponseModel> _collection;

        [SetUp]
        public void Setup()
        {

            base.Setup();
            _collection = Substitute.For<IMongoCollection<PingResponseModel>>();

            _unit = new PingResponseModelObjectRepository(_collection);
        }

        [Test]
        public void OnStoreItShouldStoreTheItemViaFindAndReplace()
        {
            // Arrange
            var item = new PingResponseModel();

            // Act
            _unit.StoreAsync( item).Wait();

            // Assert
            _collection
                .Received(1)
                .ReplaceOneAsync(Arg.Any<ExpressionFilterDefinition<PingResponseModel>>(), item, Arg.Any<ReplaceOptions>());
           

        }

    }
}