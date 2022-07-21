using netmon.core.Configuration;
using netmon.core.Data;
using netmon.core.Handlers;
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
        public void Setup()
        {
            // unit setup
            _pingHandlerOptions = new PingHandlerOptions();
            _pingHandler = new PingHandler(_pingHandlerOptions);
            _pingRequestModelFactory = new PingRequestModelFactory();
            _traceRouteHandlerOptions = new TraceRouteOrchestratorOptions();

            _unit = new TraceRouteOrchestrator(_pingHandler, _traceRouteHandlerOptions, _pingRequestModelFactory);

            
        }

        [Test]
        public void OnExecuteToLoopbackAddressItReturnsResponses()
        {
            var responses = _unit.Execute(Defaults.LoopbackAddress, _cancellationToken).Result;

            Assert.That(actual: responses, Is.Not.Empty);

            ShowResults(responses);
        }


        [Test]
        public void OnExecuteToWorldAddressItReturnsResponses()
        {
            var responses = _unit.Execute(TestConditions.WorldAddresses.Last(), _cancellationToken).Result;

            Assert.That(actual: responses, Is.Not.Empty);

            ShowResults(responses);          
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

            pingHandler.Options.Returns(_pingHandlerOptions);

            _unit = new TraceRouteOrchestrator(pingHandler, _traceRouteHandlerOptions, _pingRequestModelFactory);

            var responses = _unit.Execute(Defaults.LoopbackAddress, _cancellationToken).Result;
            
            Assert.That(actual: responses, Is.Empty);

            pingHandler.Received(_traceRouteHandlerOptions.MaxHops * _traceRouteHandlerOptions.MaxAttempts)
                    .Execute(Arg.Any<PingRequestModel>(), Arg.Any<CancellationToken>());
        }

    }
}