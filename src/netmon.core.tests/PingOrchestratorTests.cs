using netmon.core.Configuration;
using netmon.core.Data;
using netmon.core.Handlers;
using netmon.core.Models;
using netmon.core.Orchestrators;
using Newtonsoft.Json;
using System.Net;

namespace netmon.core.tests
{
    public class PingOrchestratorTests : TestBase<PingOrchestrator>
    {
        private IPingHandler _pingHandler;
        private PingHandlerOptions _pingHandlerOptions;
        private IPingRequestModelFactory _pingRequestModelFactory;
        private PingOrchestratorOptions _pingOrchestratorOptions;

        [SetUp]
        public override void Setup()
        {
            base.Setup();
            // unit setup
            _pingRequestModelFactory = new PingRequestModelFactory();
            _pingHandlerOptions = new PingHandlerOptions();
            _pingHandler = new PingHandler(_pingHandlerOptions);
            _pingOrchestratorOptions = new PingOrchestratorOptions() { MillisecondsBetweenPings = 1000 };
            _unit = new PingOrchestrator(_pingHandler, _pingRequestModelFactory, _pingOrchestratorOptions);
        }

        [Test]
        public void OnExecuteWithDefaltLoopbackRequestFor2SecondsItSucceeeds()
        {
            var duration = new TimeSpan(0, 0, 2);
            var request = new IPAddress[] { Defaults.LoopbackAddress };
            var responses = _unit.PingUntil(request, duration, _cancellationToken).Result;
            Assert.That(responses, Has.Count.EqualTo(request.Length * duration.Seconds), "The test returned the wrong number of results");
            Assert.That(responses.Where(x => x.Value.Request.Address is null).Count, Is.EqualTo(0), "One or more null address were returned");

            ShowResults(responses);
        }

        [Test]
        public void OnExecuteWithDefaltLoopbackRequestFor3SecondsItSucceeeds()
        {
            var duration = new TimeSpan(0, 0, 3);
            var request = new IPAddress[] { Defaults.LoopbackAddress };
            var responses = _unit.PingUntil(request, duration, _cancellationToken).Result;
            Assert.That(responses, Has.Count.EqualTo(request.Length * duration.Seconds), "The test returned the wrong number of results");
            Assert.That(responses.Where(x => x.Value.Request.Address is null).Count, Is.EqualTo(0), "One or more null address were returned");
            ShowResults(responses);
        }

        [Test]
        public void OnExecuteWithComplexRequestkRequestFor4SecondsItSucceeeds()
        {
            var duration = new TimeSpan(0, 0, 1);
            var request = new List<IPAddress>();
            request.AddRange(TestConditions.LocalAddresses);
            request.AddRange(TestConditions.WorldAddresses);

            var responses = _unit.PingUntil(request.ToArray(), duration, _cancellationToken).Result;

            Assert.That(responses, Has.Count.EqualTo(request.Count * duration.Seconds), "The test returned the wrong number of results");
            ShowResults(responses);
        }
    }
}