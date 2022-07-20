using netmon.core.Configuration;
using netmon.core.Data;
using netmon.core.Handlers;
using netmon.core.Models;
using netmon.core.Orchestrators;
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
        private PingHandlerOptions _pingHandlerOptions;
        private IPingRequestModelFactory _pingRequestModelFactory;
        [SetUp]
        public void Setup()
        {
            // unit setup
            _pingRequestModelFactory = new PingRequestModelFactory();
            _pingHandlerOptions = new PingHandlerOptions();
            _pingHandler = new PingHandler(_pingHandlerOptions);

            _unit = new PingOrchestrator(_pingHandler, _pingRequestModelFactory);

            // control setup
            _cancellationTokenSource = new CancellationTokenSource();
            _cancellationToken = _cancellationTokenSource.Token;
            
            // output setup
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
            var request = new IPAddress[] { Defaults.LoopbackAddress };
            var responses = _unit.PingManyUntil(request, duration, _cancellationToken).Result;
            Assert.That(responses.Count, Is.EqualTo(request.Count() * duration.Seconds), "The test returned the wrong number of results");
            Assert.That(responses.Where(x => x.Value.Request.Address is null).Count, Is.EqualTo(0), "One or more null address were returned");
            TestContext.Out.WriteLine(JsonConvert.SerializeObject(responses, _settings));
        }

        [Test]
        public void OnExecuteWithDefaltLoopbackRequestFor3SecondsItSucceeeds()
        {
            var duration = new TimeSpan(0, 0, 3);
            var request = new IPAddress[] { Defaults.LoopbackAddress };
            var responses = _unit.PingManyUntil(request, duration, _cancellationToken).Result;
            Assert.That(responses.Count, Is.EqualTo(request.Count() * duration.Seconds), "The test returned the wrong number of results");
            Assert.That(responses.Where(x => x.Value.Request.Address is null).Count, Is.EqualTo(0), "One or more null address were returned");
            TestContext.Out.WriteLine(JsonConvert.SerializeObject(responses, _settings));
        }



        [Test]
        public void OnExecuteWithComplexRequestkRequestFor4SecondsItSucceeeds()
        {
            var duration = new TimeSpan(0, 0, 10);
            var request = new List<IPAddress>();
            request.AddRange(TestConditions.LocalAddresses);
            request.AddRange(TestConditions.WorldAddresses);

            var responses = _unit.PingManyUntil(request.ToArray(), duration, _cancellationToken).Result;
            Assert.That(responses.Count, Is.EqualTo(request.Count() * duration.Seconds), "The test returned the wrong number of results");
            TestContext.Out.WriteLine(JsonConvert.SerializeObject(responses, _settings));
        }


    }
}