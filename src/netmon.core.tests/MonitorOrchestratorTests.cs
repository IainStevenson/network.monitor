using netmon.core.Configuration;
using netmon.core.Handlers;
using netmon.core.Interfaces;
using netmon.core.Models;
using netmon.core.Orchestrators;
using NSubstitute;
using System.Net;

namespace netmon.core.tests
{
    public class MonitorOrchestratorTests : TestBase<MonitorOrchestrator>
    {
        //private MonitorOptions _monitorOptions;
        private TraceRouteOrchestrator _traceRouteOrchestrator;
        private PingOrchestrator _pingOrchestrator;
        private IPingHandler _pingHandler;
        private TraceRouteOrchestratorOptions _traceRouteOrchestratorOptions;
        private IPingRequestModelFactory _pingRequestModelFactory;
        private PingHandlerOptions _pingHandlerOptions;
        private PingOrchestratorOptions _pingOrchestratorOptions;
        private IStorage<PingResponseModel> _pingResponseStorage;
        private readonly List<IPAddress> _monitorLoopbackAddresses = new() { IPAddress.Parse("127.0.0.1") };
        
        [SetUp]
        public override void Setup()
        {
            base.Setup();
            // unit setup - need to get more interfaces going and uses mocking.
            _pingHandlerOptions = new PingHandlerOptions();
            _traceRouteOrchestratorOptions = new TraceRouteOrchestratorOptions();
            _pingRequestModelFactory = new PingRequestModelFactory(_pingHandlerOptions);
            _pingHandler = new PingHandler(_pingHandlerOptions);
            _traceRouteOrchestrator = new TraceRouteOrchestrator(_pingHandler, _traceRouteOrchestratorOptions, _pingRequestModelFactory);
            _pingOrchestratorOptions = new PingOrchestratorOptions() { MillisecondsBetweenPings = 1000 };// faster for testing
            _pingOrchestrator = new PingOrchestrator(_pingHandler, _pingRequestModelFactory, _pingOrchestratorOptions);
            
            _pingResponseStorage = NSubstitute.Substitute.For<IStorage<PingResponseModel>>();
            _unit = new MonitorOrchestrator(_traceRouteOrchestrator, _pingOrchestrator, _pingResponseStorage);
        }



        [Test]
        [Category("Integration")]
        public async Task OnExecuteFirstTimeItTracesRouteToDefaultAddressAndMonitorsDiscoveredAddresses()
        {
            // two seconds is long enough , must keep ratio of until to _pingOrchestratorOptions.MillsecondsBetweenPings as  even seconds to get count
            var until = new TimeSpan(0, 0, 2);


            var multiPingMonitorResponses = await _unit.Execute(new List<IPAddress>(), until, _cancellationToken);

            ShowResults(multiPingMonitorResponses);

            Assert.Multiple(() =>
            {
                Assert.That(actual: multiPingMonitorResponses, Is.Not.Null);
                Assert.That(actual: multiPingMonitorResponses, Is.Not.Empty);
            });

            _pingResponseStorage.Received((int)(until.TotalSeconds * multiPingMonitorResponses.Count)).Store(Arg.Any<PingResponseModel>()).Wait();
        }

        [Test]
        [Category("Integration")]
        public async Task OnExecuteNextTimeItJustMonitorsSpecifiedAddresses()
        {
            //would use this for continuous use: var forEver = new TimeSpan(DateTimeOffset.MaxValue.Ticks - DateTimeOffset.UtcNow.Ticks);
            var until = new TimeSpan(0, 0, 2); // two seconds is long enough

            var responses = await _unit.Execute(_monitorLoopbackAddresses, until, _cancellationToken);

            ShowResults(responses);

            Assert.That(actual: responses, Is.Not.Empty);
            _pingResponseStorage.Received(2).Store(Arg.Any<PingResponseModel>()).Wait();

        }

    }
}