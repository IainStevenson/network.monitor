﻿using Microsoft.Extensions.Logging;
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

    /// <summary>
    /// Test Summary:
    /// Fore each <see cref="MonitorModes"/>. 
    /// no addresses passed - <see cref="OnExecuteWithZeroAddresses_ItDoesNothingAndTerminates"/>
    /// One address passed - <see cref="OnExecuteWithOneAddress_ItStores"/>
    /// multiple addresses passed - <see cref="OnExecuteWithMultipleAddresses_ItStoresAll"/>
    /// Timepsan immediately expired (0) - <see cref="OnExecuteWithZeroUntil_ItDoesNothingAndTerminates"/>
    /// Cancelled already - <see cref="OnExecuteWhenAlreadyCancelled_ItDoesNothingAndTerminates"/>
    /// cancels early. - <see cref="OnExecuteWhenCancelled_ItTerminates"/>
    /// Ping Throws exception - <see cref="OnExecuteWhenThrowsException_ItDoesNotStoreButContinuesTrying"/>
    /// </summary>
    public class MonitorOrchestratorIntegrationTests : TestBase<MonitorOrchestrator>
    {
        private ILogger<MonitorPingSubOrchestrator> _monitorPingOnlySubOrchestratorLogger;
        private ILogger<PingHandler> _pingLogger;
        private ILogger<MonitorOrchestrator> _monitorOrchestratorLogger;
        private ILogger<TraceRouteOrchestrator> _traceRouteOrchestratorLogger;

        
        
        private ITraceRouteOrchestrator _traceRouteOrchestrator;
        private IPingOrchestrator _pingOrchestrator;
        private IPingHandler _pingHandler;
        private TraceRouteOrchestratorOptions _traceRouteOrchestratorOptions;
        private IPingRequestModelFactory _pingRequestModelFactory;
        private PingHandlerOptions _pingHandlerOptions;
        private PingOrchestratorOptions _pingOrchestratorOptions;
        private IStorageOrchestrator<PingResponseModel> _pingResponseModelStorageOrchestrator;

        private IMonitorSubOrchestrator _monitorPingOnlySubOrchestrator;
        private IMonitorSubOrchestrator _monitorTraceRouteSubOrchestrator;
        private IMonitorSubOrchestrator _monitorTraceRouteThenPingSubOrchestrator;
        private ILogger<MonitorTraceRouteSubOrchestrator> _monitorTraceRouteSubOrchestratorLogger;
        private ILogger<MonitorTraceRouteThenPingSubOrchestrator> _monitorTraceRouteThenPingSubOrchestratorLogger;
        private readonly List<IPAddress> _monitorLoopbackAddresses = new() { IPAddress.Parse("127.0.0.1") };
        private readonly TimeSpan _testUntil = new(0, 0, 2); //would use this for continuous use: var forEver = new TimeSpan(DateTimeOffset.MaxValue.Ticks - DateTimeOffset.UtcNow.Ticks);



        /// <summary>
        /// To DO: Remove all mocks except Loggers and storage - actually do and get the pings varified
        /// </summary>
        [SetUp]
        public override void Setup()
        {
            base.Setup();


            // unit setup - need to get more interfaces going and uses mocking.
            _pingLogger = Substitute.For<ILogger<PingHandler>>();
            
            _monitorOrchestratorLogger = Substitute.For<ILogger<MonitorOrchestrator>>();

            _monitorPingOnlySubOrchestratorLogger = Substitute.For<ILogger<MonitorPingSubOrchestrator>>();
            _monitorTraceRouteSubOrchestratorLogger = Substitute.For<ILogger<MonitorTraceRouteSubOrchestrator>>();
            _monitorTraceRouteThenPingSubOrchestratorLogger = Substitute.For<ILogger<MonitorTraceRouteThenPingSubOrchestrator>>();
            _traceRouteOrchestratorLogger = Substitute.For<ILogger<TraceRouteOrchestrator>>();
            
            _pingHandlerOptions = new PingHandlerOptions();
            _pingOrchestratorOptions = new PingOrchestratorOptions() { MillisecondsBetweenPings = 1000 };// faster for testing
            _traceRouteOrchestratorOptions = new TraceRouteOrchestratorOptions();
            
            _pingRequestModelFactory = new PingRequestModelFactory(_pingHandlerOptions);
            
            _pingHandler = new PingHandler(_pingHandlerOptions, _pingLogger);
            _traceRouteOrchestrator = new TraceRouteOrchestrator(_pingHandler,
                                                                    _traceRouteOrchestratorOptions,
                                                                    _pingRequestModelFactory,
                                                                    _traceRouteOrchestratorLogger);
            _pingOrchestrator = new PingOrchestrator(_pingHandler, _pingRequestModelFactory, _pingOrchestratorOptions);
            
            
            _pingResponseModelStorageOrchestrator = Substitute.For<IStorageOrchestrator<PingResponseModel>>();// for the moment mock out the storage.

           
            _monitorPingOnlySubOrchestrator = new MonitorPingSubOrchestrator(
                    _pingResponseModelStorageOrchestrator, 
                    _pingOrchestrator,
                    _monitorPingOnlySubOrchestratorLogger);

            _monitorTraceRouteSubOrchestrator = new MonitorTraceRouteSubOrchestrator(
                    _pingResponseModelStorageOrchestrator,
                    _traceRouteOrchestrator,
                    _monitorTraceRouteSubOrchestratorLogger
                );

            _monitorTraceRouteThenPingSubOrchestrator = new MonitorTraceRouteThenPingSubOrchestrator(
                    _pingResponseModelStorageOrchestrator,
                    _traceRouteOrchestrator,
                    _pingOrchestrator,
                    _monitorTraceRouteThenPingSubOrchestratorLogger
                );



            Dictionary<MonitorModes, IMonitorSubOrchestrator> subOrchestrators = new()
            {
                { MonitorModes.PingContinuously, _monitorPingOnlySubOrchestrator },
                { MonitorModes.TraceRouteContinuously, _monitorTraceRouteSubOrchestrator },
                { MonitorModes.TraceRouteThenPingContinuously, _monitorTraceRouteThenPingSubOrchestrator }
            };

            _unit = new MonitorOrchestrator(subOrchestrators, _monitorOrchestratorLogger);
        }

        [Test]
        [Category("Integration")] 
        public void OnExecuteWithZeroAddresses_ItDoesNothingAndTerminates() { Assert.That((1 == 11), Is.EqualTo(true)); }
        [Test]
        [Category("Integration")]
        public void OnExecuteWithOneAddress_ItStores() { Assert.That((1 == 11), Is.EqualTo(true)); }
        [Test]
        [Category("Integration")]
        public void OnExecuteWithMultipleAddresses_ItStoresAll() { Assert.That((1 == 11), Is.EqualTo(true)); }
        [Test]
        [Category("Integration")]
        public void OnExecuteWithZeroUntil_ItDoesNothingAndTerminates() { Assert.That((1 == 11), Is.EqualTo(true)); }
        [Test]
        [Category("Integration")]
        public void OnExecuteWhenAlreadyCancelled_ItDoesNothingAndTerminates() { Assert.That((1 == 11), Is.EqualTo(true)); }
        [Test]
        [Category("Integration")]
        public void OnExecuteWhenCancelled_ItTerminates() { Assert.That((1 == 11), Is.EqualTo(true)); }
        [Test]
        [Category("Integration")]
        public void OnExecuteWhenThrowsException_ItDoesNotStoreButContinuesTrying() { Assert.That((1 == 11), Is.EqualTo(true)); }

        [Test]
        [Category("Integration")]
        public async Task OnExecuteFirstTimeItTracesRouteToDefaultAddressAndMonitorsDiscoveredAddresses()
        {
            // two seconds is long enough , must keep ratio of until to _pingOrchestratorOptions.MillsecondsBetweenPings as  even seconds to get count
            var until = new TimeSpan(0, 0, 2);


            // trap the mock storage results here and display them in the test output.
            PingResponses storedResponses = new();

            _pingResponseModelStorageOrchestrator.When(it => it.Store(Arg.Any<PingResponseModel>()))
                .Do(doit =>
                    storedResponses.TryAdd(new Tuple<DateTimeOffset, IPAddress>(
                        doit.Arg<PingResponseModel>().Start, doit.Arg<PingResponseModel>().Request.Address), doit.Arg<PingResponseModel>()));

            await _unit.Execute(MonitorModes.TraceRouteThenPingContinuously, new List<IPAddress>(), _testUntil, _cancellationToken);

            ShowResults(storedResponses);

            _pingResponseModelStorageOrchestrator.Received((int)(storedResponses.Count)).Store(Arg.Any<PingResponseModel>()).Wait();
        }

        [Test]
        [Category("Integration")]
        public async Task OnExecuteNextTimeItJustMonitorsSpecifiedAddresses()
        {

            await _unit.Execute(MonitorModes.PingContinuously, _monitorLoopbackAddresses, _testUntil, _cancellationToken);

            _pingResponseModelStorageOrchestrator.Received(2).Store(Arg.Any<PingResponseModel>()).Wait();

        }




    }
}

