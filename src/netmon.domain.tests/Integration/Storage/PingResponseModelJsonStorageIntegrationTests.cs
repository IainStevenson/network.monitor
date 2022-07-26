﻿using Microsoft.Extensions.Logging;
using netmon.domain.Messaging;
using netmon.domain.Storage;
using System.Net;

namespace netmon.domain.tests.Integration.Storage
{
    public class PingResponseModelJsonStorageIntegrationTests : TestBase<PingResponseModelJsonRepository>
    {
        private DirectoryInfo _testFolder;
        private PingResponseModels _testData = new();
        private ILogger<PingResponseModelJsonRepository> _logger;
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
            _logger = NSubstitute.Substitute.For<ILogger<PingResponseModelJsonRepository>>();

            _testData = new PingResponseModels();
            foreach (var address in TestConditions.WorldAddresses)
            {
                _testData.TryAdd(
                      new Tuple<DateTimeOffset, IPAddress>(DateTimeOffset.UtcNow, address),
                      new Models.PingResponseModel() { Request = new Models.PingRequestModel() { Address = address } })
                    ;
            }
            _unit = new PingResponseModelJsonRepository(_testFolder, _settings, _storageFolderDelimiter, _logger);
        }

       

        [Test]
        public void ItShouldDeclareItCanFile()
        {
            // Arrange

            // Act
            var actual = _unit.Capabilities.HasFlag(Interfaces.Repositories.RepositoryCapabilities.File);
            // Assert
            Assert.That(actual, Is.True);

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
        public void ItShouldDeclareItCanRetrieve()
        {
            // Arrange

            // Act
            var actual = _unit.Capabilities.HasFlag(Interfaces.Repositories.RepositoryCapabilities.Retrieve);
            // Assert
            Assert.That(actual, Is.True);

        }
        [Test]
        public void ItShouldDeclareItCanDelete()
        {
            // Arrange

            // Act
            var actual = _unit.Capabilities.HasFlag(Interfaces.Repositories.RepositoryCapabilities.Delete);
            // Assert
            Assert.That(actual, Is.True);

        }

        [Test]
        [Category("Integration")]
        public async Task OnStoreAsyncItShouldContainTheAddedItems()
        {
            //Arrange


            // Act
            foreach (var item in _testData)
            {
                await _unit.StoreAsync(item.Value);
            }

            // Assert
            foreach (var item in _testData)
            {
                var response = await _unit.RetrieveAsync(item.Value.Id);
                Assert.That(response, Is.Not.Null);
                Assert.That(response.Id, Is.EqualTo(item.Value.Id));
            }
        }

        [Test]
        [Category("Integration")]
        public async Task OnDeleteAsyncItShouldNoLongerContainTheAddedItems()
        {
            //Arrange
            foreach (var item in _testData)
            {
                await _unit.StoreAsync(item.Value);
            }
            foreach (var item in _testData)
            {
                var response = await _unit.RetrieveAsync(item.Value.Id);
                Assert.That(response, Is.Not.Null);
                Assert.That(response.Id, Is.EqualTo(item.Value.Id));
            }


            // Act
            foreach (var item in _testData)
            {
                await _unit.DeleteAsync(item.Value.Id);
            }


            // Assert
            foreach (var item in _testData)
            {
                var response = await _unit.RetrieveAsync(item.Value.Id);
                Assert.That(response, Is.Null);
            }
        }


        [Test]
        [Category("Integration")]
        public async Task OnGetFileDataAsyncItShouldReturnData()
        {

            //Arrange
            foreach (var item in _testData)
            {
                await _unit.StoreAsync(item.Value);
            }
            foreach (var item in _testData)
            {
                var response = await _unit.RetrieveAsync(item.Value.Id);
                Assert.That(response, Is.Not.Null);
                Assert.That(response.Id, Is.EqualTo(item.Value.Id));
            }


            foreach (var item in _testData)
            {
                var filename = $@"{_testFolder.FullName}{_storageFolderDelimiter}{item.Value.Start.ToString("o").Replace(":", "-")}-{item.Value.Request.Address}-{item.Value.Id}.json"; ;
                // Act
                var response = await _unit.GetFileDataAsync(filename);
                // Assert
                Assert.That(response, Is.Not.Null);
            }
        }


        [Test]
        [Category("Integration")]
        public async Task OnGetFileDataAsyncItShouldReturnEmpty()
        {

            //Arrange
            foreach (var item in _testData)
            {
                await _unit.StoreAsync(item.Value);
            }
            foreach (var item in _testData)
            {
                var response = await _unit.RetrieveAsync(item.Value.Id);
                Assert.That(response, Is.Not.Null);
                Assert.That(response.Id, Is.EqualTo(item.Value.Id));
            }


            foreach (var item in _testData)
            {
                var filename = $@"{_testFolder.FullName}{_storageFolderDelimiter}invalidfilename-{item.Value.Request.Address}-{item.Value.Id}.json"; ;
                // Act
                var response = await _unit.GetFileDataAsync(filename);
                // Assert
                Assert.That(response, Is.Empty);
            }
        }


        [Test]
        [Category("Integration")]
        public async Task OnGetFileInformationAsyncItShouldReturnOne()
        {

            //Arrange
            foreach (var item in _testData)
            {
                await _unit.StoreAsync(item.Value);
            }
            foreach (var item in _testData)
            {
                var response = await _unit.RetrieveAsync(item.Value.Id);
                Assert.That(response, Is.Not.Null);
                Assert.That(response.Id, Is.EqualTo(item.Value.Id));
            }


            foreach (var item in _testData)
            {
                var filename = $@"{item.Value.Start.ToString("o").Replace(":", "-")}-{item.Value.Request.Address}-{item.Value.Id}.json"; ;
                // Act
                var response = (await _unit.GetFileInformationAsync(filename)).ToList();
                // Assert
                Assert.That(response, Is.Not.Empty);
                Assert.That(response, Has.Count.EqualTo(1));
            }
        }
        [Test]
        [Category("Integration")]
        public async Task OnGetFileInformationAsyncItShouldReturnNone() {
            //Arrange
            foreach (var item in _testData)
            {
                await _unit.StoreAsync(item.Value);
            }
            foreach (var item in _testData)
            {
                var response = await _unit.RetrieveAsync(item.Value.Id);
                Assert.That(response, Is.Not.Null);
                Assert.That(response.Id, Is.EqualTo(item.Value.Id));
            }


            foreach (var item in _testData)
            {
                var filename = $@"InvalidFilename-{item.Value.Request.Address}-{item.Value.Id}.json"; ;
                // Act
                var response = (await _unit.GetFileInformationAsync(filename)).ToList();
                // Assert
                Assert.That(response, Is.Empty);

            }
        }


        [Test]
        [Category("Integration")]
        public async Task OnDeleteFileAsyncItShouldDeleteTheFile() {

            //Arrange
            foreach (var item in _testData)
            {
                await _unit.StoreAsync(item.Value);
            }
            foreach (var item in _testData)
            {
                var response = await _unit.RetrieveAsync(item.Value.Id);
                Assert.That(response, Is.Not.Null);
                Assert.That(response.Id, Is.EqualTo(item.Value.Id));
            }


            foreach (var item in _testData)
            {
                var filename = $@"{_testFolder.FullName}{_storageFolderDelimiter}{item.Value.Start.ToString("o").Replace(":", "-")}-{item.Value.Request.Address}-{item.Value.Id}.json"; ;
                // Act
                var response = await _unit.DeleteFileAsync(filename);
                // Assert
                Assert.That(response, Is.Empty);
            }
        }

        [Test]
        [Category("Integration")]
        public async Task OnDeleteFileAsyncItShouldDoNothing() {
            //Arrange
            foreach (var item in _testData)
            {
                await _unit.StoreAsync(item.Value);
            }
            foreach (var item in _testData)
            {
                var response = await _unit.RetrieveAsync(item.Value.Id);
                Assert.That(response, Is.Not.Null);
                Assert.That(response.Id, Is.EqualTo(item.Value.Id));
            }


            foreach (var item in _testData)
            {
                var filename = $@"{_testFolder.FullName}{_storageFolderDelimiter}InvalidFilename-{item.Value.Request.Address}-{item.Value.Id}.json"; ;
                // Act
                var response = await _unit.DeleteFileAsync(filename);
                // Assert                
                Assert.That(response, Is.EqualTo(filename));
            }
        }
    }
}