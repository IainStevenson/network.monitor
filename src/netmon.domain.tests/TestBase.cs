using netmon.domain.Messaging;
using netmon.domain.Serialisation;
using Newtonsoft.Json;

namespace netmon.domain.tests
{


    [TestFixture]
    public abstract class TestBase<T>
    {
        protected T _unit;
        protected CancellationTokenSource _cancellationTokenSource;
        protected CancellationToken _cancellationToken;
        protected JsonSerializerSettings _settings;
        
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public TestBase()
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        {
            

            // output setup
            _settings = new JsonSerializerSettings();
            _settings.Converters.Add(new IPAddressConverter());
            _settings.Converters.Add(new Newtonsoft.Json.Converters.StringEnumConverter());
            _settings.Converters.Add(new HostAdddresAndTypeConverter());
            _settings.Formatting = Formatting.Indented;
        }

        [SetUp]
        public virtual void Setup()
        {
            // control setup
            _cancellationTokenSource = new CancellationTokenSource();
            _cancellationToken = _cancellationTokenSource.Token;
        }


        [TearDown]
        public void Teardown()
        {
            (_unit as IDisposable)?.Dispose();
        }

        /// <summary>
        /// Display as serialised Json, the responses in the test console.
        /// </summary>
        /// <typeparam name="TData">The type of data to display</typeparam>
        /// <param name="results">the data.</param>
        protected void ShowResults<TData>(TData results)
        {
            if (results is PingResponseModels output)
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