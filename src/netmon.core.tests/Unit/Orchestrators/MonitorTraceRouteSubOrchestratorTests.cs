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
    /// <summary>
    /// Test Summary:
    /// 
    /// Things to test:
    /// no addresses passed - OnHandleWithZeroAddresses
    /// One address passed - OnHandleWithOneAddress
    /// multiple addresses passed - OnHandleWithMultipleAddresses
    /// Timepsan immediately expired (0) - OnHandleWithZeroUntil
    /// Cancelled already - OnHandleWhenAlreadyCancelled
    /// cancels early. - OnHandleWhenCancelled
    /// Ping Throws exception - OnHandleWhenThrowsException
    /// </summary>
    public class MonitorTraceRouteSubOrchestratorTests : TestBase<MonitorTraceRouteSubOrchestrator>
    {
        //TODO: Move to the sub orchestrator tests
        //private MonitorOptions _monitorOptions;
        private ITraceRouteOrchestrator _traceRouteOrchestrator;
        private IPingOrchestrator _pingOrchestrator;
        private IPingHandler _pingHandler;
        private TraceRouteOrchestratorOptions _traceRouteOrchestratorOptions;
        private IPingRequestModelFactory _pingRequestModelFactory;
        private PingHandlerOptions _pingHandlerOptions;
        private PingOrchestratorOptions _pingOrchestratorOptions;
        private IStorageOrchestrator<PingResponseModel> _pingResponseModelStorageOrchestrator;
        private ILogger<PingHandler> _pingLogger;
        private ILogger<MonitorTraceRouteSubOrchestrator> _monitorOrchestratorLogger;
        private ILogger<TraceRouteOrchestrator> _traceRouteOrchestratorLogger;
        private readonly List<IPAddress> _monitorLoopbackAddresses = new() { IPAddress.Parse("127.0.0.1") };
        private readonly TimeSpan _testUntil = new(0, 0, 2); //would use this for continuous use: var forEver = new TimeSpan(DateTimeOffset.MaxValue.Ticks - DateTimeOffset.UtcNow.Ticks);

        /// <summary>
        ///         //TODO: Move most of the content here to the sub orchestrator tests
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
            _pingResponseModelStorageOrchestrator = Substitute.For<IStorageOrchestrator<PingResponseModel>>();


            _monitorOrchestratorLogger = Substitute.For<ILogger<MonitorTraceRouteSubOrchestrator>>();
            _unit = new MonitorTraceRouteSubOrchestrator(
                _pingResponseModelStorageOrchestrator,
                _traceRouteOrchestrator,
                _monitorOrchestratorLogger);
        }


        private MonitorTraceRouteSubOrchestrator CreateIsolatedUnit(PingResponses traceRouteResponses, PingResponses pingResponses)
        {
            _traceRouteOrchestrator = Substitute.For<ITraceRouteOrchestrator>();
            _pingOrchestrator = Substitute.For<IPingOrchestrator>();

            _traceRouteOrchestrator
                .Execute(Arg.Any<IPAddress>(), Arg.Any<CancellationToken>())
                .Returns(traceRouteResponses);
            _pingOrchestrator
                .PingUntil(Arg.Any<IPAddress[]>(), _testUntil, Arg.Any<CancellationToken>())
                .Returns(pingResponses);

            // dont do this as it duplicates events
            //_pingOrchestrator.Results += (sender, args) => 
            //        _pingResponseStorage.Store(args.Model); // call through to the mocked storage
            // now make sure the event is raised...when the mocked method is called

            _pingOrchestrator.When(it => it.PingUntil(Arg.Any<IPAddress[]>(), _testUntil, Arg.Any<CancellationToken>()))

            .Do(doit =>
                    _pingOrchestrator.Results +=
                    Raise.Event<EventHandler<PingResponseModelEventArgs>>(
                        this,
                        new PingResponseModelEventArgs(new PingResponseModel() { }))
                ); // add address to event data? perhaps multiple calls via foreach?


            return new MonitorTraceRouteSubOrchestrator(
                    _pingResponseModelStorageOrchestrator,
                    _traceRouteOrchestrator,
                    _monitorOrchestratorLogger);
        }



        //TODO: Move to the sub orchestrator tests
        //// unit tests
        //// Call with empty list , true|false -> traces routes to default address and monitors all discovered hops
        //// Call with an address , true|false -> traces routes to address and then monitors all discovered hops
        //// Call with list of addresses , true -> just monitors all addresses - no trace route
        //// Call with list of addresses , false -> traces routes to and then monitors all discover hops of all addresses
        [Test]
        [Category("Unit")]
        public async Task OnExecuteWithNoAddressesAllowingTraceRouteItWillTraceRouteToTheDefaultAddressAndThenMonitorAllOfTheDiscoveredAddresses()
        {

            var requestedAddresses = new List<IPAddress>(); // no addresses defined

            PingResponses responsesFromTraceRoute;
            PingResponses responsesFromPingUntil;
            (responsesFromTraceRoute, responsesFromPingUntil) = PrepeareTestData(requestedAddresses);
            requestedAddresses.Clear(); // because PrepareTestData correctly Adds the default monitor address if empty.

            _unit = CreateIsolatedUnit(responsesFromTraceRoute, responsesFromPingUntil);

            await _unit.Execute(requestedAddresses, _testUntil, _cancellationToken);

            //ShowResults(responses);

            // assert traceroute called for default trace/monitor address
            await _traceRouteOrchestrator
                    .Received(1)
                    .Execute(Defaults.DefaultMonitoringDestination, _cancellationToken);

            // assert monitor
            await _pingOrchestrator.Received(1)
                    .PingUntil(Arg.Any<IPAddress[]>(), _testUntil, _cancellationToken);

            /// NOTE: Because we are not able to mock the repeat until or loops for multiple addresses 
            /// its just 1 per ping request
            await _pingResponseModelStorageOrchestrator.Received(NumberofStorageCalls)
                    .StoreAsync(Arg.Any<PingResponseModel>());

        }

        private const int NumberofStorageCalls = 1;

        //TODO: Move to the sub orchestrator tests
        [Test]
        [Category("Unit")]
        public async Task OnExecuteWithOneAddressRequestingTraceRouteItWillTraceToThatAddressAndMonitor()
        {

            var testAddresses = new List<IPAddress>() { IPAddress.Parse("8.8.4.4") }; // one addresses defined
            PingResponses responsesFromTraceRoute;
            PingResponses responsesFromPingUntil;
            (responsesFromTraceRoute, responsesFromPingUntil) = PrepeareTestData(testAddresses);

            _unit = CreateIsolatedUnit(responsesFromTraceRoute, responsesFromPingUntil);


            _traceRouteOrchestrator.Execute(Defaults.DefaultMonitoringDestination, _cancellationToken).Returns(responsesFromTraceRoute);

            await _unit.Execute(testAddresses, _testUntil, _cancellationToken);

            //ShowResults(responses);
            // assert traceroute
            await _traceRouteOrchestrator.Received(1).Execute(IPAddress.Parse("8.8.4.4"), _cancellationToken);
            // assert monitor
            await _pingOrchestrator.Received(responsesFromTraceRoute.Count).PingUntil(Arg.Any<IPAddress[]>(), _testUntil, _cancellationToken);
            /// NOTE: Because we are not able to mock the repeat until or loops for multiple addresses 
            /// its just 1 per ping request
            await _pingResponseModelStorageOrchestrator.Received(NumberofStorageCalls)
                    .StoreAsync(Arg.Any<PingResponseModel>());

        }

        //TODO: Move to the sub orchestrator tests
        [Test]
        [Category("Unit")]
        public async Task OnExecuteWithTwoAddressRequestingTraceRouteItWillTraceToThoseAddressesAndMonitor()
        {

            var testAddresses = new List<IPAddress>() { IPAddress.Parse("8.8.8.8"), IPAddress.Parse("8.8.4.4") };
            PingResponses responsesFromTraceRoute;
            PingResponses responsesFromPingUntil;
            (responsesFromTraceRoute, responsesFromPingUntil) = PrepeareTestData(testAddresses);

            _unit = CreateIsolatedUnit(responsesFromTraceRoute, responsesFromPingUntil);

            _traceRouteOrchestrator.Execute(Defaults.DefaultMonitoringDestination, _cancellationToken).Returns(responsesFromTraceRoute);

            await _unit.Execute(testAddresses, _testUntil, _cancellationToken);

            //ShowResults(responses);

            // assert traceroute to both addresses
            await _traceRouteOrchestrator.Received(testAddresses.Count)
                .Execute(Arg.Any<IPAddress>(), _cancellationToken);

            // assert monitor both address from results
            await _pingOrchestrator.Received(1)
                .PingUntil(Arg.Any<IPAddress[]>(), _testUntil, _cancellationToken);

            /// NOTE: Because we are not able to mock the repeat until or loops for multiple addresses 
            /// its just 1 per ping request
            await _pingResponseModelStorageOrchestrator.Received(1)
                    .StoreAsync(Arg.Any<PingResponseModel>());
        }
        //TODO: Move to the sub orchestrator tests
        [Test]
        [Category("Unit")]
        public async Task OnExecuteWithTwoAddressPreventingTraceRouteItJustMonitors()
        {

            var testAddresses = new List<IPAddress>() { IPAddress.Parse("8.8.8.8"), IPAddress.Parse("8.8.4.4") }; // one addresses defined
            PingResponses responsesFromTraceRoute;
            PingResponses responsesFromPingUntil;
            (responsesFromTraceRoute, responsesFromPingUntil) = PrepeareTestData(testAddresses);

            _unit = CreateIsolatedUnit(responsesFromTraceRoute, responsesFromPingUntil);


            _traceRouteOrchestrator.Execute(Defaults.DefaultMonitoringDestination, _cancellationToken).Returns(responsesFromTraceRoute);

            await _unit.Execute( testAddresses, _testUntil, _cancellationToken);

            //ShowResults(responses);
            // assert traceroute to both addresses
            await _traceRouteOrchestrator.Received(0).Execute(Arg.Any<IPAddress>(), _cancellationToken);
            // assert monitor both address from results
            await _pingOrchestrator.Received(1).PingUntil(Arg.Any<IPAddress[]>(), _testUntil, _cancellationToken);

            /// NOTE: Because we are not able to mock the repeat until or loops for multiple addresses 
            /// its just 1 per ping request
            await _pingResponseModelStorageOrchestrator.Received(NumberofStorageCalls)
                    .StoreAsync(Arg.Any<PingResponseModel>());

        }




        //TODO: Move to the sub orchestrator tests
        /// <summary>
        /// Return a pair of matched test <see cref="PingResponses"/> to simualte target addresses as all being hop 1 addresses. will addd default monitoring address if no addresses sepcified as per the code behaviour.
        /// </summary>
        /// <param name="testAddresses">The addreses to simulate</param>
        /// <returns>matched pair of test <see cref="PingResponses"/></returns>

        private static (PingResponses, PingResponses) PrepeareTestData(List<IPAddress> testAddresses)
        {
            var responsesFromTraceRoute = new PingResponses();
            var responsesFromPingUntil = new PingResponses();

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

                responsesFromTraceRoute.TryAdd(
                        new Tuple<DateTimeOffset, IPAddress>(DateTimeOffset.UtcNow, address),
                        new PingResponseModel()
                        {
                            Request = new PingRequestModel() { Address = address }
                        ,
                            Response = pingReply
                        }); ;
                responsesFromPingUntil.TryAdd(
                        new Tuple<DateTimeOffset, IPAddress>(DateTimeOffset.UtcNow, address),
                        new PingResponseModel()
                        {
                            Request = new PingRequestModel() { Address = address }
                        ,
                            Response = pingReply
                        });
            } // 

            return (responsesFromTraceRoute, responsesFromPingUntil);
        }
    }
}

