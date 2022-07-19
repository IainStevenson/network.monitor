using netmon.core.Handlers;
using System.Net.NetworkInformation;

namespace netmon.core.tests
{
    public class PingHandlerTests
    {
        private CancellationTokenSource _cancellationTokenSource;
        private CancellationToken _cancellationToken;
        private PingHandler _unit;
        [SetUp]
        public void Setup()
        {
            _unit = new PingHandler();
            _cancellationTokenSource = new CancellationTokenSource();
            _cancellationToken = _cancellationTokenSource.Token;
        }

        [Test]
        public void OnExecuteWithDefaltLoopbackRequestItSucceeeds()
        {
            var request = new Models.PingRequestModel();
            var response = _unit.Execute(  request, _cancellationToken ).Result;
            Assert.That(response.Response.Status, Is.EqualTo(IPStatus.Success), "The test was a complete failure");
            Assert.That(response.Response.RoundtripTime, Is.GreaterThanOrEqualTo(0), "The operation took no time to complete.");
            Assert.That(response.Response.Options.Ttl, Is.LessThanOrEqualTo(request.Options.Ttl), "The test has gone wrong");
            Assert.That(response.Start, Is.Not.EqualTo(DateTimeOffset.MinValue), "The test did not start");
            Assert.That(response.Finish, Is.Not.EqualTo(DateTimeOffset.MinValue), "The test did not finish");
            Assert.That(response.Duration.TotalMilliseconds, Is.Not.EqualTo(0), "The test took ZERO time");
        }

        [Test]
        public void OnExecuteWithDefaltLoopbackRequestWithOneMSTimeoutSucceeds()
        {
            var request = new Models.PingRequestModel() {  Timeout = 1 };
            var response = _unit.Execute(request, _cancellationToken).Result;
            Assert.That(response.Response.Status, Is.EqualTo(IPStatus.Success), "The test was a complete failure");
        }
        [Test]
        public void OnExecuteWithDefaltLoopbackRequestZeroMSTimeoutThrowsException()
        {
            var request = new Models.PingRequestModel() { Timeout = 0 };
            Assert.ThrowsAsync<PingException>(async () => await _unit.Execute(request, _cancellationToken));
        }
    }
}