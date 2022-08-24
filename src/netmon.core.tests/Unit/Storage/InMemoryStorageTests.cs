using MongoDB.Driver;
using netmon.core.Messaging;
using netmon.core.Models;
using netmon.core.Storage;
using System.Collections.Concurrent;
using System.Net;

namespace netmon.core.tests.Integration.Storage
{
    [TestFixture]
    public class InMemoryStorageTests : TestBase<PingResponseModelInMemoryRepository>
    {
        private PingResponses _TestData = new();
        private ConcurrentDictionary<Guid, PingResponseModel> _storage;
        [SetUp]
        public override void Setup()
        {
            base.Setup();
            _TestData = new PingResponses();
            // simulate traceroute response.
            List<bool> states = new();
            foreach (var address in TestConditions.WorldAddresses)
            {
                states.Add(_TestData.TryAdd(
                      new Tuple<DateTimeOffset, IPAddress>(DateTimeOffset.UtcNow, address),
                      new Models.PingResponseModel() { Request = new Models.PingRequestModel() { Address = address } })
                    );
            }
            _storage = new();
            _unit = new PingResponseModelInMemoryRepository(_storage);
        }

        [Test]
        [Category("Unit")]
        public async Task OnRetrieveItShouldContainAllOfTheAddedItems()
        {

            // Arrange
            foreach (var item in _TestData)
            {
                await _unit.StoreAsync(item.Value);
            }
            Assert.That(_storage, Has.Count.EqualTo(_TestData.Count));
            // Act

            foreach (var item in _TestData)
            {
                var response = await _unit.RetrieveAsync(item.Value.Id);
                Assert.That(response, Is.Not.Null);
                Assert.That(response.Id, Is.EqualTo(item.Value.Id));
            }
        }

        [Test]
        [Category("Unit")]
        public async Task OnStoreItShouldContainTheUniqueAddedItems()
        {
            // Arrange

            Assert.That( _storage, Is.Empty);

             // Act
            foreach (var item in _TestData)
            {
                await _unit.StoreAsync(item.Value);
                await _unit.StoreAsync(item.Value); // note that this is idempotent                
            }

            // Assert
            Assert.That(_storage, Has.Count.EqualTo(_TestData.Count)); // proves it is idempotent
            
        }

        [Test]
        [Category("Unit")]
        public async Task OnDeleteItShouldNoLongerContainTheUniqueAddedItems()
        {
            // Arrange
            foreach (var item in _TestData)
            {
                await _unit.StoreAsync(item.Value);
            }
            Assert.That(_storage, Has.Count.EqualTo(_TestData.Count));

            // Act
            foreach (var item in _TestData)
            {
                await _unit.DeleteAsync(item.Value.Id);
            }

            // Assert
            Assert.That(_storage, Is.Empty);
        }
    }
}