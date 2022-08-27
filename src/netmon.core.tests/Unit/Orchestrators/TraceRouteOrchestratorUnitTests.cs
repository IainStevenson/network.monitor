using Microsoft.Extensions.Logging;
using netmon.domain.Configuration;
using netmon.domain.Data;
using netmon.domain.Handlers;
using netmon.domain.Interfaces;
using netmon.domain.Models;
using netmon.domain.Orchestrators;
using NSubstitute;
using System.Net.NetworkInformation;

namespace netmon.domain.tests.Integration.Orchestrators
{
    public class TraceRouteOrchestratorUnitTests : TestBase<TraceRouteOrchestrator>
    {
        private PingHandlerOptions _pingHandlerOptions;
        private IPingHandler _pingHandler;
        private IPingRequestModelFactory _pingRequestModelFactory;
        private TraceRouteOrchestratorOptions _traceRouteHandlerOptions;
        private ILogger<PingHandler> _pingHandlerLogger;
        private ILogger<TraceRouteOrchestrator> _traceRouteOrchestratorLogger;

        /// <summary>
        /// TODO: Mock out all deppendencies
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

        /// <summary>
        /// using Mocked ping handler to emit exception types
        /// </summary>
        [Test]
        [Category("Unit")]
        public void OnExecuteToLoopbackOnPingExceptionReturnsResultsWithoutResponses()
        {
            var pingHandler = Substitute.For<IPingHandler>();

            pingHandler.Execute(Arg.Any<PingRequestModel>(), Arg.Any<CancellationToken>())
                        .Returns(Task.FromException<PingResponseModel>(new PingException("some fake error")));

            _unit = new TraceRouteOrchestrator(pingHandler, _traceRouteHandlerOptions, _pingRequestModelFactory, _traceRouteOrchestratorLogger);

            var responses = _unit.Execute(Defaults.LoopbackAddress, _cancellationToken).Result;

            ShowResults(responses);

            pingHandler.Received(_traceRouteHandlerOptions.MaxHops).Execute(Arg.Any<PingRequestModel>(), Arg.Any<CancellationToken>());
            Assert.That(actual: responses, Has.Count.EqualTo(1));
            Assert.That(actual: responses.Where(x => x.Value.Request != null & x.Value.Response == null).ToList(), Has.Count.EqualTo(1));

        }
    }
}