﻿using netmon.core.Data;
using netmon.core.Handlers;
using netmon.core.Models;
using System.Net;

namespace netmon.core.tests
{
    public class HostAddressTypeHandlerTests
    {
        private HostAddressTypeHandler _unit;

        [SetUp]
        public void Sestup()
        {
            _unit = new HostAddressTypeHandler();
        }


        [Test]
        [TestCase("192.168.0.1", HostTypes.Private)]
        [TestCase("172.16.0.1", HostTypes.Private)]
        [TestCase("10.0.0.1", HostTypes.Private)]
        [TestCase("172.27.83.1", HostTypes.Private)]
        [TestCase("195.68.0.2", HostTypes.Public)]
        [TestCase("216.239.48.217",HostTypes.Public)]
        [TestCase("8.8.8.8", HostTypes.Public)]
        public void OnGetPrivateHostTypeItReturnsTheExepctedType(string address, HostTypes expectation)
        {
            Assert.That(_unit.GetPrivateHostType(IPAddress.Parse(address)), Is.EqualTo(expectation));
        }

        [Test]
        [TestCase("192.168.0.1", HostTypes.Private)]
        [TestCase("172.16.0.1", HostTypes.Private)]
        [TestCase("10.0.0.1", HostTypes.Private)]
        [TestCase("172.27.83.1", HostTypes.Private)]
        [TestCase("195.68.0.2", HostTypes.Public)]
        [TestCase("216.239.48.217", HostTypes.Public)]
        [TestCase("8.8.8.8", HostTypes.Public)]
        public void OnGetPublicHostTypeItReturnsTheExepctedType(string address, HostTypes expectation)
        {
            Assert.That(_unit.GetPublicHostType(IPAddress.Parse(address), TestConditions.WorldAddresses), Is.EqualTo(expectation));
        }

    }
}