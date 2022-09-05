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
        private IPingOrchestrator _pingOrchestrator;
        private IPingRequestModelFactory _pingRequestModelFactory;
        private TraceRouteOrchestratorOptions _traceRouteHandlerOptions;
        private ILogger<TraceRouteOrchestrator> _unitLogger;
        private PingHandlerOptions _pingHandlerOptions;
        //private ILogger<PingHandler> _pingHandlerLogger;

        /// <summary>
        /// TODO: Mock out all deppendencies
        /// </summary>
        [SetUp]
        public override void Setup()
        {
            base.Setup();
            // unit setup
            _pingOrchestrator = Substitute.For<IPingOrchestrator>(); ;
            _traceRouteHandlerOptions = new TraceRouteOrchestratorOptions();
            _pingHandlerOptions = new PingHandlerOptions();
            _pingRequestModelFactory = new PingRequestModelFactory(_pingHandlerOptions);
            _unitLogger = Substitute.For<ILogger<TraceRouteOrchestrator>>();
            _unit = new TraceRouteOrchestrator(_pingOrchestrator, _traceRouteHandlerOptions, _pingRequestModelFactory, _unitLogger);

        }

        /// <summary>
        /// using Mocked ping handler to emit exception types
        /// </summary>
        [Test]
        [Category("Unit")]
        public void OnExecuteToLoopbackOnPingExceptionReturnsResultsWithoutResponses()
        {
            var pingHandler = Substitute.For<IPinOrchestrator>();

            _pingOrchestrator.PingOne(Arg.Any<PingRequestModel>(), Arg.Any<CancellationToken>())
                        .Returns(Task.FromException<PingResponseModel>(new PingException("some fake error")));
            
            var responses = _unit.Execute(Defaults.LoopbackAddress, _cancellationToken).Result;

            ShowResults(responses);

            _pingOrchestrator.Received(_traceRouteHandlerOptions.MaxHops).PingOne(Arg.Any<PingRequestModel>(), Arg.Any<CancellationToken>());
            Assert.That(actual: responses, Has.Count.EqualTo(1));
            Assert.That(actual: responses.Where(x => x.Value.Request != null & x.Value.Response == null).ToList(), Has.Count.EqualTo(1));

        }
    }
}