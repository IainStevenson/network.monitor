using MongoDB.Driver;
using netmon.domain.Models;
using netmon.domain.Storage;
using NSubstitute;

namespace netmon.domain.tests.Integration.Storage
{
    [TestFixture]
    public class PingResponseModelObjectRepositoryTests : TestBase<PingResponseModelObjectRepository>
    {
        private IMongoCollection<PingResponseModel> _collection;

        [SetUp]
        public new void Setup()
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