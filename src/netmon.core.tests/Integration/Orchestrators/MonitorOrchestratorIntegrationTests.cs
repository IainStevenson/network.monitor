using Microsoft.Extensions.Logging;
using netmon.core.Configuration;
using netmon.core.Data;
using netmon.core.Handlers;
using netmon.core.Interfaces;
using netmon.core.Messaging;
using netmon.core.Models;
using netmon.core.Orchestrators;
using NSubstitute;
using System.Net;
using System.Net.NetworkInformation;

namespace netmon.core.tests.Integration.Orchestrators
{
    public class MonitorOrchestratorIntegrationTests : TestBase<MonitorOrchestrator>
    {
        private ITraceRouteOrchestrator _traceRouteOrchestrator;
        private IPingOrchestrator _pingOrchestrator;
        private IPingHandler _pingHandler;
        private TraceRouteOrchestratorOptions _traceRouteOrchestratorOptions;
        private IPingRequestModelFactory _pingRequestModelFactory;
        private PingHandlerOptions _pingHandlerOptions;
        private PingOrchestratorOptions _pingOrchestratorOptions;
        private IPingResponseModelStorageOrchestrator _pingResponseModelStorageOrchestrator;
        private ILogger<PingHandler> _pingLogger;
        private ILogger<MonitorOrchestrator> _monitorOrchestratorLogger;
        private ILogger<TraceRouteOrchestrator> _traceRouteOrchestratorLogger;
        private readonly List<IPAddress> _monitorLoopbackAddresses = new() { IPAddress.Parse("127.0.0.1") };
        private readonly TimeSpan _testUntil = new(0, 0, 2); //would use this for continuous use: var forEver = new TimeSpan(DateTimeOffset.MaxValue.Ticks - DateTimeOffset.UtcNow.Ticks);



        /// <summary>
        /// To DO: Remove all mocks
        /// </summary>
        [SetUp]
        public override void Setup()
        {
            base.Setup();
            // unit setup - need to get more interfaces going and uses mocking.
            _pingHandlerOptions = new PingHandlerOptions();
            _traceRouteOrchestratorOptions = new TraceRouteOrchestratorOptions();
            _pingRequestModelFactory = new PingRequestModelFactory(_pingHandlerOptions);
            _pingLogger = Substitute.For<ILogger<PingHandler>>();
            _pingHandler = new PingHandler(_pingHandlerOptions, _pingLogger);
            _traceRouteOrchestratorLogger = Substitute.For<ILogger<TraceRouteOrchestrator>>();
            _traceRouteOrchestrator = new TraceRouteOrchestrator(_pingHandler,
                _traceRouteOrchestratorOptions,
                _pingRequestModelFactory,
                _traceRouteOrchestratorLogger);
            _pingOrchestratorOptions = new PingOrchestratorOptions() { MillisecondsBetweenPings = 1000 };// faster for testing
            _pingOrchestrator = new PingOrchestrator(_pingHandler, _pingRequestModelFactory, _pingOrchestratorOptions);
            _pingResponseModelStorageOrchestrator = Substitute.For<IPingResponseModelStorageOrchestrator>();// for the moment mock out the storage.


            _monitorOrchestratorLogger = Substitute.For<ILogger<MonitorOrchestrator>>();
            _unit = new MonitorOrchestrator(
                _traceRouteOrchestrator,
                _pingOrchestrator,
                _pingResponseModelStorageOrchestrator,
                _monitorOrchestratorLogger);
        }


        [Test]
        [Category("Integration")]
        public async Task OnExecuteFirstTimeItTracesRouteToDefaultAddressAndMonitorsDiscoveredAddresses()
        {
            // two seconds is long enough , must keep ratio of until to _pingOrchestratorOptions.MillsecondsBetweenPings as  even seconds to get count
            var until = new TimeSpan(0, 0, 2);


            // trap the mock storage resutls here and display them in the test output.
            PingResponses storedResponses = new();
            _pingResponseModelStorageOrchestrator.When(it => it.Store(Arg.Any<PingResponseModel>()))
                .Do(doit => storedResponses.TryAdd(new Tuple<DateTimeOffset, IPAddress>(doit.Arg<PingResponseModel>().Start, doit.Arg<PingResponseModel>().Request.Address), doit.Arg<PingResponseModel>()));

            var multiPingMonitorResponses = await _unit.Execute(new List<IPAddress>(), _testUntil, false, _cancellationToken);

            ShowResults(multiPingMonitorResponses);
            ShowResults(storedResponses);


            Assert.Multiple(() =>
            {
                Assert.That(actual: multiPingMonitorResponses, Is.Not.Null);
                Assert.That(actual: multiPingMonitorResponses, Is.Not.Empty);
            });

            _pingResponseModelStorageOrchestrator.Received((int)(until.TotalSeconds * multiPingMonitorResponses.Count)).Store(Arg.Any<PingResponseModel>()).Wait();
        }

        [Test]
        [Category("Integration")]
        public async Task OnExecuteNextTimeItJustMonitorsSpecifiedAddresses()
        {

            var responses = await _unit.Execute(_monitorLoopbackAddresses, _testUntil, false, _cancellationToken);

            ShowResults(responses);

            Assert.That(actual: responses, Is.Not.Empty);
            _pingResponseModelStorageOrchestrator.Received(2).Store(Arg.Any<PingResponseModel>()).Wait();

        }

       
      

    }
}

