using Microsoft.Extensions.Logging;
using netmon.domain.Configuration;
using netmon.domain.Data;
using netmon.domain.Handlers;
using netmon.domain.Interfaces;
using netmon.domain.Interfaces.Repositories;
using netmon.domain.Models;
using netmon.domain.Orchestrators;
using netmon.domain.Storage;
using Newtonsoft.Json;
using NSubstitute;
using NUnit.Framework.Constraints;
using System.Collections.Concurrent;
using System.Net.NetworkInformation;

namespace netmon.domain.tests.Integration.Orchestrators
{
    public class TraceRouteOrchestratorIntegrationTests : TestBase<TraceRouteOrchestrator>
    {
        private PingHandlerOptions _pingHandlerOptions;
        private IPinOrchestrator _pingHandler;
        private IPingRequestModelFactory _pingRequestModelFactory;
        private TraceRouteOrchestratorOptions _traceRouteHandlerOptions;
        private ILogger<PingHandler> _pingHandlerLogger;
        private ILogger<TraceRouteOrchestrator> _logger;
        private IPingOrchestrator _pingOrchestrator;
        private PingOrchestratorOptions _pingOrchestratorOptions;
        private IEnumerable<IRepository> _repositories;
        private ConcurrentDictionary<Guid, PingResponseModel> _store= new();
        private ILogger<PingResponseModelInMemoryRepository> _storeLogger;

        [SetUp]
        public override void Setup()
        {
            base.Setup();

            // unit setup
            _store .Clear();
            _pingHandlerOptions = new PingHandlerOptions() { Timeout = 1000};
            _pingOrchestratorOptions = new PingOrchestratorOptions() { MillisecondsBetweenPings = 1000};
            _pingHandlerLogger = Substitute.For<ILogger<PingHandler>>();
            _pingHandler = new PingHandler(_pingHandlerOptions, _pingHandlerLogger);

            _pingRequestModelFactory = new PingRequestModelFactory(_pingHandlerOptions);
            _traceRouteHandlerOptions = new TraceRouteOrchestratorOptions() {};
            _logger = Substitute.For<ILogger<TraceRouteOrchestrator>>();
            _storeLogger = Substitute.For<ILogger<PingResponseModelInMemoryRepository>>();


            _repositories = new List<IRepository>() {
                new PingResponseModelInMemoryRepository(_store, _storeLogger)
            };

            _pingOrchestrator = new PingOrchestrator(_pingHandler, _pingRequestModelFactory, _pingOrchestratorOptions, _repositories);

            _unit = new TraceRouteOrchestrator(
                    _pingOrchestrator, 
                    _traceRouteHandlerOptions, 
                    _pingRequestModelFactory, 
                    _logger);


        }

        [Test]
        [Category("Integration")]
        public async Task OnExecuteToLoopbackAddressItReturnsResponses()
        {
            var responses = await _unit.Execute(Defaults.LoopbackAddress, _cancellationToken);

            Assert.That(actual: responses, Has.Count.EqualTo(4));
            Assert.Multiple(() =>
            {
                Assert.That(actual: responses.Where(x => x.Value.Request.Ttl == 1).ToList(), Has.Count.EqualTo(1));
                Assert.That(actual: responses.Where(x => x.Value.Request.Ttl == 128).ToList(), Has.Count.EqualTo(3));
                Assert.That(actual: responses.Where(x => x.Value.TraceInfo == null).ToList(), Has.Count.EqualTo(1));
                Assert.That(actual: responses.Where(x => x.Value.Request.Ttl == 128).ToList(), Has.Count.EqualTo(3));
                Assert.That(actual: responses.Where(x => x.Value.Response?.Status == IPStatus.Success).ToList(), Has.Count.EqualTo(4));
              
            });
            ShowResults(responses);
        }

        [Test]
        [Category("Integration")]
        public void OnExecuteToLoopbackAddressWhenCancelledReturnsFewerResponses()
        {
            _cancellationTokenSource.Cancel();

            var responses = _unit.Execute(Defaults.LoopbackAddress, _cancellationToken).Result;

            Assert.That(actual: responses, Has.Count.LessThan(4));
       

            ShowResults(responses);
        }


        [Test]
        [Category("Integration")]
        public void OnExecuteToWorldAddressItReturnsResponses()
        {
            var responses = _unit.Execute(TestConditions.WorldAddresses.Last(), _cancellationToken).Result;

            ShowResults(responses);

            Assert.That(actual: responses, Is.Not.Empty);

            var numberOfResponsesWhichWhereHopDiscovery = responses
                    .Where(w => w.Value.TraceInfo == null)
                    .Count(); ;
            var numberOfResponsesWhichWhereHopAnalysis = responses
                    .Where(w => w.Value.TraceInfo != null)
                    .Count(); ;

            var expectedCount = numberOfResponsesWhichWhereHopDiscovery + numberOfResponsesWhichWhereHopAnalysis;

            Assert.That(actual: responses, Has.Count.EqualTo(expectedCount));
            
        }
    }
}