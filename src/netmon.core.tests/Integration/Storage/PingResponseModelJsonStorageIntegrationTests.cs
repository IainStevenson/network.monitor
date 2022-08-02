using netmon.core.Messaging;
using netmon.core.Storage;
using System.Net;

namespace netmon.core.tests.Integration.Storage
{
    public class PingResponseModelJsonStorageIntegrationTests : TestBase<PingResponseModelJsonStorage>
    {
        private DirectoryInfo _testFolder;
        private const string _storageFolderDelimiter = "\\";
        [SetUp]
        public override void Setup()
        {
            base.Setup();
            _testFolder = new DirectoryInfo($".\\{Guid.NewGuid()}");
            if (!_testFolder.Exists)
            {
                _testFolder.Create();
            }
            _unit = new PingResponseModelJsonStorage(_testFolder, _storageFolderDelimiter);
        }

        private void AddWorldAddressesTestData()
        {
            var items = new PingResponses();
            // simualte traceroute response.
            List<bool> states = new();
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
                _unit.Store(item.Value).Wait();
            }
        }

        [Test]
        [Category("Integration")]
        public void OnStoreItShouldContainTheAddedItems()
        {
            Assert.That(_unit.Count, Is.EqualTo(0));
            AddWorldAddressesTestData();
            Assert.That(_unit.Count, Is.EqualTo(TestConditions.WorldAddresses.Length));
        }


    }
}