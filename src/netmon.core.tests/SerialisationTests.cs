﻿using netmon.core.Data;
using netmon.core.Models;
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


#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
            IPAddress actual = JsonConvert.DeserializeObject<IPAddress>(json, _settings);
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.

            Assert.That(actual, Is.Not.Null);
            Assert.That(actual, Is.TypeOf<IPAddress>());
            Assert.That(actual.ToString(), Is.EqualTo(addressString));

        }

        [Test]
        [Category("Integration")]
        public void OnJsonConvertOfMonitorRequestItSerialisesBiDirectionally()
        {
            MonitorRequestModel request = new()
            {
                Destination = IPAddress.Parse("8.8.8.8")
            };
            request.LocalHosts.Add(IPAddress.Parse("192.168.0.1"));
            request.LocalHosts.Add(IPAddress.Parse("192.168.1.1"));
            request.LocalHosts.Add(IPAddress.Parse("172.16.0.1"));
            foreach (var host in TestConditions.WorldAddresses)
            {
                request.Hosts.Add(host, HostTypes.Public);
            }

            var json = JsonConvert.SerializeObject(request, _settings);
            Assert.That(json, Is.Not.Null);
            Assert.That(json, Is.TypeOf<string>());


#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
            MonitorRequestModel actual = JsonConvert.DeserializeObject<MonitorRequestModel>(json, _settings);
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.

            Assert.That(actual, Is.Not.Null);
            Assert.That(actual, Is.TypeOf<MonitorRequestModel>());
            Assert.That(actual.Destination, Is.EqualTo(request.Destination));

        }

    }
}