using Microsoft.Extensions.Logging;
using netmon.core.Interfaces.Repositories;
using netmon.core.Models;
using netmon.core.Orchestrators;
using NSubstitute;
using NSubstitute.ReceivedExtensions;

namespace netmon.core.tests.Unit.Orchestrators
{
    public class PingResponseModelReStorageOrchestratorTests : TestBase<PingResponseModelReStorageOrchestrator>
    {
        private ILogger<PingResponseModelReStorageOrchestrator> _logger;
        private DirectoryInfo _testFolder;
        private IStorageRepository<Guid, PingResponseModel> _storageRepository;
        private IFileSystemRepository _fileSystemRepository;

        [SetUp]
        public override void Setup()
        {
            base.Setup();
            _testFolder = new DirectoryInfo($".\\{Guid.NewGuid()}");
            if (!_testFolder.Exists)
            {
                _testFolder.Create();
                var items = Enumerable.Range(0, 10).Select(s => new PingResponseModel()).ToList();
                foreach (var item in items)
                {
                    File.WriteAllText($@"{_testFolder.FullName}\{item.Id}.json", Newtonsoft.Json.JsonConvert.SerializeObject(item, _settings));
                }
            }

            _storageRepository = Substitute.For<IStorageRepository<Guid, PingResponseModel>>();
            _fileSystemRepository = Substitute.For<IFileSystemRepository>(); //new PingResponseModelJsonRepository(_testFolder,_settings, _storageFolderDelimiter)

            _logger = Substitute.For<ILogger<PingResponseModelReStorageOrchestrator>>();
            _unit = new PingResponseModelReStorageOrchestrator(_storageRepository, _fileSystemRepository, _logger, _settings);
        }

        [Test]
        public void OnMoveFilesToObjectStorageItDoesNothingWhenNoFilesAreFound()
        {
            // Arange
            _fileSystemRepository.GetFileInformationAsync("*.json").Returns(new List<FileInfo>().AsEnumerable());

            // Act
            _unit.MoveFilesToObjectStorage(_cancellationToken).Wait();

            // Assert
            _fileSystemRepository.Received(1).GetFileInformationAsync("*.json");
            _fileSystemRepository.Received(0).GetFileDataAsync(Arg.Any<string>());
            _storageRepository.Received(0).StoreAsync(Arg.Any<PingResponseModel>());
            _fileSystemRepository.Received(0).DeleteFileAsync(Arg.Any<string>());

        }

        [Test]
        public void OnMoveFilesToObjectStorageItStoresAllFilesFoundAndDeserialised()
        {
            // Arange
            // note this is what teh mocked repository actually does here but we are isolating
            var filesFound = _testFolder.EnumerateFiles("*.json");
            _fileSystemRepository.GetFileInformationAsync("*.json").Returns(filesFound);
            _fileSystemRepository.GetFileDataAsync(Arg.Any<string>()).Returns(Newtonsoft.Json.JsonConvert.SerializeObject(new PingResponseModel(), _settings));

            // Act
            _unit.MoveFilesToObjectStorage(_cancellationToken).Wait();

            // Assert
            _fileSystemRepository.Received(1).GetFileInformationAsync("*.json");

            _fileSystemRepository.Received(filesFound.Count()).GetFileDataAsync(Arg.Any<string>());
            _storageRepository.Received(filesFound.Count()).StoreAsync(Arg.Any<PingResponseModel>());
            _fileSystemRepository.Received(filesFound.Count()).DeleteFileAsync(Arg.Any<string>());

        }

        [Test]
        public void OnMoveFilesToObjectStorageItFailsToStoreFilesThatAreNotDeserialised()
        {
            // Arange
            // note this is what teh mocked repository actually does here but we are isolating
            var filesFound = _testFolder.EnumerateFiles("*.json");
            _fileSystemRepository.GetFileInformationAsync("*.json").Returns(filesFound);
            _fileSystemRepository.GetFileDataAsync(Arg.Any<string>()).Returns(string.Empty);

            // Act
            _unit.MoveFilesToObjectStorage(_cancellationToken).Wait();

            // Assert
            _fileSystemRepository.Received(1).GetFileInformationAsync("*.json");

            _fileSystemRepository.Received(filesFound.Count()).GetFileDataAsync(Arg.Any<string>());
            _storageRepository.Received(0).StoreAsync(Arg.Any<PingResponseModel>());
            _fileSystemRepository.Received(0).DeleteFileAsync(Arg.Any<string>());

        }

        [Test]
        public void OnMoveFilesToObjectStorageItFailsToStoreFilesThatAreNotDeserialisedAsTheRightType()
        {
            // Arange
            // note this is what teh mocked repository actually does here but we are isolating
            var filesFound = _testFolder.EnumerateFiles("*.json");
            _fileSystemRepository.GetFileInformationAsync("*.json").Returns(filesFound);
            _fileSystemRepository.GetFileDataAsync(Arg.Any<string>()).Returns(" This is not a class");

            // Act
            _unit.MoveFilesToObjectStorage(_cancellationToken).Wait();

            // Assert
            _fileSystemRepository.Received(1).GetFileInformationAsync("*.json");

            _fileSystemRepository.Received(filesFound.Count()).GetFileDataAsync(Arg.Any<string>());

            _storageRepository.Received(0).StoreAsync(Arg.Any<PingResponseModel>());
            _fileSystemRepository.Received(0).DeleteFileAsync(Arg.Any<string>());

        }
    }
}