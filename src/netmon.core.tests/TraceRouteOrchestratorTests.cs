using netmon.core.Configuration;
using netmon.core.Data;
using netmon.core.Handlers;
using netmon.core.Interfaces;
using netmon.core.Models;
using netmon.core.Orchestrators;
using NSubstitute;
using System.Net.NetworkInformation;

namespace netmon.core.tests
{
    public class TraceRouteOrchestratorTests : TestBase<TraceRouteOrchestrator>
    {
        private PingHandlerOptions _pingHandlerOptions;
        private IPingHandler _pingHandler;       
        private IPingRequestModelFactory _pingRequestModelFactory;
        private TraceRouteOrchestratorOptions _traceRouteHandlerOptions;

        [SetUp]
        public override void Setup()
        {
            base.Setup();
            // unit setup
            _pingHandlerOptions = new PingHandlerOptions();
            _pingHandler = new PingHandler(_pingHandlerOptions);
            _pingRequestModelFactory = new PingRequestModelFactory(_pingHandlerOptions);
            _traceRouteHandlerOptions = new TraceRouteOrchestratorOptions();

            _unit = new TraceRouteOrchestrator(_pingHandler, _traceRouteHandlerOptions, _pingRequestModelFactory);

            
        }

        [Test]
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
        public void OnExecuteToLoopbackAddressWhenCancelledReturnsFewerResponses()
        {
            _cancellationTokenSource.Cancel();
            
            var responses = _unit.Execute(Defaults.LoopbackAddress, _cancellationToken).Result;
            
            Assert.That(actual: responses, Has.Count.LessThan(4));

            ShowResults(responses);
        }


        [Test]
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




        /// <summary>
        /// using Mocked ping handler to emit exception types
        /// note this will make 30 * 3 attempts to reach the loopback but generate 30 exceptions simulating a network not working at all.
        /// </summary>
        [Test]
        public void OnExecuteToLoopbackOnPingExceptionReturnsEmptyList()
        {
            var pingHandler = Substitute.For<IPingHandler>();

            pingHandler.Execute(Arg.Any<PingRequestModel>(), Arg.Any<CancellationToken>())
                        .Returns(Task.FromException<PingResponseModel>(new PingException("some fake error")));

            //pingHandler.Options.Returns(_pingHandlerOptions);

            _unit = new TraceRouteOrchestrator(pingHandler, _traceRouteHandlerOptions, _pingRequestModelFactory);

            var responses = _unit.Execute(Defaults.LoopbackAddress, _cancellationToken).Result;
            
            Assert.That(actual: responses, Is.Empty);

            //pingHandler.Received(_traceRouteHandlerOptions.MaxHops * _traceRouteHandlerOptions.MaxAttempts)
            //        .Execute(Arg.Any<PingRequestModel>(), Arg.Any<CancellationToken>());
        }

    }
}