
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

namespace netmon.core.tests
{
    public class MonitorOrchestratorTests : TestBase<MonitorOrchestrator>
    {
        //private MonitorOptions _monitorOptions;
        private ITraceRouteOrchestrator _traceRouteOrchestrator;
        private IPingOrchestrator _pingOrchestrator;
        private IPingHandler _pingHandler;
        private TraceRouteOrchestratorOptions _traceRouteOrchestratorOptions;
        private IPingRequestModelFactory _pingRequestModelFactory;
        private PingHandlerOptions _pingHandlerOptions;
        private PingOrchestratorOptions _pingOrchestratorOptions;
        private IStorage<PingResponseModel> _pingResponseStorage;
        private ILogger<PingHandler> _pingLogger;
        private ILogger<MonitorOrchestrator> _monitorOrchestratorLogger;
        private ILogger<TraceRouteOrchestrator> _traceRouteOrchestratorLogger;
        private readonly List<IPAddress> _monitorLoopbackAddresses = new() { IPAddress.Parse("127.0.0.1") };
        private readonly TimeSpan _testUntil = new(0, 0, 2); //would use this for continuous use: var forEver = new TimeSpan(DateTimeOffset.MaxValue.Ticks - DateTimeOffset.UtcNow.Ticks);

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
            _pingResponseStorage = NSubstitute.Substitute.For<IStorage<PingResponseModel>>();

            _monitorOrchestratorLogger = Substitute.For<ILogger<MonitorOrchestrator>>();
            _unit = new MonitorOrchestrator(_traceRouteOrchestrator, _pingOrchestrator, _pingResponseStorage, _monitorOrchestratorLogger);
        }


        private MonitorOrchestrator CreateIsolatedUnit(PingResponses testTraceRouteResponses, PingResponses testPingResponses)
        {
            _traceRouteOrchestrator = Substitute.For<ITraceRouteOrchestrator>();
            _pingOrchestrator = Substitute.For<IPingOrchestrator>();

            _traceRouteOrchestrator.Execute(Arg.Any<IPAddress>(), Arg.Any<CancellationToken>()).Returns(testTraceRouteResponses);
            _pingOrchestrator.PingUntil(Arg.Any<IPAddress[]>(), _testUntil, Arg.Any<CancellationToken>()).Returns(testPingResponses);

            _pingOrchestrator.Results += (sender, args) => _pingResponseStorage.Store(args.Model); // call through to the mocked storage

            // now make sure the event is raised...when the mocked method is called

            _pingOrchestrator.When(it => it.PingUntil(Arg.Any<IPAddress[]>(), _testUntil, Arg.Any<CancellationToken>()))
            .Do(doit =>
                    _pingOrchestrator.Results +=
                    Raise.Event<EventHandler<PingResponseModelEventArgs>>(
                        this,
                        new PingResponseModelEventArgs(new PingResponseModel() { }))
                ); // add address to event data?


            return new MonitorOrchestrator(_traceRouteOrchestrator, _pingOrchestrator, _pingResponseStorage, _monitorOrchestratorLogger);
        }


        [Test]
        [Category("Integration")]
        public async Task OnExecuteFirstTimeItTracesRouteToDefaultAddressAndMonitorsDiscoveredAddresses()
        {
            // two seconds is long enough , must keep ratio of until to _pingOrchestratorOptions.MillsecondsBetweenPings as  even seconds to get count
            var until = new TimeSpan(0, 0, 2);


            // trap the mock storage resutls here and display them in the test output.
            PingResponses storedResponses = new();
            _pingResponseStorage.When(it => it.Store(Arg.Any<PingResponseModel>()))
                .Do(doit => storedResponses.TryAdd(new Tuple<DateTimeOffset, IPAddress>(doit.Arg<PingResponseModel>().Start, doit.Arg<PingResponseModel>().Request.Address), doit.Arg<PingResponseModel>()));

            var multiPingMonitorResponses = await _unit.Execute(new List<IPAddress>(), _testUntil, false, _cancellationToken);

            ShowResults(multiPingMonitorResponses);
            ShowResults(storedResponses);

            
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

            var responses = await _unit.Execute(_monitorLoopbackAddresses, _testUntil, false, _cancellationToken);

            ShowResults(responses);

            Assert.That(actual: responses, Is.Not.Empty);
            _pingResponseStorage.Received(2).Store(Arg.Any<PingResponseModel>()).Wait();

        }

        //// unit tests
        //// Call with empty list , true|false -> traces routes to default address and monitors all discovered hops
        //// Call with an address , true|false -> traces routes to address and then monitors all discovered hops
        //// Call with list of addresses , true -> just monitors all addresses - no trace route
        //// Call with list of addresses , false -> traces routes to and then monitors all discover hops of all addresses
        [Test]
        [Category("Unit")]
        public async Task OnExecuteWithNoAddressesNotPingOnlyItWillTraceRouteToTheDefaultAddressAndMonitor()
        {

            var testAddresses = new List<IPAddress>(); // no addresses defined
            PingResponses testTraceRouteResponses;
            PingResponses testPingResponses;
            (testTraceRouteResponses, testPingResponses) = PrepeareTestData(testAddresses);

            _unit = CreateIsolatedUnit(testTraceRouteResponses, testPingResponses);

            testAddresses = new List<IPAddress>();
            var pingOnly = false;

            _traceRouteOrchestrator.Execute(Defaults.DefaultMonitoringDestination, _cancellationToken).Returns(testTraceRouteResponses);

            var responses = await _unit.Execute(testAddresses, _testUntil, pingOnly, _cancellationToken);

            ShowResults(responses);

            // assert traceroute
            await _traceRouteOrchestrator.Received(1).Execute(Defaults.DefaultMonitoringDestination, _cancellationToken);
            // assert monitor
            await _pingOrchestrator.Received(1).PingUntil(Arg.Any<IPAddress[]>(), _testUntil, _cancellationToken);
            await _pingResponseStorage.Received(2).Store(Arg.Any<PingResponseModel>());
        }

