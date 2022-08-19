using MongoDB.Driver;
using netmon.core.Messaging;
using netmon.core.Storage;
using System.Net;

namespace netmon.core.tests.Integration.Storage
{
    [TestFixture]
    public class InMemoryStorageTests : TestBase<PingResponseModelInMemoryRepository>
    {
        private PingResponses _TestData = new();
        [SetUp]
        public override void Setup()
        {
            base.Setup();
            _unit = new PingResponseModelInMemoryRepository();
        }

        private void AddWorldAddressesTestData()
        {
            _TestData = new PingResponses();
            // simualte traceroute response.
            List<bool> states = new();
            foreach (var address in TestConditions.WorldAddresses)
            {
                states.Add(_TestData.TryAdd(
                      new Tuple<DateTimeOffset, IPAddress>(DateTimeOffset.UtcNow, address),
                      new Models.PingResponseModel() { Request = new Models.PingRequestModel() { Address = address } })
                    );
            }
            if (states.Any(a => !a)) return; // fail the test
            // load it into storage as needed.
            foreach (var item in _TestData)
            {
                _unit.StoreAsync(item.Value).Wait();
            }
        }

        [Test]
        [Category("Unit")]
        public async Task OnStoreItShouldContainTheAddedItems()
        {

            AddWorldAddressesTestData();

            foreach (var item in _TestData)
            {
                var response = await _unit.RetrieveAsync(item.Value.Id);
                Assert.That(response, Is.Not.Null);
                Assert.That(response.Id, Is.EqualTo(item.Value.Id));
            }


        }



    }
}