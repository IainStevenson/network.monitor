using Microsoft.Extensions.Logging;
using netmon.core.Configuration;
using netmon.core.Handlers;
using netmon.core.Interfaces;
using netmon.core.Messaging;
using netmon.core.Models;
using netmon.core.Orchestrators;
using NSubstitute;
using System.Net;

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
        private IStorageOrchestrator<PingResponseModel> _pingResponseModelStorageOrchestrator;
        private ILogger<PingHandler> _pingLogger;
        private ILogger<MonitorOrchestrator> _monitorOrchestratorLogger;
        private ILogger<TraceRouteOrchestrator> _traceRouteOrchestratorLogger;


        private IMonitorSubOrchestrator _monitorPingOnlySubOrchestrator;
        private IMonitorSubOrchestrator _monitorTraceRouteSubOrchestrator;
        private IMonitorSubOrchestrator _monitorTraceRouteThenPingSubOrchestrator;
        
        
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
            _pingResponseModelStorageOrchestrator = Substitute.For<IStorageOrchestrator<PingResponseModel>>();// for the moment mock out the storage.

            // need to test individually each sub orchestrator
            // and then mock here the sub orchestrators using this test as a template.
            //var subOrchestrators = new Dictionary<MonitorModes, IMonitorOrchestrator>();

            _monitorPingOnlySubOrchestrator = Substitute.For<IMonitorSubOrchestrator>();
            _monitorPingOnlySubOrchestrator = Substitute.For<IMonitorSubOrchestrator>();
            _monitorTraceRouteThenPingSubOrchestrator = Substitute.For<IMonitorSubOrchestrator>();

            _monitorOrchestratorLogger = Substitute.For<ILogger<MonitorOrchestrator>>();
            Dictionary<MonitorModes, IMonitorSubOrchestrator> subOrchestrators = new Dictionary<MonitorModes, IMonitorSubOrchestrator>();
            subOrchestrators.Add(MonitorModes.PingOnly, _monitorPingOnlySubOrchestrator);
            subOrchestrators.Add(MonitorModes.TraceRoute, _monitorTraceRouteSubOrchestrator);
            subOrchestrators.Add(MonitorModes.TraceRouteThenPing, _monitorTraceRouteThenPingSubOrchestrator);

            _unit = new MonitorOrchestrator(subOrchestrators, _monitorOrchestratorLogger);
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
                .Do(doit =>
                    storedResponses.TryAdd(new Tuple<DateTimeOffset, IPAddress>(
                        doit.Arg<PingResponseModel>().Start, doit.Arg<PingResponseModel>().Request.Address), doit.Arg<PingResponseModel>()));

            await _unit.Execute(MonitorModes.TraceRouteThenPing, new List<IPAddress>(), _testUntil, _cancellationToken);

            //ShowResults(multiPingMonitorResponses);
            ShowResults(storedResponses);


            //Assert.Multiple(() =>
            //{
            //    Assert.That(actual: multiPingMonitorResponses, Is.Not.Null);
            //    Assert.That(actual: multiPingMonitorResponses, Is.Not.Empty);
            //});

            _pingResponseModelStorageOrchestrator.Received((int)(storedResponses.Count())).Store(Arg.Any<PingResponseModel>()).Wait();
        }

        [Test]
        [Category("Integration")]
        public async Task OnExecuteNextTimeItJustMonitorsSpecifiedAddresses()
        {

            await _unit.Execute(MonitorModes.PingOnly, _monitorLoopbackAddresses, _testUntil, _cancellationToken);

            //ShowResults(responses);

            //Assert.That(actual: responses, Is.Not.Empty);
            _pingResponseModelStorageOrchestrator.Received(2).Store(Arg.Any<PingResponseModel>()).Wait();

        }




    }
}