        [Test]
        [Category("Unit")]
        public async Task OnExecuteWithOneAddressNotPingOnlyItWillTraceToThatAddressAndMonitor()
        {

            var testAddresses = new List<IPAddress>() { IPAddress.Parse("8.8.4.4") }; // one addresses defined
            PingResponses testTraceRouteResponses;
            PingResponses testPingResponses;
            (testTraceRouteResponses, testPingResponses) = PrepeareTestData(testAddresses);

            _unit = CreateIsolatedUnit(testTraceRouteResponses, testPingResponses);

            var pingOnly = false;

            _traceRouteOrchestrator.Execute(Defaults.DefaultMonitoringDestination, _cancellationToken).Returns(testTraceRouteResponses);

            var responses = await _unit.Execute(testAddresses, _testUntil, pingOnly, _cancellationToken);

            ShowResults(responses);
            // assert traceroute
            await _traceRouteOrchestrator.Received(1).Execute(IPAddress.Parse("8.8.4.4"), _cancellationToken);
            // assert monitor
            await _pingOrchestrator.Received(1).PingUntil(Arg.Any<IPAddress[]>(), _testUntil, _cancellationToken);
            await _pingResponseStorage.Received(2).Store(Arg.Any<PingResponseModel>());
        }

        [Test]
        [Category("Unit")]
        public async Task OnExecuteWithNoTwoAddressNotPingOnlyItWillTraceToThoseAddressesAndMonitor()
        {

            var testAddresses = new List<IPAddress>() { IPAddress.Parse("8.8.8.8"), IPAddress.Parse("8.8.4.4") }; // one addresses defined
            PingResponses testTraceRouteResponses;
            PingResponses testPingResponses;
            (testTraceRouteResponses, testPingResponses) = PrepeareTestData(testAddresses);

            _unit = CreateIsolatedUnit(testTraceRouteResponses, testPingResponses);

            var pingOnly = false;

            _traceRouteOrchestrator.Execute(Defaults.DefaultMonitoringDestination, _cancellationToken).Returns(testTraceRouteResponses);

            var responses = await _unit.Execute(testAddresses, _testUntil, pingOnly, _cancellationToken);

            ShowResults(responses);
            // assert traceroute to both addresses
            await _traceRouteOrchestrator.Received(testAddresses.Count).Execute(Arg.Any<IPAddress>(), _cancellationToken);
            // assert monitor both address from resi;ts
            await _pingOrchestrator.Received(1).PingUntil(Arg.Any<IPAddress[]>(), _testUntil, _cancellationToken);

            /// NOTE: Because we are not able to mock the repeat until its just 1 per address
            await _pingResponseStorage.Received(testAddresses.Count).Store(Arg.Any<PingResponseModel>());
        }
        [Test]
        [Category("Unit")]
        public async Task OnExecuteWithNoTwoAddressPingOnlyItJustMonitors()
        {

            var testAddresses = new List<IPAddress>() { IPAddress.Parse("8.8.8.8"), IPAddress.Parse("8.8.4.4") }; // one addresses defined
            PingResponses testTraceRouteResponses;
            PingResponses testPingResponses;
            (testTraceRouteResponses, testPingResponses) = PrepeareTestData(testAddresses);

            _unit = CreateIsolatedUnit(testTraceRouteResponses, testPingResponses);

            var pingOnly = true;

            _traceRouteOrchestrator.Execute(Defaults.DefaultMonitoringDestination, _cancellationToken).Returns(testTraceRouteResponses);

            var responses = await _unit.Execute(testAddresses, _testUntil, pingOnly, _cancellationToken);

            ShowResults(responses);
            // assert traceroute to both addresses
            await _traceRouteOrchestrator.Received(0).Execute(Arg.Any<IPAddress>(), _cancellationToken);
            // assert monitor both address from resi;ts
            await _pingOrchestrator.Received(1).PingUntil(Arg.Any<IPAddress[]>(), _testUntil, _cancellationToken);

            /// NOTE: Because we are not able to mock the repeat until its just 1 per address
            await _pingResponseStorage.Received(testAddresses.Count).Store(Arg.Any<PingResponseModel>());
        }



      

        private static (PingResponses, PingResponses) PrepeareTestData(List<IPAddress> testAddresses)
        {
            var testTraceRouteResponses = new PingResponses();
            var testPingResponses = new PingResponses();

            if (testAddresses.Count == 0)
            {
                testAddresses.Add(Defaults.DefaultMonitoringDestination);
            }
            foreach (var address in testAddresses)
            {
                PingReplyModel pingReply = new()
                {
                    Address = address,
                    Buffer = Array.Empty<byte>(),
                    Options = new PingOptions(),
                    RoundtripTime = 1,
                    Status = IPStatus.Success
                }; ;

                testTraceRouteResponses.TryAdd(
                        new Tuple<DateTimeOffset, IPAddress>(DateTimeOffset.UtcNow, address),
                        new PingResponseModel()
                        {
                            Request = new PingRequestModel() { Address = address }
                        ,
                            Response = pingReply
                        }); ;
                testPingResponses.TryAdd(
                        new Tuple<DateTimeOffset, IPAddress>(DateTimeOffset.UtcNow, address),
                        new PingResponseModel()
                        {
                            Request = new PingRequestModel() { Address = address }
                        ,
                            Response = pingReply
                        });
            } // simualte target addresses as all being hop 1 addresses

            return (testTraceRouteResponses, testPingResponses);
        }

    }
}

