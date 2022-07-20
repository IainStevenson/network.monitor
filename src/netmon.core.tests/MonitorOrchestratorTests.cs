using netmon.core.Configuration;
using netmon.core.Data;
using netmon.core.Handlers;
using netmon.core.Models;
using netmon.core.Orchestrators;
using Newtonsoft.Json;

namespace netmon.core.tests
{
    public class MonitorOrchestratorTests : TestBase<MonitorOrchestrator>
    {
        private MonitorOrchestrator _unit;
        private MonitorOptions _monitorOptions;
        private MonitorRequestModel _monitorModel;
        private TraceRouteOrchestrator _traceRouteOrchestrator;
        private PingOrchestrator _pingOrchestrator;
        private IHostAddressTypeHandler _hostAddressTypeHandler;
        private IPingHandler _pingHandler;
        private TraceRouteOrchestratorOptions _traceRouteOrchestratorOptions;
        private IPingRequestModelFactory _pingRequestModelFactory;
        private PingHandlerOptions _pingOptions;
       

        [SetUp]
        public void Setup()
        {
            // unit setup - need to get more interfaces going and uses mocking.
            _pingOptions = new PingHandlerOptions();
            _traceRouteOrchestratorOptions = new TraceRouteOrchestratorOptions();
            _pingRequestModelFactory = new PingRequestModelFactory();
            _hostAddressTypeHandler = new  HostAddressTypeHandler();
            _pingHandler = new  PingHandler(_pingOptions);
            _traceRouteOrchestrator = new TraceRouteOrchestrator(_pingHandler, _traceRouteOrchestratorOptions, _pingRequestModelFactory);
            _pingOrchestrator = new PingOrchestrator(_pingHandler, _pingRequestModelFactory);
            _monitorModel = new MonitorRequestModel();
            _monitorOptions = new MonitorOptions();
            _unit = new MonitorOrchestrator(_traceRouteOrchestrator, _pingOrchestrator, _monitorOptions, _hostAddressTypeHandler);          

        }

        

        [Test]
        public async Task  OnExecuteItAutoConfiguresMonitorsAndDoesNotThrowException()
        {
            var forEver = new TimeSpan(DateTimeOffset.MaxValue.Ticks - DateTimeOffset.UtcNow.Ticks);
            var until = new TimeSpan(0,0,2); // two seconds is long enough
            

            var responses  = _unit.Execute(_monitorModel, until, _cancellationToken).Result;


            Assert.That(_monitorModel.LocalHosts.Count, Is.GreaterThanOrEqualTo(1));
            Assert.That(_monitorModel.Hosts.Count, Is.GreaterThan(0));
            Assert.That(responses.Count, Is.GreaterThan(0));

            ShowResults(_monitorModel);
            ShowResults(responses);
        }


        [Test]
        public async Task OnExecuteWithConfigurationItMonitorsSpecifiedHosts()
        {
            var forEver = new TimeSpan(DateTimeOffset.MaxValue.Ticks - DateTimeOffset.UtcNow.Ticks);
            var until = new TimeSpan(0, 0, 2); // two seconds is long enough

            var monitorJson = File.ReadAllText($@".\MonitorModel.json");
            _monitorModel = JsonConvert.DeserializeObject<MonitorRequestModel>(monitorJson, _settings);

            var responses = _unit.Execute(_monitorModel, until, _cancellationToken).Result;



            ShowResults(_monitorModel);
            ShowResults(responses);
        }



    }
}