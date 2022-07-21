﻿using netmon.core.Configuration;
using netmon.core.Data;
using netmon.core.Handlers;
using netmon.core.Models;
using netmon.core.Orchestrators;
using Newtonsoft.Json;

namespace netmon.core.tests
{
    public class MonitorOrchestratorTests : TestBase<MonitorOrchestrator>
    {
        private MonitorOptions _monitorOptions;
        private MonitorRequestModel _monitorModel;
        private TraceRouteOrchestrator _traceRouteOrchestrator;
        private PingOrchestrator _pingOrchestrator;
        private IHostAddressTypeHandler _hostAddressTypeHandler;
        private IPingHandler _pingHandler;
        private TraceRouteOrchestratorOptions _traceRouteOrchestratorOptions;
        private IPingRequestModelFactory _pingRequestModelFactory;
        private PingHandlerOptions _pingOptions;
        private PingOrchestratorOptions _pingOrchestratorOptions;


        [SetUp]
        public void Setup()
        {
            // unit setup - need to get more interfaces going and uses mocking.
            _pingOptions = new PingHandlerOptions();
            _traceRouteOrchestratorOptions = new TraceRouteOrchestratorOptions();
            _pingRequestModelFactory = new PingRequestModelFactory();
            _hostAddressTypeHandler = new HostAddressTypeHandler();
            _pingHandler = new PingHandler(_pingOptions);
            _traceRouteOrchestrator = new TraceRouteOrchestrator(_pingHandler, _traceRouteOrchestratorOptions, _pingRequestModelFactory);
            _pingOrchestratorOptions = new PingOrchestratorOptions() { MillisecondsBetweenPings = 1000 };// faster for testing
            _pingOrchestrator = new PingOrchestrator(_pingHandler, _pingRequestModelFactory, _pingOrchestratorOptions);
            _monitorModel = new MonitorRequestModel();
            _monitorOptions = new MonitorOptions();
            _unit = new MonitorOrchestrator(_traceRouteOrchestrator, _pingOrchestrator, _monitorOptions, _hostAddressTypeHandler);

        }



        [Test]
        public async Task OnExecuteItAutoConfiguresAndMonitors()
        {
            //var forEver = new TimeSpan(DateTimeOffset.MaxValue.Ticks - DateTimeOffset.UtcNow.Ticks);
            var until = new TimeSpan(0, 0, 2);
            // two seconds is long enough , must keep ratio of until to _pingOrchestratorOptions.MillsecondsBetweenPings as  even seconds to get count
            _monitorModel = new MonitorRequestModel();

            var multiPingMonitorResponses = await _unit.Execute(_monitorModel, until, _cancellationToken);


            Assert.That(actual: _monitorModel.Hosts, Is.Not.Empty);
            Assert.That(actual: _monitorModel.LocalHosts, Is.Not.Empty);
            Assert.IsNotNull(multiPingMonitorResponses);
            Assert.That(actual: multiPingMonitorResponses, Is.Not.Empty);
            var expectedCount = _monitorModel.Hosts.Count *
                    (int)(until.TotalMilliseconds / _pingOrchestratorOptions.MillisecondsBetweenPings);

            Assert.That(actual: multiPingMonitorResponses.Count, Is.EqualTo(expectedCount));

            ShowResults(_monitorModel);
            ShowResults(multiPingMonitorResponses);
        }

        [Test]
        public async Task OnExecuteWithConfigurationItMonitorsSpecifiedHosts()
        {
            //var forEver = new TimeSpan(DateTimeOffset.MaxValue.Ticks - DateTimeOffset.UtcNow.Ticks);
            var until = new TimeSpan(0, 0, 2); // two seconds is long enough

            var monitorJson = File.ReadAllText($@".\MonitorModel.json");
            if (monitorJson != null)
            {
#pragma warning disable CS8601 // Possible null reference assignment. Defended against below
                _monitorModel = JsonConvert.DeserializeObject<MonitorRequestModel>(monitorJson, _settings);
#pragma warning restore CS8601 // Possible null reference assignment. Defended against below

                if (_monitorModel != null)
                {
                    ShowResults(_monitorModel);

                    var responses = await _unit.Execute(_monitorModel, until, _cancellationToken);
                    Assert.That(actual: responses, Is.Not.Empty);

                    ShowResults(responses);
                }
                else
                {
                    Assert.Fail("Failed to deserialise the model.");
                }

            }
            else
            {
                Assert.Fail("Failed to deserialise the model.");
            }
        }



    }
}