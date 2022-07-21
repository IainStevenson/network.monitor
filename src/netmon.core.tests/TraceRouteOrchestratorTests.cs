using netmon.core.Configuration;
using netmon.core.Data;
using netmon.core.Handlers;
using netmon.core.Models;
using netmon.core.Orchestrators;

namespace netmon.core.tests
{
    public class TraceRouteOrchestratorTests : TestBase<TraceRouteOrchestrator>
    {
        private IPingHandler _pingHandler;       
        private IPingRequestModelFactory _pingRequestModelFactory;

        [SetUp]
        public void Setup()
        {
            // unit setup
            _pingHandler = new PingHandler(new PingHandlerOptions());
            _pingRequestModelFactory = new PingRequestModelFactory();
            var traceRouteHandlerOptions = new TraceRouteOrchestratorOptions();

            _unit = new TraceRouteOrchestrator(_pingHandler, traceRouteHandlerOptions, _pingRequestModelFactory);

            
        }

        [Test]
        public void OnExecuteToLoopbackAddressItReturnsResponses()
        {
            var responses = _unit.Execute(Defaults.LoopbackAddress, _cancellationToken).Result;

            Assert.That(actual: responses, Is.Not.Empty);

            ShowResults(responses);
        }


        [Test]
        public void OnExecuteToWorldAddressItReturnsResponses()
        {
            var responses = _unit.Execute(TestConditions.WorldAddresses.Last(), _cancellationToken).Result;

            Assert.That(actual: responses, Is.Not.Empty);

            // show responses first because 'sometimes' there are incorrect Ttl values and we need to work out why?
            ShowResults(responses);

            var incorrectTtlValues = responses
                .Where(x => x.Value.Response?.Status == System.Net.NetworkInformation.IPStatus.Success)
                .Where(z => z.Value.Response?.Options?.Ttl + z.Value.Ttl + 1 != Defaults.Ttl);
            
            Assert.That(!incorrectTtlValues.Any());

            incorrectTtlValues = responses
                .Where(x => x.Value.Response?.Status != System.Net.NetworkInformation.IPStatus.Success)
                .Where(z => (z.Value.Response?.Options?.Ttl?? Defaults.Ttl) != Defaults.Ttl);

            Assert.That(!incorrectTtlValues.Any());
          
        }
    }
}