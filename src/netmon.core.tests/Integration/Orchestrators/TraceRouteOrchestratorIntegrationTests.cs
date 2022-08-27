using Microsoft.Extensions.Logging;
using netmon.domain.Configuration;
using netmon.domain.Data;
using netmon.domain.Handlers;
using netmon.domain.Interfaces;
using netmon.domain.Orchestrators;
using NSubstitute;
using System.Net.NetworkInformation;

namespace netmon.domain.tests.Integration.Orchestrators
{
    public class TraceRouteOrchestratorIntegrationTests : TestBase<TraceRouteOrchestrator>
    {
        private PingHandlerOptions _pingHandlerOptions;
        private IPingHandler _pingHandler;
        private IPingRequestModelFactory _pingRequestModelFactory;
        private TraceRouteOrchestratorOptions _traceRouteHandlerOptions;
        private ILogger<PingHandler> _pingHandlerLogger;
        private ILogger<TraceRouteOrchestrator> _traceRouteOrchestratorLogger;

        /// <summary>
        /// To DO: Remove all mocks
        /// </summary>
        [SetUp]
        public override void Setup()
        {
            base.Setup();
            // unit setup
            _pingHandlerOptions = new PingHandlerOptions();
            _pingHandlerLogger = Substitute.For<ILogger<PingHandler>>();
            _pingHandler = new PingHandler(_pingHandlerOptions, _pingHandlerLogger);
            _pingRequestModelFactory = new PingRequestModelFactory(_pingHandlerOptions);
            _traceRouteHandlerOptions = new TraceRouteOrchestratorOptions();
            _traceRouteOrchestratorLogger = Substitute.For<ILogger<TraceRouteOrchestrator>>();
            _unit = new TraceRouteOrchestrator(_pingHandler, _traceRouteHandlerOptions, _pingRequestModelFactory, _traceRouteOrchestratorLogger);


        }

        [Test]
        [Category("Integration")]
        public void OnExecuteToLoopbackAddressItReturnsResponses()
        {
            var responses = _unit.Execute(Defaults.LoopbackAddress, _cancellationToken).Result;

            Assert.That(actual: responses, Has.Count.EqualTo(4));
            Assert.Multiple(() =>
            {
                Assert.That(actual: responses.Where(x => x.Value.Request.Ttl == 1).ToList(), Has.Count.EqualTo(1));
                Assert.That(actual: responses.Where(x => x.Value.Request.Ttl == 128).ToList(), Has.Count.EqualTo(3));
                Assert.That(actual: responses.Where(x => x.Value.Hop == null).ToList(), Has.Count.EqualTo(1));
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
                    .Where(w => w.Value.Hop == null)
                    .Count(); ;
            var numberOfResponsesWhichWhereHopAnalysis = responses
                    .Where(w => w.Value.Hop != null)
                    .Count(); ;

            var expectedCount = numberOfResponsesWhichWhereHopDiscovery + numberOfResponsesWhichWhereHopAnalysis;

            Assert.That(actual: responses, Has.Count.EqualTo(expectedCount));
        }
    }
}