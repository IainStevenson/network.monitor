using Microsoft.Extensions.Logging;
using netmon.domain.Interfaces.Repositories;
using netmon.domain.Messaging;
using netmon.domain.Orchestrators;
using netmon.domain.Storage;
using NSubstitute;
using NUnit.Framework.Interfaces;
using System.Net;
using System.Linq;

namespace netmon.domain.tests.Integration.Orchestrators
{
    public class PingResponseModelStorageOrchestratorIntegrationTests : TestBase<PingResponseModelStorageOrchestrator>
    {
        private List<IRepository> _respositories;
        private ILogger<PingResponseModelStorageOrchestrator> _logger;
        private DirectoryInfo _testFolder;
        private PingResponses _testData;
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

            _testData = new PingResponses();
            foreach (var address in TestConditions.WorldAddresses)
            {
                _testData.TryAdd(
                      new Tuple<DateTimeOffset, IPAddress>(DateTimeOffset.UtcNow, address),
                      new Models.PingResponseModel() { Request = new Models.PingRequestModel() { Address = address } })
                    ;
            }

            _respositories = new List<IRepository>()
            {
                { new PingResponseModelJsonRepository(_testFolder,_settings, _storageFolderDelimiter) },
                { new PingResponseModelTextSummaryRepository(_testFolder, _storageFolderDelimiter) }
             };
            _logger = Substitute.For<ILogger<PingResponseModelStorageOrchestrator>>();
            _unit = new PingResponseModelStorageOrchestrator(_respositories, _logger, _settings);


        }

        [Test]
        [Category("Integration")]
        public async Task OnStoreItShouldContainTheAddedItems()
        {
            foreach (var (respository, pattern) in from IFileSystemRepository respository in _respositories.Where(w => w.GetType().IsAssignableTo(typeof(IFileSystemRepository)))
                                                   let pattern = respository.GetType() == typeof(PingResponseModelJsonRepository) ? "*.json" : "*-summary.txt"
                                                   select (respository, pattern))
            {
                Assert.That(respository.GetFileInformationAsync(pattern).Result.Count, Is.EqualTo(0));
            }

            foreach (var item in _testData.Values)
            {
                await _unit.StoreAsync(item);
            }

            foreach (var (respository, pattern) in from IFileSystemRepository respository in _respositories.Where(w => w.GetType().IsAssignableTo(typeof(IFileSystemRepository)))
                                                   let pattern = respository.GetType() == typeof(PingResponseModelJsonRepository) ? "*.json" : "*-summary.txt"
                                                   select (respository, pattern))
            {
                Assert.That(respository.GetFileInformationAsync(pattern).Result.Count, Is.EqualTo(TestConditions.WorldAddresses.Length));
            }
        }
    }
}