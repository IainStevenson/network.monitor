using netmon.core.Configuration;
using netmon.core.Data;
using netmon.core.Handlers;
using netmon.core.Models;
using netmon.core.Orchestrators;
using Newtonsoft.Json;

namespace netmon.core.tests
{
    public class TraceRouteOrchestratorTests
    {
        private TraceRouteOrchestrator _unit;
        private IPingHandler _pingHandler;
        private CancellationTokenSource _cancellationTokenSource;
        private CancellationToken _cancellationToken;
        private JsonSerializerSettings _settings;
        private IPingRequestModelFactory _pingRequestModelFactory;

        [SetUp]
        public void Setup()
        {
            // unit setup
            _pingHandler = new PingHandler(new PingHandlerOptions());
            _pingRequestModelFactory = new PingRequestModelFactory();
            var traceRouteHandlerOptions = new TraceRoutOrchestratorOptions();

            _unit = new TraceRouteOrchestrator(_pingHandler, traceRouteHandlerOptions, _pingRequestModelFactory);

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
        public void OnExecuteToLoopbackAddressItReturnsResponses()
        {
            var responses = _unit.Execute(Defaults.LoopbackAddress, _cancellationToken).Result;

            Assert.That(responses.Count, Is.GreaterThan(0));

            ShowResults(responses);
        }

        public void ShowResults(PingResponses results)
        {
            TestContext.Out.WriteLine(JsonConvert.SerializeObject(
               results.AsOrderedList()

               , _settings)
               );
        }

        [Test]
        public void OnExecuteToWorldAddressItReturnsResponses()
        {
            var responses = _unit.Execute(TestConditions.WorldAddresses.Last(), _cancellationToken).Result;

            Assert.That(responses.Count, Is.GreaterThan(0));

            var incorrectTtlValues = responses
                .Where(x => x.Value.Response.Status == System.Net.NetworkInformation.IPStatus.Success)
                .Where(z => z.Value.Response?.Options?.Ttl + z.Value.Ttl + 1 != Defaults.Ttl);
            
            Assert.That(!incorrectTtlValues.Any());

            incorrectTtlValues = responses
                .Where(x => x.Value.Response.Status != System.Net.NetworkInformation.IPStatus.Success)
                .Where(z => (z.Value.Response?.Options?.Ttl?? Defaults.Ttl) != Defaults.Ttl);

            Assert.That(!incorrectTtlValues.Any());



            ShowResults(responses);
        }
    }
}