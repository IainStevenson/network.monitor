using netmon.core.Messaging;
using netmon.core.Storage;
using System.Net;

namespace netmon.core.tests.Integration.Storage
{
    public class PingResponseModelTextSummaryStorageIntegrationTests : TestBase<PingResponseModelTextSummaryRepository>
    {
        private DirectoryInfo _testFolder;
        private PingResponses _TestData = new();
        private const string _storageFolderDelimiter = "\\"; [SetUp]
        public override void Setup()
        {
            base.Setup();
            _testFolder = new DirectoryInfo($".\\{Guid.NewGuid()}");
            if (!_testFolder.Exists)
            {
                _testFolder.Create();
            }
            _unit = new PingResponseModelTextSummaryRepository(_testFolder, _storageFolderDelimiter);
        }

        private void AddWorldAddressesTestData()
        {
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
            if (states.Any(a => !a)) return; // fail the test
            // load it into storage as needed.
            foreach (var item in _TestData)
            {
                _unit.StoreAsync(item.Value).Wait();
            }
        }

        [Test]
        [Category("Integration")]
        public void OnStoreItShouldContainTheAddedItemsAsync()
        {
            Assert.DoesNotThrow(() => AddWorldAddressesTestData());


        }

    }
}