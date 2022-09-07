using netmon.domain.Data;
using netmon.domain.Messaging;
using netmon.domain.Models;
using System.Net;
using System.Net.NetworkInformation;

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

        /// <summary>
        /// Return a pair of matched test <see cref="PingResponses"/> to simualte target addresses as all being hop 1 addresses. will addd default monitoring address if no addresses sepcified as per the code behaviour.
        /// </summary>
        /// <param name="testAddresses">The addreses to simulate</param>
        /// <returns>matched pair of test <see cref="PingResponses"/></returns>

        public static PingResponseModels PrepeareTestData(List<IPAddress> testAddresses)
        {
            var responsesFromTraceRoute = new PingResponseModels();

            if (testAddresses.Count == 0)
            {
                testAddresses.Add(Defaults.DefaultMonitoringDestination);
            }
            foreach (var address in testAddresses)
            {
                PingReplyModel pingReply = new()
                {
                    Address = address,
                    Buffer = Array.Empty<byte>(),
                    Options = new PingOptions(),
                    RoundtripTime = 1,
                    Status = IPStatus.Success
                }; ;

                responsesFromTraceRoute.TryAdd(
                        new Tuple<DateTimeOffset, IPAddress>(DateTimeOffset.UtcNow, address),
                        new PingResponseModel()
                        {
                            Request = new PingRequestModel() { Address = address }
                        ,
                            Response = pingReply
                        }); ;

            }

            return responsesFromTraceRoute;
        }
    }
}