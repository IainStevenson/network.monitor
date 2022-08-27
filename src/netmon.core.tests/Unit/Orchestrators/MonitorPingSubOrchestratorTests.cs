using Microsoft.Extensions.Logging;
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
    /// Test Summary:
    /// no addresses passed - <see cref="OnHandleWithZeroAddresses_ItDoesNothingAndTerminates"/>
    /// One address passed - <see cref="OnHandleWithOneAddress_ItStores"/>
    /// multiple addresses passed - <see cref="OnHandleWithMultipleAddresses_ItStoresAll"/>
    /// Timepsan immediately expired (0) - <see cref="OnHandleWithZeroUntil_ItDoesNothingAndTerminates"/>
    /// Cancelled already - <see cref="OnHandleWhenAlreadyCancelled_ItDoesNothingAndTerminates"/>
    /// cancels early. - <see cref="OnHandleWhenCancelled_ItTerminates"/>
    /// Ping Throws exception - <see cref="OnHandleWhenThrowsException_ItDoesNotStoreButContinuesTrying"/>
    /// </summary>
    public class MonitorPingSubOrchestratorTests : TestBase<MonitorPingSubOrchestrator>
    {
        private IStorageOrchestrator<PingResponseModel> _pingResponseModelStorageOrchestrator;
        private TestStorageOrchestrator? _mockStorageOrcestrator;
        private IPingOrchestrator _pingOrchestrator;
        private ILogger<MonitorPingSubOrchestrator> _monitorOrchestratorLogger;
        private static PingResponses _pingTestResponses = new();
        private TimeSpan _testUntil = new(0, 0, 2); 

        [SetUp]
        public override void Setup()
        {
            base.Setup();

            _pingResponseModelStorageOrchestrator = new TestStorageOrchestrator();
            _mockStorageOrcestrator = _pingResponseModelStorageOrchestrator as TestStorageOrchestrator;
            _pingOrchestrator = Substitute.For<IPingOrchestrator>();
            _monitorOrchestratorLogger = Substitute.For<ILogger<MonitorPingSubOrchestrator>>();
            _unit = new MonitorPingSubOrchestrator(_pingResponseModelStorageOrchestrator, _pingOrchestrator, _monitorOrchestratorLogger);
        }


        /// <summary>
        /// Setup test <see cref="PingResponses"/> to simualte target addresses as all being hop 1 addresses. 
        /// will addd default monitoring address if no addresses sepcified as per the code behaviour.
        /// </summary>
        /// <param name="testAddresses">The addreses to simulate</param>
        /// <returns>void</returns>

        private void TestDatasetup(List<IPAddress> testAddresses)
        {
            _pingTestResponses = new PingResponses();

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

            await _pingOrchestrator.Received(0).PingUntil(Arg.Any<IPAddress[]>(), _testUntil, _cancellationToken);

            Assert.That(_mockStorageOrcestrator?.StorageRequestCount, Is.EqualTo(0));

        }

       

        [Test]
        [Category("Unit")]
        public async Task OnHandleWithOneAddress_ItStores()
        {

            // Arrange
            var testAddresses = new List<IPAddress>() { IPAddress.Parse("8.8.4.4") }; // one addresses defined

            TestDatasetup(testAddresses);

            // Act
            await _unit.Execute(testAddresses, _testUntil, _cancellationToken);


            // assert 
            await _pingOrchestrator.Received(_pingTestResponses.Count).PingUntil(Arg.Any<IPAddress[]>(), _testUntil, _cancellationToken);

            Assert.That(_mockStorageOrcestrator?.StorageRequestCount, Is.EqualTo(_pingTestResponses.Count));


        }

        [Test]
        [Category("Unit")]
        public async Task OnHandleWithMultipleAddresses_ItStoresAll()
        {
            // Arrange
            var testAddresses = new List<IPAddress>() { IPAddress.Parse("8.8.8.8"), IPAddress.Parse("8.8.4.4") };
            
            TestDatasetup(testAddresses);

            // Act

            await _unit.Execute(testAddresses, _testUntil, _cancellationToken);

            // assert 
            await _pingOrchestrator.Received(testAddresses.Count).PingUntil(Arg.Any<IPAddress[]>(), Arg.Any<TimeSpan>(), Arg.Any<CancellationToken>());

            Assert.That(_mockStorageOrcestrator?.StorageRequestCount, Is.EqualTo(testAddresses.Count));
        }


        [Test]
        [Category("Unit")]
        public async Task OnHandleWithZeroUntil_ItDoesNothingAndTerminates() {

            // Arrange
            var testAddresses = new List<IPAddress>() { IPAddress.Parse("8.8.4.4") }; // one addresses defined

            TestDatasetup(testAddresses);

            _testUntil = new(0);

            // Act
            await _unit.Execute(testAddresses, _testUntil, _cancellationToken);


            // assert 
            await _pingOrchestrator.Received(0).PingUntil(Arg.Any<IPAddress[]>(), _testUntil, _cancellationToken);

            Assert.That(_mockStorageOrcestrator?.StorageRequestCount, Is.EqualTo(0));

        }


        [Test]
        [Category("Unit")]
        public async Task OnHandleWhenAlreadyCancelled_ItDoesNothingAndTerminates() {

            // Arrange
            var testAddresses = new List<IPAddress>() { IPAddress.Parse("8.8.4.4") }; // one addresses defined

            TestDatasetup(testAddresses);

            _cancellationTokenSource.Cancel();

            // Act
            await _unit.Execute(testAddresses, _testUntil, _cancellationToken);


            // assert 
            await _pingOrchestrator.Received(0).PingUntil(Arg.Any<IPAddress[]>(), _testUntil, _cancellationToken);

            Assert.That(_mockStorageOrcestrator?.StorageRequestCount, Is.EqualTo(0));

        }


        [Test]
        [Category("Unit")]
        public async Task OnHandleWhenCancelled_ItTerminates() {

            // Arrange
            var testAddresses = new List<IPAddress>() { IPAddress.Parse("127.0.0.1") }; 

            TestDatasetup(testAddresses);

            _cancellationTokenSource.CancelAfter( 5001 );

            // Act
            await _unit.Execute(testAddresses, _testUntil, _cancellationToken);


            // assert 
            await _pingOrchestrator.Received(_pingTestResponses.Count).PingUntil(Arg.Any<IPAddress[]>(), _testUntil, _cancellationToken);

            Assert.That(_mockStorageOrcestrator?.StorageRequestCount, Is.EqualTo(_pingTestResponses.Count));


        }
        [Test]
        [Category("Unit")]
        public async Task OnHandleWhenThrowsException_ItDoesNotStoreButContinuesTrying()
        {

            // Arrange
            var testAddresses = new List<IPAddress>() { IPAddress.Parse("8.8.4.4") }; // one addresses defined

            TestDatasetup(testAddresses);

            
            _pingOrchestrator.PingUntil(Arg.Any<IPAddress[]>(), Arg.Any<TimeSpan>(), Arg.Any<CancellationToken>()).ThrowsAsync<Exception>();

            // Act
            await _unit.Execute(testAddresses, _testUntil, _cancellationToken);


            // assert 
            await _pingOrchestrator.Received(1).PingUntil(Arg.Any<IPAddress[]>(), Arg.Any<TimeSpan>(), Arg.Any<CancellationToken>());

            Assert.That(_mockStorageOrcestrator?.StorageRequestCount, Is.EqualTo(1));

        }
    }
}

