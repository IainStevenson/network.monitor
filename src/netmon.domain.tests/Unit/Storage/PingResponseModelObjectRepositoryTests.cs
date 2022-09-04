using Microsoft.Extensions.Logging;
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
        private ILogger<PingResponseModelObjectRepository> _logger;

        [SetUp]
        public new void Setup()
        {

            base.Setup();
            _collection = Substitute.For<IMongoCollection<PingResponseModel>>();
            _logger = Substitute.For<ILogger<PingResponseModelObjectRepository>>();
            _unit = new PingResponseModelObjectRepository(_collection, _logger);
        }


        [Test]
        public void ItShouldDeclareItCanStore()
        {
            // Arrange

            // Act
            var actual = _unit.Capabilities.HasFlag(Interfaces.Repositories.RepositoryCapabilities.Store);
            // Assert
            Assert.That(actual, Is.True);

        }


        [Test]
        public void OnStoreItShouldStoreTheItemViaFindAndReplace()
        {
            // Arrange
            var item = new PingResponseModel();

            // Act
            _unit.StoreAsync(item).Wait();

            // Assert
            _collection
                .Received(1)
                .ReplaceOneAsync(Arg.Any<ExpressionFilterDefinition<PingResponseModel>>(), item, Arg.Any<ReplaceOptions>());


        }

    }
}