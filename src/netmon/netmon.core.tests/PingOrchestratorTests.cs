using netmon.core.Data;
using netmon.core.Handlers;
using netmon.core.Orchestators;
using Newtonsoft.Json;
using System.Net;

namespace netmon.core.tests
{
    public class PingOrchestratorTests
    {
        private CancellationTokenSource _cancellationTokenSource;
        private CancellationToken _cancellationToken;
        private PingOrchestrator _unit;
        private JsonSerializerSettings _settings;
        private IPingHandler _pingHandler;
        [SetUp]
        public void Setup()
        {
            _pingHandler = new PingHandler();
            _unit = new PingOrchestrator(_pingHandler);
            _cancellationTokenSource = new CancellationTokenSource();
            _cancellationToken = _cancellationTokenSource.Token;
            _settings = new JsonSerializerSettings();
            _settings.Converters.Add(new IPAddressConverter());
            _settings.Converters.Add(new IPEndPointConverter());
            _settings.Converters.Add(new Newtonsoft.Json.Converters.StringEnumConverter());
            _settings.Formatting = Formatting.Indented;
        }

        [Test]
        public void OnExecuteWithDefaltLoopbackRequestFor2SecondsItSucceeeds()
        {
            var duration = new TimeSpan(0, 0, 2);
            var request = new IPAddress[] { IPAddress.Parse("127.0.0.1") };
            var responses = _unit.PingManyUntil(request, duration, _cancellationToken).Result;
            Assert.That(responses.Count, Is.EqualTo(request.Count() * duration.Seconds), "The test returned the wrong number of results");
            Assert.That(responses.Where(x => x.Value.Request.Address is null).Count, Is.EqualTo(0), "One or more null address were returned");
            TestContext.Out.WriteLine(JsonConvert.SerializeObject(responses, _settings));
        }

        [Test]
        public void OnExecuteWithDefaltLoopbackRequestFor4SecondsItSucceeeds()
        {
            var duration = new TimeSpan(0, 0, 4);
            var request = new IPAddress[] { IPAddress.Parse("127.0.0.1") };
            var responses = _unit.PingManyUntil(request, duration, _cancellationToken).Result;
            Assert.That(responses.Count, Is.EqualTo(request.Count() * duration.Seconds), "The test returned the wrong number of results");
            Assert.That(responses.Where(x => x.Value.Request.Address is null).Count, Is.EqualTo(0), "One or more null address were returned");
            TestContext.Out.WriteLine(JsonConvert.SerializeObject(responses, _settings));
        }



        [Test]
        public void OnExecuteWithComplexRequestkRequestFor4SecondsItSucceeeds()
        {
            var duration = new TimeSpan(0, 0, 10);
            var request = new IPAddress[] {
                IPAddress.Parse("127.0.0.1"),
                IPAddress.Parse("192.168.0.1"),
                IPAddress.Parse("172.16.0.1"),
                IPAddress.Parse("172.26.19.5"),
                IPAddress.Parse("172.26.24.142"),
                IPAddress.Parse("172.26.24.93"),
                IPAddress.Parse("172.26.3.146"),
                IPAddress.Parse("185.153.237.154"),
                IPAddress.Parse("185.153.237.155"),
                IPAddress.Parse("216.239.48.217"),
                IPAddress.Parse("142.251.52.145"),
                IPAddress.Parse("142.251.52.145"),
                IPAddress.Parse("8.8.8.8")
            };
            var responses = _unit.PingManyUntil(request, duration, _cancellationToken).Result;
            Assert.That(responses.Count, Is.EqualTo(request.Count() * duration.Seconds), "The test returned the wrong number of results");
            //Assert.That(responses.Where(x => x.Value.Address is null).Count, Is.EqualTo(0), "One or more null address were returned");
            TestContext.Out.WriteLine(JsonConvert.SerializeObject(responses, _settings));
        }


    }
}