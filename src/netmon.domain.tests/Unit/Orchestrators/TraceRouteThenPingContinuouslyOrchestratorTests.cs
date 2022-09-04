using Microsoft.Extensions.Logging;
using netmon.domain.Data;
using netmon.domain.Interfaces;
using netmon.domain.Messaging;
using netmon.domain.Models;
using netmon.domain.Orchestrators;
using NSubstitute;
using System.Net;
using System.Net.NetworkInformation;

namespace netmon.domain.tests.Unit.Orchestrators
{
    /// <summary>
    /// Test Summary:
    /// no addresses passed - OnExecute_WithZeroAddresses
    /// One address passed - OnExecute_WithOneAddress
    /// multiple addresses passed - OnExecute_WithMultipleAddresses
    /// Timepsan immediately expired (0) - OnExecute_WithZeroUntil
    /// Cancelled already - OnExecute_WhenAlreadyCancelled
    /// cancels early. - OnExecute_WhenCancelled
    /// Ping Throws exception - OnExecute_WhenThrowsException
    /// </summary>
    public class TraceRouteThenPingContinuouslyOrchestratorTests : TestBase<TraceRouteThenPingContinuouslyOrchestrator>
    {
        private ITraceRouteOrchestrator _traceRouteOrchestrator;
        private IPingOrchestrator _pingOrchestrator;
        private List<IPAddress> testAddresses;
        private PingResponseModels? _traceRouteResponses;
        private PingResponseModels? _pingResponses;
        private readonly TimeSpan _testUntil = new(0, 0, 2); //would use this for continuous use: var forEver = new TimeSpan(DateTimeOffset.MaxValue.Ticks - DateTimeOffset.UtcNow.Ticks);
        private ILogger<TraceRouteThenPingContinuouslyOrchestrator> _unitLogger;
        
        
        [SetUp]
        public override void Setup()
        {
            base.Setup();

            
            _traceRouteOrchestrator = Substitute.For<ITraceRouteOrchestrator>();
            _pingOrchestrator = Substitute.For<IPingOrchestrator>();            

            testAddresses = new List<IPAddress>() { IPAddress.Parse("8.8.8.8"), IPAddress.Parse("8.8.4.4") };

            (_traceRouteResponses, _pingResponses) = PrepeareTestData(testAddresses);

            _traceRouteOrchestrator
                .Execute(Arg.Any<IPAddress>(), Arg.Any<CancellationToken>())
                .Returns(_traceRouteResponses);

            _pingOrchestrator
                .PingUntil(Arg.Any<IPAddress[]>(), _testUntil, Arg.Any<CancellationToken>())
                .Returns(_pingResponses);

            _unitLogger = Substitute.For<ILogger<TraceRouteThenPingContinuouslyOrchestrator>>();
            _unit = new TraceRouteThenPingContinuouslyOrchestrator(_traceRouteOrchestrator, _pingOrchestrator, _unitLogger);
        }


        private static (PingResponseModels, PingResponseModels) PrepeareTestData(List<IPAddress> testAddresses)
        {
            var responsesFromTraceRoute = new PingResponseModels();
            var responsesFromPingUntil = new PingResponseModels();

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

        [Test]
        [Category("Integration")]
        public async Task OnExecute_WithNoAddresses_ItWillTraceRouteToTheDefaultAddressAndThenMonitorAllOfTheDiscoveredAddressesUntil()
        {

            // Arrange
            var requestedAddresses = new List<IPAddress>(); // no addresses defined

         
            // Act
            await _unit.Execute(requestedAddresses, _testUntil, _cancellationToken);

            // Assert
            await _traceRouteOrchestrator
                    .Received(1)
                    .Execute(Defaults.DefaultMonitoringDestination, _cancellationToken);

            await _pingOrchestrator.Received(1)
                    .PingUntil(Arg.Any<IPAddress[]>(), _testUntil, _cancellationToken);

        }

        private const int NumberofStorageCalls = 1;

        [Test]
        [Category("Integration")]
        public async Task OnExecute_WithOneAddress_ItWillTraceToThatAddressAndMonitor()
        {
            // Arrange
            var requestedAddresses = new List<IPAddress>() { IPAddress.Parse("8.8.4.4") };

            // Act
            await _unit.Execute(requestedAddresses, _testUntil, _cancellationToken);

          
            // Assert
            await _traceRouteOrchestrator
                    .Received(1)
                    .Execute(Arg.Any<IPAddress>(), _cancellationToken);

            await _pingOrchestrator.Received(1)
                    .PingUntil(Arg.Any<IPAddress[]>(), _testUntil, _cancellationToken);



        }

        [Test]
        [Category("Integration")]
        public async Task OnExecute_WithTwoAddress_ItWillTraceToThoseAddressesAndMonitor()
        {

            // Arrange

            // Act            
            await _unit.Execute(testAddresses, _testUntil, _cancellationToken);

           
            // Assert 
            await _traceRouteOrchestrator.Received(testAddresses.Count).Execute(Arg.Any<IPAddress>(), _cancellationToken);            
            await _pingOrchestrator.Received(1).PingUntil(Arg.Any<IPAddress[]>(), _testUntil, _cancellationToken);

            
        }


    }
}

