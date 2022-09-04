using Microsoft.Extensions.Logging;
using netmon.domain.Configuration;
using netmon.domain.Data;
using netmon.domain.Handlers;
using netmon.domain.Interfaces;
using netmon.domain.Interfaces.Repositories;
using netmon.domain.Models;
using netmon.domain.Orchestrators;
using netmon.domain.Storage;
using NSubstitute;
using System.Collections.Concurrent;
using System.Net;

namespace netmon.domain.tests.Integration.Orchestrators
{
    public class PingOrchestratorIntegrationTests : TestBase<PingOrchestrator>
    {
        private IPingHandler _pingHandler;
        private PingHandlerOptions _pingHandlerOptions;
        private ILogger<PingHandler> _pingHandlerLogger;
        private IPingRequestModelFactory _pingRequestModelFactory;
        private PingOrchestratorOptions _pingOrchestratorOptions;
        private PingResponseModelInMemoryRepository _storageRepository;
        private IEnumerable<IRepository> _repositories;
        private ConcurrentDictionary<Guid, PingResponseModel> _storage = new();
        

        [SetUp]
        public override void Setup()
        {
            base.Setup();
            // unit setup
            
            _storage.Clear();
            _storageRepository = new PingResponseModelInMemoryRepository(_storage, Substitute.For<ILogger<PingResponseModelInMemoryRepository>> ());

            _repositories = new List<IRepository>() { _storageRepository };

            _pingRequestModelFactory = new PingRequestModelFactory(_pingHandlerOptions);
            _pingHandlerOptions = new PingHandlerOptions();
            _pingHandlerLogger = Substitute.For<ILogger<PingHandler>>();
            _pingHandler = new PingHandler(_pingHandlerOptions, _pingHandlerLogger);
            _pingOrchestratorOptions = new PingOrchestratorOptions() { MillisecondsBetweenPings = 1000 };
            _unit = new PingOrchestrator(_pingHandler, _pingRequestModelFactory, _pingOrchestratorOptions, _repositories);
        }


        [Test]
        [Category("Integration")]
        public async Task OnPingWithDefaltLoopbackRequestItReturnsResponses()
        {
            // Arrange
            var request = new IPAddress[] { Defaults.LoopbackAddress };
            // Act
            var responses = await _unit.Ping(request, _cancellationToken);
            // Assert
            Assert.That(responses, Has.Count.EqualTo(request.Length), "The test returned the wrong number of results");
            Assert.That(responses.Where(x => x.Value.Request.Address is null).Count, Is.EqualTo(0), "One or more null address were returned");
            Assert.That(_storage.Count, Is.EqualTo(responses.Count), "The responses were not stored");
            ShowResults(responses);
        }


        [Test]
        [Category("Integration")]
        public async Task OnPingUntilWithDefaltLoopbackRequestFor2SecondsItReturnsResponses()
        {
            // Arrange
            var duration = new TimeSpan(0, 0, 2);
            var request = new IPAddress[] { Defaults.LoopbackAddress };
            // Act
            var responses = await _unit.PingUntil(request, duration, _cancellationToken);
            // Assert
            Assert.That(responses.Where(x => x.Value.Request.Address is null).Count, Is.EqualTo(0), "One or more null address were returned");
            Assert.That(_storage.Count, Is.EqualTo(responses.Count), "The responses were not stored");
            
            ShowResults(responses);
        }

        [Test]
        [Category("Integration")]
        public void OnPingUntilWithDefaltLoopbackRequestFor3SecondsItReturnsResponses()
        {
            var duration = new TimeSpan(0, 0, 3);
            var request = new IPAddress[] { Defaults.LoopbackAddress };
            var responses = _unit.PingUntil(request, duration, _cancellationToken).Result;
            Assert.That(responses.Where(x => x.Value.Request.Address is null).Count, Is.EqualTo(0), "One or more null address were returned");
            Assert.That(_storage.Count, Is.EqualTo(responses.Count), "The responses were not stored");
            ShowResults(responses);
        }

        [Test]
        [Category("Integration")]
        public void OnPingUntilWithComplexRequestkRequestFor4SecondsItReturnsResponses()
        {
            var duration = new TimeSpan(0, 0, 1);
            var request = new List<IPAddress>();
            request.AddRange(TestConditions.LocalAddresses);
            request.AddRange(TestConditions.WorldAddresses);

            var responses = _unit.PingUntil(request.ToArray(), duration, _cancellationToken).Result;

            Assert.That(responses, Has.Count.EqualTo(request.Count * duration.Seconds), "The test returned the wrong number of results");
            Assert.That(_storage.Count, Is.EqualTo(responses.Count), "The responses were not stored");
            ShowResults(responses);
        }
    }
}