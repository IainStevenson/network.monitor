using netmon.core.Data;
using netmon.core.Handlers;
using System.Net;

namespace netmon.core.tests
{
    public class InMemoryStorageTests: TestBase<InMemoryStorage>
    {
        [SetUp] 
        public override void Setup() 
        { 
            base.Setup();
            _unit = new InMemoryStorage();
        }

        private void AddTestData()
        {
            var items = new PingResponses();
            // simualte traceroute response.
            foreach(var address in TestConditions.WorldAddresses)
            {
                items.TryAdd(new Tuple<DateTimeOffset, IPAddress>(DateTimeOffset.UtcNow, address), new Models.PingResponseModel());
            }
            // load it into storage as needed.
            foreach (var item in items)
            {
                _unit.Store(item.Value);
            }
        }

        [Test]
        public void OnStoreItShouldContaiaTheAddedItems()
        {
            Assert.That(_unit.Count, Is.EqualTo(0));            
            AddTestData();
            Assert.That(_unit.Count, Is.EqualTo(TestConditions.WorldAddresses.Length));
        }
    }
}