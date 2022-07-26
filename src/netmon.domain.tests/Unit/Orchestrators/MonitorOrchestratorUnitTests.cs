﻿using Microsoft.Extensions.Logging;
using netmon.domain.Configuration;
using netmon.domain.Data;
using netmon.domain.Interfaces;
using netmon.domain.Orchestrators;
using NSubstitute;
using System.Net;

namespace netmon.domain.tests.Integration.Orchestrators
{

    public class MonitorOrchestratorUnitTests : TestBase<MonitorOrchestrator>
    {
        private Dictionary<MonitorModes, IMonitorModeOrchestrator> _monitors;
        private ILogger<MonitorOrchestrator> _monitorOrchestratorLogger;
        private IMonitorModeOrchestrator _traceRouteThenPingContinuously;
        private IMonitorModeOrchestrator _traceRouteContinuously;
        private IMonitorModeOrchestrator _pingContinuously;
        private readonly TimeSpan _testUntil = new(0, 0, 2); //would use this for continuous use: var forEver = new TimeSpan(DateTimeOffset.MaxValue.Ticks - DateTimeOffset.UtcNow.Ticks);

        [SetUp]
        public override void Setup()
        {
            base.Setup();

            _traceRouteThenPingContinuously = Substitute.For<IMonitorModeOrchestrator>();
            _traceRouteContinuously = Substitute.For<IMonitorModeOrchestrator>();
            _pingContinuously = Substitute.For<IMonitorModeOrchestrator>();

            _monitors = new ()
            {
                { MonitorModes.TraceRouteThenPingContinuously, _traceRouteThenPingContinuously },
                { MonitorModes.TraceRouteContinuously, _traceRouteContinuously },
                { MonitorModes.PingContinuously, _pingContinuously }
            };

            _monitorOrchestratorLogger = Substitute.For<ILogger<MonitorOrchestrator>>();
            _unit = new MonitorOrchestrator(_monitors, _monitorOrchestratorLogger);
        }


        [TestCase(MonitorModes.TraceRouteThenPingContinuously)]
        [TestCase(MonitorModes.TraceRouteContinuously)]
        [TestCase(MonitorModes.PingContinuously)]
        public void OnExecute_AccordingToMode_ItShouldCallTheCorrectOrchestrator(MonitorModes mode)
        {
            //  Arrange  - done in setup

            // Act
            _unit.Execute(mode, new List<IPAddress>() { Defaults.DefaultMonitoringDestination }, _testUntil, _cancellationToken).Wait();

            // Assert
            switch (mode)
            {
                case MonitorModes.TraceRouteThenPingContinuously:
                    _traceRouteThenPingContinuously.Received(1).Execute(Arg.Any<List<IPAddress>>(), _testUntil, _cancellationToken);
                    _traceRouteContinuously.Received(0).Execute(Arg.Any<List<IPAddress>>(), _testUntil, _cancellationToken);
                    _pingContinuously.Received(0).Execute(Arg.Any<List<IPAddress>>(), _testUntil, _cancellationToken);
                    break;
                case MonitorModes.TraceRouteContinuously:
                    _traceRouteThenPingContinuously.Received(0).Execute(Arg.Any<List<IPAddress>>(), _testUntil, _cancellationToken);
                    _traceRouteContinuously.Received(1).Execute(Arg.Any<List<IPAddress>>(), _testUntil, _cancellationToken);
                    _pingContinuously.Received(0).Execute(Arg.Any<List<IPAddress>>(), _testUntil, _cancellationToken);
                    break;
                case MonitorModes.PingContinuously:
                    _traceRouteThenPingContinuously.Received(0).Execute(Arg.Any<List<IPAddress>>(), _testUntil, _cancellationToken);
                    _traceRouteContinuously.Received(0).Execute(Arg.Any<List<IPAddress>>(), _testUntil, _cancellationToken);
                    _pingContinuously.Received(1).Execute(Arg.Any<List<IPAddress>>(), _testUntil, _cancellationToken);
                    break;
            }
        }
    }
}

