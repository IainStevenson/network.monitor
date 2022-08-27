using Microsoft.Extensions.Logging;
using netmon.domain.Configuration;
using netmon.domain.Handlers;
using netmon.domain.Interfaces;
using netmon.domain.Models;
using NSubstitute;
using System.Net.NetworkInformation;

namespace netmon.domain.tests.Integration.Handlers
{
    public class PingHandlerIntegrationTests : TestBase<PingHandler>
    {
        private PingHandlerOptions _pingHandlerOptions;
        private IPingRequestModelFactory _pingRequestModelFactory;
        private ILogger<PingHandler> _pingHandlerLogger;

        [SetUp]
        public override void Setup()
        {
            base.Setup();
            _pingRequestModelFactory = new PingRequestModelFactory(_pingHandlerOptions);
            _pingHandlerOptions = new PingHandlerOptions();
            _pingHandlerLogger = Substitute.For<ILogger<PingHandler>>();
            _unit = new PingHandler(_pingHandlerOptions, _pingHandlerLogger);
        }

        [Test]
        [Category("Integration")]
        public void OnExecuteWithDefaltLoopbackRequestItSucceeeds()
        {
            PingRequestModel request = _pingRequestModelFactory.Create();
            PingResponseModel response = _unit.Execute(request, _cancellationToken).Result;
            Assert.Multiple(() =>
            {
                Assert.That(response.Response?.Status, Is.EqualTo(IPStatus.Success), "The test was a complete failure");
                Assert.That(response.Response?.RoundtripTime, Is.GreaterThanOrEqualTo(0), "The operation took no time to complete.");
                Assert.That(response.Response?.Options?.Ttl ?? -1, Is.LessThanOrEqualTo(request.Ttl), "The final TTL was not provided or is illegal");
                Assert.That(response.Start, Is.Not.EqualTo(DateTimeOffset.MinValue), "The test did not start");
                Assert.That(response.Finish, Is.Not.EqualTo(DateTimeOffset.MinValue), "The test did not finish");
                Assert.That(response.Duration.TotalMilliseconds, Is.Not.EqualTo(0), "The test took ZERO time");
            });
            ShowResults(response);
        }

        [Test]
        [Category("Integration")]
        public void OnExecuteWithDefaltLoopbackRequestWithOneMSTimeoutSucceeds()
        {
            PingRequestModel request = _pingRequestModelFactory.Create();
            PingResponseModel response = _unit.Execute(request, _cancellationToken).Result;
            Assert.That(response.Response?.Status, Is.EqualTo(IPStatus.Success), "The test was a complete failure");
        }
        [Test]
        [Category("Integration")]
        public void OnExecuteWithDefaltLoopbackRequestZeroMSTimeoutThrowsException()
        {
            _pingHandlerOptions.Timeout = 0;
            PingRequestModel request = _pingRequestModelFactory.Create();
            Assert.ThrowsAsync<PingException>(async () => await _unit.Execute(request, _cancellationToken));
        }
    }
}