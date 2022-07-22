using netmon.core.Data;
using System.Net;

namespace netmon.core.tests
{
    /// <summary>
    /// Lifted from: https://stackoverflow.com/questions/1499269/how-to-check-if-an-ip-address-is-within-a-particular-subnet
    /// </summary>
    public class IpAddressExtensionsTests
    {
        [Theory]
        [TestCase("192.168.5.85/24", "192.168.5.1")]
        [TestCase("192.168.5.85/24", "192.168.5.254")]
        [TestCase("10.128.240.50/30", "10.128.240.48")]
        [TestCase("10.128.240.50/30", "10.128.240.49")]
        [TestCase("10.128.240.50/30", "10.128.240.50")]
        [TestCase("10.128.240.50/30", "10.128.240.51")]
        [TestCase("192.168.5.85/0", "0.0.0.0")]
        [TestCase("192.168.5.85/0", "255.255.255.255")]
        [TestCase("172.16.0.0/12", "172.26.19.5")]
        [Category("Unit")]
        public void IpV4SubnetMaskMatchesValidIpAddress(string netMask, string ipAddress)
        {
            IPAddress ipAddressObj = IPAddress.Parse(ipAddress);
            Assert.True(ipAddressObj.IsInSubnet(netMask));
        }

        [Theory]
        [TestCase("192.168.5.85/24", "192.168.4.254")]
        [TestCase("192.168.5.85/24", "191.168.5.254")]
        [TestCase("10.128.240.50/30", "10.128.240.47")]
        [TestCase("10.128.240.50/30", "10.128.240.52")]
        [TestCase("10.128.240.50/30", "10.128.239.50")]
        [TestCase("10.128.240.50/30", "10.127.240.51")]
        [Category("Unit")]
        public void IpV4SubnetMaskDoesNotMatchInvalidIpAddress(string netMask, string ipAddress)
        {
            IPAddress ipAddressObj = IPAddress.Parse(ipAddress);
            Assert.False(ipAddressObj.IsInSubnet(netMask));
        }

        [Theory]
        [TestCase("2001:db8:abcd:0012::0/64", "2001:0DB8:ABCD:0012:0000:0000:0000:0000")]
        [TestCase("2001:db8:abcd:0012::0/64", "2001:0DB8:ABCD:0012:FFFF:FFFF:FFFF:FFFF")]
        [TestCase("2001:db8:abcd:0012::0/64", "2001:0DB8:ABCD:0012:0001:0000:0000:0000")]
        [TestCase("2001:db8:abcd:0012::0/64", "2001:0DB8:ABCD:0012:FFFF:FFFF:FFFF:FFF0")]
        [TestCase("2001:db8:abcd:0012::0/128", "2001:0DB8:ABCD:0012:0000:0000:0000:0000")]
        [TestCase("2001:db8:abcd:5678::0/53", "2001:0db8:abcd:5000:0000:0000:0000:0000")]
        [TestCase("2001:db8:abcd:5678::0/53", "2001:0db8:abcd:57ff:ffff:ffff:ffff:ffff")]
        [TestCase("2001:db8:abcd:0012::0/0", "::")]
        [TestCase("2001:db8:abcd:0012::0/0", "ffff:ffff:ffff:ffff:ffff:ffff:ffff:ffff")]
        [Category("Unit")]
        public void IpV6SubnetMaskMatchesValidIpAddress(string netMask, string ipAddress)
        {
            IPAddress ipAddressObj = IPAddress.Parse(ipAddress);
            Assert.True(ipAddressObj.IsInSubnet(netMask));
        }

        [Theory]
        [TestCase("2001:db8:abcd:0012::0/64", "2001:0DB8:ABCD:0011:FFFF:FFFF:FFFF:FFFF")]
        [TestCase("2001:db8:abcd:0012::0/64", "2001:0DB8:ABCD:0013:0000:0000:0000:0000")]
        [TestCase("2001:db8:abcd:0012::0/64", "2001:0DB8:ABCD:0013:0001:0000:0000:0000")]
        [TestCase("2001:db8:abcd:0012::0/64", "2001:0DB8:ABCD:0011:FFFF:FFFF:FFFF:FFF0")]
        [TestCase("2001:db8:abcd:0012::0/128", "2001:0DB8:ABCD:0012:0000:0000:0000:0001")]
        [TestCase("2001:db8:abcd:5678::0/53", "2001:0db8:abcd:4999:0000:0000:0000:0000")]
        [TestCase("2001:db8:abcd:5678::0/53", "2001:0db8:abcd:5800:0000:0000:0000:0000")]
        [Category("Unit")]
        public void IpV6SubnetMaskDoesNotMatchInvalidIpAddress(string netMask, string ipAddress)
        {
            IPAddress ipAddressObj = IPAddress.Parse(ipAddress);
            Assert.False(ipAddressObj.IsInSubnet(netMask));
        }
    }
}