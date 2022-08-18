using Microsoft.Extensions.Logging;
using netmon.core.Interfaces;
using netmon.core.Messaging;
using netmon.core.Models;
using netmon.core.Orchestrators;
using netmon.core.Storage;
using NSubstitute;
using System.Net;

namespace netmon.core.tests.Integration.Orchestrators
{
    public class PingResponseModelStorageOrchestratorIntegrationTests : TestBase<PingResponseModelStorageOrchestrator>
    {
        private List<IRepository> _respositories;
        private ILogger<PingResponseModelStorageOrchestrator> _logger;
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
            _respositories = new List<IRepository>()
            {
                { new PingResponseModelJsonRepository(_testFolder,_settings, _storageFolderDelimiter) },
                { new PingResponseModelTextSummaryRepository(_testFolder, _storageFolderDelimiter) }
             };
            _logger = Substitute.For<ILogger<PingResponseModelStorageOrchestrator>>();
            _unit = new PingResponseModelStorageOrchestrator(_respositories, _logger, _settings);
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
            foreach (IFileSystemQuery respository in _respositories)
            {
                var pattern = respository.GetType() == typeof(PingResponseModelJsonRepository) ? "*.json" : "*-summary.txt";
                Assert.That(respository.GetFileInformationAsync(pattern).Count, Is.EqualTo(0));
            }
            AddWorldAddressesTestData();

            foreach (IFileSystemQuery respository in _respositories)
            {
                var pattern = respository.GetType() == typeof(PingResponseModelJsonRepository) ? "*.json" : "*-summary.txt";


                Assert.That(respository.GetFileInformationAsync(pattern).Count, Is.EqualTo(TestConditions.WorldAddresses.Length));
            }


        }
    }
}