using Microsoft.Extensions.Logging;
using netmon.domain.Configuration;
using netmon.domain.Data;
using netmon.domain.Interfaces;
using netmon.domain.Messaging;
using netmon.domain.Models;
using netmon.domain.Orchestrators;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using NSubstitute.ReceivedExtensions;
using System.Net;
using System.Net.NetworkInformation;

namespace netmon.domain.tests.Integration.Orchestrators
{
    /// <summary>
    /// 
    /// This is jsut a wrap and pass to storage mechanism. It adds nothing to the 
    /// Test Summary:
    /// no addresses passed - <see cref="OnHandleWithZeroAddresses_ItDoesNothingAndTerminates"/>
    /// One address passed - <see cref="OnHandleWithOneAddress_ItStores"/>
    /// multiple addresses passed - <see cref="OnHandleWithMultipleAddresses_ItStoresAll"/>
    /// Timepsan immediately expired (0) - <see cref="OnHandleWithZeroUntil_ItDoesNothingAndTerminates"/>
    /// Cancelled already - <see cref="OnHandleWhenAlreadyCancelled_ItDoesNothingAndTerminates"/>
    /// cancels early. - <see cref="OnHandleWhenCancelled_ItTerminates"/>
    /// Ping Throws exception - <see cref="OnHandleWhenThrowsException_ItDoesNotStoreButContinuesTrying"/>
    /// </summary>
    public class PingContinuouslyOrchestratorTests : TestBase<PingContinuouslyOrchestrator>
    {
        //private IStorageOrchestrator<PingResponseModel> _pingResponseModelStorageOrchestrator;
        //private TestStorageOrchestrator? _mockStorageOrcestrator;
        private IPingOrchestrator _pingOrchestrator;
        private ILogger<PingContinuouslyOrchestrator> _unitLogger;
        private static PingResponseModels _pingTestResponses = new();
        private PingOrchestratorOptions _pingOrchestratorOptions;
        private TimeSpan _testUntil = new(0, 0, 2);

        [SetUp]
        public override void Setup()
        {
            base.Setup();
            _pingOrchestratorOptions = new PingOrchestratorOptions() { MillisecondsBetweenPings = 1000 }; // speed things up for testing
            _pingOrchestrator = Substitute.For<IPingOrchestrator>();
            _unitLogger = Substitute.For<ILogger<PingContinuouslyOrchestrator>>();
            _unit = new PingContinuouslyOrchestrator(_pingOrchestrator, _pingOrchestratorOptions, _unitLogger);
        }


        /// <summary>
        /// Setup test <see cref="PingResponses"/> to simualte target addresses as all being hop 1 addresses. 
        /// will addd default monitoring address if no addresses sepcified as per the code behaviour.
        /// </summary>
        /// <param name="testAddresses">The addreses to simulate</param>
        /// <returns>void</returns>

        private void TestDatasetup(List<IPAddress> testAddresses)
        {
            _pingTestResponses = new PingResponseModels();

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

                _pingTestResponses.TryAdd(
                        new Tuple<DateTimeOffset, IPAddress>(DateTimeOffset.UtcNow, address),
                        new PingResponseModel()
                        {
                            Request = new PingRequestModel() { Address = address }
                        ,
                            Response = pingReply
                        });
            }


            _pingOrchestrator
                .PingUntil(Arg.Any<IPAddress[]>(), _testUntil, Arg.Any<CancellationToken>())
                .Returns(_pingTestResponses);

        }


        [Test]
        [Category("Unit")]
        public async Task OnHandleWithZeroAddresses_ItDoesNothingAndTerminates()
        {

            var requestedAddresses = new List<IPAddress>(); // no addresses defined

            TestDatasetup(requestedAddresses);

            requestedAddresses.Clear(); // remove the added default address

            // Act
            await _unit.Execute(requestedAddresses, _testUntil, _cancellationToken);

            // assert 

            await _pingOrchestrator.Received(1).PingUntil(Arg.Any<IPAddress[]>(), _testUntil, _cancellationToken);

            

        }



        [Test]
        [Category("Unit")]
        public async Task OnExecuteWhenPreCancelled_ItStillPassesCallToHandler()
        {

            // Arrange
            var testAddresses = new List<IPAddress>() { IPAddress.Parse("127.0.0.1") };

            TestDatasetup(testAddresses);

            _cancellationTokenSource.Cancel();

            // Act
            await _unit.Execute(testAddresses, _testUntil, _cancellationToken);


            // assert 
            await _pingOrchestrator.Received(1).PingUntil(Arg.Any<IPAddress[]>(), _testUntil, _cancellationToken);


        }

        [Test]
        [Category("Unit")]
        public async Task OnExecuteWhenHAndlerThrowsException_ItDoesContinuesTrying()
        {

            // Arrange
            var testAddresses = new List<IPAddress>() { IPAddress.Parse("8.8.4.4") }; // one addresses defined

            TestDatasetup(testAddresses);


            _pingOrchestrator.PingUntil(Arg.Any<IPAddress[]>(), Arg.Any<TimeSpan>(), Arg.Any<CancellationToken>()).ThrowsAsync<Exception>();

            // Act
            await _unit.Execute(testAddresses, _testUntil, _cancellationToken);


            // assert 
            await _pingOrchestrator.Received(1).PingUntil(Arg.Any<IPAddress[]>(), Arg.Any<TimeSpan>(), Arg.Any<CancellationToken>());

        }
    }
}

