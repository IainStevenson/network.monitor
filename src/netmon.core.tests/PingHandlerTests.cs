using netmon.core.Configuration;
using netmon.core.Handlers;
using netmon.core.Models;
using System.Net.NetworkInformation;

namespace netmon.core.tests
{
    public class PingHandlerTests: TestBase<PingHandler>
    {
        private PingHandlerOptions _pingHandlerOptions;
        private IPingRequestModelFactory _pingRequestModelFactory;

        [SetUp]
        public override void Setup()
        {
            base.Setup();
            _pingRequestModelFactory = new PingRequestModelFactory();
            _pingHandlerOptions = new PingHandlerOptions();
            _unit = new PingHandler(_pingHandlerOptions);
        }

        [Test]
        public void OnExecuteWithDefaltLoopbackRequestItSucceeeds()
        {
            PingRequestModel request = _pingRequestModelFactory.Create(_pingHandlerOptions);
            PingResponseModel response = _unit.Execute(  request, _cancellationToken ).Result;
            Assert.Multiple(() =>
            {
                Assert.That(response.Response?.Status, Is.EqualTo(IPStatus.Success), "The test was a complete failure");
                Assert.That(response.Response?.RoundtripTime, Is.GreaterThanOrEqualTo(0), "The operation took no time to complete.");
                Assert.That(response.Response?.Options?.Ttl ?? -1, Is.LessThanOrEqualTo(_pingHandlerOptions.Ttl), "The final TTL was not provided or is illegal");
                Assert.That(response.Start, Is.Not.EqualTo(DateTimeOffset.MinValue), "The test did not start");
                Assert.That(response.Finish, Is.Not.EqualTo(DateTimeOffset.MinValue), "The test did not finish");
                Assert.That(response.Duration.TotalMilliseconds, Is.Not.EqualTo(0), "The test took ZERO time");
            });
            ShowResults(response);
        }

        [Test]
        public void OnExecuteWithDefaltLoopbackRequestWithOneMSTimeoutSucceeds()
        {
            PingRequestModel request = _pingRequestModelFactory.Create(_pingHandlerOptions);
            PingResponseModel response = _unit.Execute(request, _cancellationToken).Result;
            Assert.That(response.Response?.Status, Is.EqualTo(IPStatus.Success), "The test was a complete failure");
        }
        [Test]
        public void OnExecuteWithDefaltLoopbackRequestZeroMSTimeoutThrowsException()
        {
            _pingHandlerOptions.Timeout = 0;
            PingRequestModel request = _pingRequestModelFactory.Create(_pingHandlerOptions);
            Assert.ThrowsAsync<PingException>(async () => await _unit.Execute(request, _cancellationToken));
        }
    }
}