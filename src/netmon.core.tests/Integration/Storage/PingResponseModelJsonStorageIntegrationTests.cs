using netmon.core.Messaging;
using netmon.core.Storage;
using System.Net;

namespace netmon.core.tests.Integration.Storage
{
    public class PingResponseModelJsonStorageIntegrationTests : TestBase<PingResponseModelJsonRepository>
    {
        private DirectoryInfo _testFolder;
        private PingResponses _TestData = new();
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
            _unit = new PingResponseModelJsonRepository(_testFolder,_settings, _storageFolderDelimiter);
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
        [Category("Integration")]
        public async Task OnStoreItShouldContainTheAddedItemsAsync()
        {
            AddWorldAddressesTestData();

            foreach (var item in _TestData)
            {
                var response = await _unit.RetrieveAsync(item.Value.Id);
                Assert.IsNotNull(response);
                Assert.That(response.Id, Is.EqualTo(item.Value.Id));
            }

        }


    }
}