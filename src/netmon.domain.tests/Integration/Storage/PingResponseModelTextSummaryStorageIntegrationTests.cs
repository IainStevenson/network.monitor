using netmon.domain.Messaging;
using netmon.domain.Storage;
using System.Net;

namespace netmon.domain.tests.Integration.Storage
{
    public class PingResponseModelTextSummaryStorageIntegrationTests : TestBase<PingResponseModelTextSummaryRepository>
    {
        private DirectoryInfo _testFolder;
        private PingResponseModels _TestData = new();
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
            _TestData = new PingResponseModels();
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
        public void ItNotShouldDeclareItCanRetrieve()
        {
            // Arrange

            // Act
            var actual = _unit.Capabilities.HasFlag(Interfaces.Repositories.RepositoryCapabilities.Retrieve);
            // Assert
            Assert.That(actual, Is.False);

        }
        [Test]
        public void ItNotShouldDeclareItCanDelete()
        {
            // Arrange

            // Act
            var actual = _unit.Capabilities.HasFlag(Interfaces.Repositories.RepositoryCapabilities.Delete);
            // Assert
            Assert.That(actual, Is.False);

        }

    }
}