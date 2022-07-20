using netmon.core.Data;
using Newtonsoft.Json;

namespace netmon.core.tests
{
    public abstract class TestBase<T>
    {
        protected T _unit;
        protected CancellationTokenSource _cancellationTokenSource;
        protected CancellationToken _cancellationToken;
        protected JsonSerializerSettings _settings;

        public TestBase()
        {
            // control setup
            _cancellationTokenSource = new CancellationTokenSource();
            _cancellationToken = _cancellationTokenSource.Token;

            // output setup
            _settings = new JsonSerializerSettings();
            _settings.Converters.Add(new IPAddressConverter());
            //_settings.Converters.Add(new IPEndPointConverter());
            _settings.Converters.Add(new Newtonsoft.Json.Converters.StringEnumConverter());
            _settings.Converters.Add(new HostAdddresAndTypeConverter());
            _settings.Formatting = Formatting.Indented;
        }

        [TearDown]
        public void Teardown()
        {
            var disposable = _unit as IDisposable;
            disposable?.Dispose();
        }
        protected void ShowResults<T>(T results)
        {
            var output = results as PingResponses;

            if (output != null)
            {
                TestContext.Out.WriteLine(JsonConvert.SerializeObject(output.AsOrderedList(), _settings));
            }
            else
            {
                TestContext.Out.WriteLine(JsonConvert.SerializeObject(results, _settings));
            }
        }
    }
}