using netmon.core.Models;
using netmon.core.Serialisation;
using Newtonsoft.Json;
using System.Net;

namespace netmon.core.tests.Unit
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
            Assert.That(actual.Skip(0).First(), Is.EqualTo(IPAddress.Parse("8.8.8.8")));
            Assert.That(actual.Skip(1).First(), Is.EqualTo(IPAddress.Parse("192.168.0.1")));
            Assert.That(actual.Skip(2).First(), Is.EqualTo(IPAddress.Parse("192.168.1.1")));
            Assert.That(actual.Skip(3).First(), Is.EqualTo(IPAddress.Parse("172.16.0.1")));

        }
        [Test]
        [TestCase("192.168.0.1", HostTypes.Private)]
        [TestCase("172.16.0.1", HostTypes.Private)]
        [TestCase("10.0.0.1", HostTypes.Private)]
        [TestCase("172.27.83.1", HostTypes.Private)]
        [TestCase("195.68.0.2", HostTypes.Public)]
        [TestCase("216.239.48.217", HostTypes.Public)]
        [TestCase("8.8.8.8", HostTypes.Public)]
        [Category("Unit")]
        public void OnJsonConvertOfIPAddressAndHostTypeTupleItSerialisesBothWays(string address, HostTypes hostType)
        {
            // serialize then deserialize the combined type.


            var data = new Tuple<IPAddress, HostTypes>(IPAddress.Parse(address), hostType);
            var json = JsonConvert.SerializeObject(data, _settings);
            Assert.That(json, Is.Not.Null);
            Assert.That(json, Is.TypeOf<string>());


            Tuple<IPAddress, HostTypes>? actual = JsonConvert.DeserializeObject<Tuple<IPAddress, HostTypes>>(json, _settings);

            Assert.That(actual, Is.Not.Null);
            Assert.That(actual, Is.TypeOf<Tuple<IPAddress, HostTypes>>());
            Assert.That(actual.Item1.ToString(), Is.EqualTo(address));
            Assert.That(actual.Item2, Is.EqualTo(hostType));
        }

        [Test]
        [TestCase("192.168.0.1", HostTypes.Private)]
        [TestCase("172.16.0.1", HostTypes.Private)]
        [TestCase("10.0.0.1", HostTypes.Private)]
        [TestCase("172.27.83.1", HostTypes.Private)]
        [TestCase("195.68.0.2", HostTypes.Public)]
        [TestCase("216.239.48.217", HostTypes.Public)]
        [TestCase("8.8.8.8", HostTypes.Public)]
        [Category("Unit")]
        public void OnJsonConvertOfIPAddressAndHostTypeTupleDictionaryItSerialisesBothWays(string address, HostTypes hostType)
        {
            // serialize then deserialize the combined type.


            var data = new Dictionary<IPAddress, HostTypes>() {
                {IPAddress.Parse(address), hostType }
            };
            var json = JsonConvert.SerializeObject(data, _settings);
            Assert.That(json, Is.Not.Null);
            Assert.That(json, Is.TypeOf<string>());


            Dictionary<IPAddress, HostTypes>? actual = JsonConvert.DeserializeObject<Dictionary<IPAddress, HostTypes>>(json, _settings);

            Assert.That(actual, Is.Not.Null);
            Assert.That(actual, Is.TypeOf<Dictionary<IPAddress, HostTypes>>());
            Assert.That(actual, Has.Count.EqualTo(1));

        }

    }
}