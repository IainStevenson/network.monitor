using netmon.core.Models;
using System.Collections.Concurrent;
using System.Net;

namespace netmon.core.Data
{
    public class PingResponses : ConcurrentDictionary<Tuple<DateTimeOffset, IPAddress>, PingResponseModel>
    {

    }
}