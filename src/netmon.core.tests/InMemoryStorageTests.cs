using netmon.core.Interfaces;
using netmon.core.Messaging;
using System.Net;

namespace netmon.core.tests
{
    public class InMemoryStorageTests : TestBase<PingResponseModelInMemoryStorage>
    {
        [SetUp]
        public override void Setup()
        {
            base.Setup();
            _unit = new PingResponseModelInMemoryStorage();
        }

        private void AddWorldAddressesTestData()
        {
            var items = new PingResponses();
            // simualte traceroute response.
            List<bool> states = new List<bool>();
            foreach (var address in TestConditions.WorldAddresses)
            {
                states.Add(items.TryAdd(
                      new Tuple<DateTimeOffset, IPAddress>(DateTimeOffset.UtcNow, address),
                      new Models.PingResponseModel() { Request = new Models.PingRequestModel() { Address = address } })
                    );
            }
            if (states.Any(a => !a)) return; // fail the test
            // load it into storage as needed.
            foreach (var item in items)
            {
                _unit.Store(item.Value);
            }
        }

        [Test]
        [Category("Unit")]
        public void OnStoreItShouldContainTheAddedItems()
        {
            Assert.That(_unit.Count, Is.EqualTo(0));
            AddWorldAddressesTestData();
            Assert.That(_unit.Count, Is.EqualTo(TestConditions.WorldAddresses.Length));
        }


        [Test]
        [Category("Unit")]
        public async Task OnRetrieveByAddressItShouldReturnTheMatchingItems()
        {
            Assert.That(_unit.Count, Is.EqualTo(0));
            AddWorldAddressesTestData();
            var addresses = new List<IPAddress>() { };
            addresses.AddRange(TestConditions.WorldAddresses);
            addresses.AddRange(TestConditions.LocalAddresses);

            var actual = await _unit.Retrieve(TestConditions.WorldAddresses);
            Assert.That(actual.ToList(), Has.Count.EqualTo(TestConditions.WorldAddresses.Length));
        }


        [Test]
        [Category("Unit")]
        public async Task OnRetrieveByAddressPredicateItShouldReturnTheExactItem()
        {
            Assert.That(_unit.Count, Is.EqualTo(0));
            AddWorldAddressesTestData();
            var actual = await _unit.Retrieve( (x) => x.Request.Address == TestConditions.WorldAddresses.Last());
            Assert.That(actual.ToList(), Has.Count.EqualTo(1));
            Assert.That(actual.Last().Request.Address, Is.EqualTo(IPAddress.Parse("8.8.8.8")));
        }

    }
}