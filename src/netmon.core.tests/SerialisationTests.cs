using netmon.core.Serialisation;
using Newtonsoft.Json;
using System.Net;

namespace netmon.core.tests
{
    public class SerialisationTests
    {
        protected JsonSerializerSettings _settings;
        [SetUp]
        public void Setup()
        {

            _settings = new JsonSerializerSettings();
            _settings.Converters.Add(new IPAddressConverter());
            _settings.Converters.Add(new Newtonsoft.Json.Converters.StringEnumConverter());
            _settings.Converters.Add(new HostAdddresAndTypeConverter());
            _settings.Formatting = Formatting.Indented;
        }

        [Test]
        [Category("Integration")]
        public void OnJsonConvertOfIPAddressItSerialisesBiDirectionally()
        {
            var addressString = "127.0.0.1";
            IPAddress address = IPAddress.Parse(addressString);
            var json = JsonConvert.SerializeObject(address, _settings);
            Assert.That(json, Is.Not.Null);
            Assert.That(json, Is.TypeOf<string>());


            IPAddress? actual = JsonConvert.DeserializeObject<IPAddress>(json, _settings);

            Assert.That(actual, Is.Not.Null);
            Assert.That(actual, Is.TypeOf<IPAddress>());
            Assert.That(actual.ToString(), Is.EqualTo(addressString));

        }

        [Test]
        [Category("Integration")]
        public void OnJsonConvertOfMonitorRequestItSerialisesBiDirectionally()
        {
            List<IPAddress> request = new()
            {
                IPAddress.Parse("8.8.8.8")            ,
                IPAddress.Parse("192.168.0.1"),
                IPAddress.Parse("192.168.1.1"),
                IPAddress.Parse("172.16.0.1")
            };

            var json = JsonConvert.SerializeObject(request, _settings);
            Assert.That(json, Is.Not.Null);
            Assert.That(json, Is.TypeOf<string>());

            var actual = JsonConvert.DeserializeObject<List<IPAddress>>(json, _settings);

            Assert.That(actual, Is.Not.Null);
            Assert.That(actual, Is.TypeOf<List<IPAddress>>());
            Assert.That(actual.First(), Is.EqualTo(IPAddress.Parse("8.8.8.8")));

        }

    }
}