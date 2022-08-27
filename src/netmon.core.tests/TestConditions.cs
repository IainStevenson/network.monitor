using System.Net;

namespace netmon.domain.tests
{
    public static class TestConditions
    {
       
        public static IPAddress[] LocalAddresses = new IPAddress[] {
            IPAddress.Parse("192.168.0.1"),
            IPAddress.Parse("172.16.0.1"),
            IPAddress.Parse("10.0.0.1") };
        public static IPAddress[] WorldAddresses = new IPAddress[]  {
                IPAddress.Parse("172.26.19.5"),
                IPAddress.Parse("172.26.24.142"),
                IPAddress.Parse("172.26.24.93"),
                IPAddress.Parse("172.26.3.146"),
                IPAddress.Parse("185.153.237.154"),
                IPAddress.Parse("185.153.237.155"),
                IPAddress.Parse("216.239.48.217"),
                IPAddress.Parse("142.251.52.145"),
                IPAddress.Parse("8.8.8.8")
            };
    }
}