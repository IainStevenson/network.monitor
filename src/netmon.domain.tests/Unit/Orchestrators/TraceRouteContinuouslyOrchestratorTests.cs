using Microsoft.Extensions.Logging;
using netmon.domain.Configuration;
using netmon.domain.Data;
using netmon.domain.Handlers;
using netmon.domain.Interfaces;
using netmon.domain.Messaging;
using netmon.domain.Models;
using netmon.domain.Orchestrators;
using NSubstitute;
using NSubstitute.ReceivedExtensions;
using System.Net;
using System.Net.NetworkInformation;

namespace netmon.domain.tests.Integration.Orchestrators
{

    public class TraceRouteContinuouslyOrchestratorTests : TestBase<TraceRouteContinuouslyOrchestrator>
    {
        private ILogger<TraceRouteContinuouslyOrchestrator> _unitLogger;
        private ITraceRouteOrchestrator _traceRouteOrchestrator;

        private readonly TimeSpan _testUntil = new(0, 0, 2); //would use this for continuous use: var forEver = new TimeSpan(DateTimeOffset.MaxValue.Ticks - DateTimeOffset.UtcNow.Ticks);

        PingResponseModels traceRouteResponses;


        [SetUp]
        public override void Setup()
        {
            base.Setup();

            _traceRouteOrchestrator = Substitute.For<ITraceRouteOrchestrator>();

            var requestedAddresses = new List<IPAddress>() {  Defaults.DefaultMonitoringDestination}; 

            traceRouteResponses = TestConditions.PrepeareTestData(requestedAddresses);


            _traceRouteOrchestrator
                .Execute(Arg.Any<IPAddress>(), Arg.Any<CancellationToken>())
                .Returns(traceRouteResponses);


            _unitLogger = Substitute.For<ILogger<TraceRouteContinuouslyOrchestrator>>();

            _unit = new TraceRouteContinuouslyOrchestrator(
                _traceRouteOrchestrator,
                _unitLogger);
        }



        [Test]
        [Category("Unit")]
        public async Task OnExecute_WithNoAddresses_ItWillTraceRouteToTheDefaultAddress()
        {
            // Arrange
            var requestedAddresses = new List<IPAddress>(); // no addresses defined

            // Act
            await _unit.Execute(requestedAddresses, _testUntil, _cancellationToken);

            // Assert 
            await _traceRouteOrchestrator
                    .Received(1)
                    .Execute(Defaults.DefaultMonitoringDestination, _cancellationToken);
        }


        [Test]
        [Category("Unit")]
        public async Task OnExecute_WithOneAddress_ItWillTraceToThatAddress()
        {
            // Arrange
            var testAddresses = new List<IPAddress>() { IPAddress.Parse("8.8.4.4") }; // one addresses defined
            _traceRouteOrchestrator.Execute(Defaults.DefaultMonitoringDestination, _cancellationToken).Returns(traceRouteResponses);

            // Act
            await _unit.Execute(testAddresses, _testUntil, _cancellationToken);


            // Assert
            await _traceRouteOrchestrator.Received(1).Execute(IPAddress.Parse("8.8.4.4"), _cancellationToken);

        }

        //TODO: Move to the sub orchestrator tests
        [Test]
        [Category("Unit")]
        public async Task OnExecute_WithTwoAddress_ItWillTraceToThoseAddresses()
        {
            // Arrange
            var testAddresses = new List<IPAddress>() { IPAddress.Parse("8.8.8.8"), IPAddress.Parse("8.8.4.4") };
            _traceRouteOrchestrator.Execute(Defaults.DefaultMonitoringDestination, _cancellationToken).Returns(traceRouteResponses);

            // Act
            await _unit.Execute(testAddresses, _testUntil, _cancellationToken);

            // Assert
            await _traceRouteOrchestrator.Received(testAddresses.Count).Execute(Arg.Any<IPAddress>(), _cancellationToken);

        }

        [Test]
        [Category("Unit")]
        public async Task OnExecute_WithManyAddress_ItTraceesRouteToThemAll()
        {
            // Arrange
            var testAddresses = TestConditions.WorldAddresses;
            _traceRouteOrchestrator.Execute(Defaults.DefaultMonitoringDestination, _cancellationToken).Returns(traceRouteResponses);

            // Act
            await _unit.Execute(testAddresses.ToList(), _testUntil, _cancellationToken);

            // Assert
            await _traceRouteOrchestrator.Received(testAddresses.Count()).Execute(Arg.Any<IPAddress>(), _cancellationToken);
        }





    }
}

