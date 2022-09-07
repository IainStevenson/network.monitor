using Microsoft.Extensions.Logging;
using netmon.domain.Configuration;
using netmon.domain.Data;
using netmon.domain.Handlers;
using netmon.domain.Interfaces;
using netmon.domain.Models;
using NSubstitute;
using System.Net.NetworkInformation;

namespace netmon.domain.tests.Integration.Handlers
{
    /// <summary>
    /// Although marked as integration test this is fast as it pings the local address.
    /// Abstracting the Ping object from within the Handler object is superfluous in this rare context.
    /// </summary>
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
        public void OnExecuteWithDefaltLoopbackRequestASuccessResponseIsReturned()
        {
            PingRequestModel request = _pingRequestModelFactory.Create(Defaults.LoopbackAddress);
            PingResponseModel response = _unit.Ping(request, _cancellationToken).Result;
            Assert.Multiple(() =>
            {
                Assert.That(response.Response?.Status, Is.EqualTo(IPStatus.Success), "The test was a complete failure it is probable the environment has no network layer at all");
                Assert.That(response.Response?.RoundtripTime, Is.GreaterThanOrEqualTo(0), "The operation took no time to complete. This is very suspicious");
                Assert.That(response.Response?.Options?.Ttl ?? -1, Is.LessThanOrEqualTo(request.Ttl), "The final TTL was not provided or is illegal");
                Assert.That(response.Start, Is.Not.EqualTo(DateTimeOffset.MinValue), "The operation did not start");
                Assert.That(response.Finish, Is.Not.EqualTo(DateTimeOffset.MinValue), "The operation did not finish");
                Assert.That(response.Duration.TotalMilliseconds, Is.Not.EqualTo(0), "The whole took ZERO time, not possible");
            });
            ShowResults(response);
        }

        [Test]
        [Category("Integration")]
        public void OnExecuteWithDefaltLoopbackRequestAndOneMillisecondTimeoutASuccessResponseIsReturned()
        {
            PingRequestModel request = _pingRequestModelFactory.Create(Defaults.LoopbackAddress);
            PingResponseModel response = _unit.Ping(request, _cancellationToken).Result;
            Assert.That(response.Response?.Status, Is.EqualTo(IPStatus.Success), "The test was a complete failure");
        }
        [Test]
        [Category("Integration")]
        public void OnExecuteWithDefaltLoopbackRequestAndZeroMillisecondsTimeoutItThrowsAPingException()
        {
            _pingHandlerOptions.Timeout = 0;
            PingRequestModel request = _pingRequestModelFactory.Create(Defaults.LoopbackAddress);
            Assert.ThrowsAsync<PingException>(async () => await _unit.Ping(request, _cancellationToken));
        }
    }
}