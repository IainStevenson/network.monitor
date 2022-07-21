using netmon.core.Data;
using System.Diagnostics.CodeAnalysis;
using System.Net.NetworkInformation;

namespace netmon.core.Models
{
    [ExcludeFromCodeCoverage]
    public class PingResponseModel
    {
        public DateTimeOffset Start { get; internal set; } = DateTimeOffset.MinValue;
        public DateTimeOffset Finish { get; internal set; } = DateTimeOffset.MinValue;
        public TimeSpan Duration { get { return new TimeSpan(Finish.Ticks - Start.Ticks); } }

        public PingReply? Response { get; set; }
        public PingRequestModel Request { get; set; } = new PingRequestModel();
        public int Hop { get; set; } = 1;

        public int Attempt { get; set; } = 1;
        public int MaxAttempts { get; set; } = 3;
        public int Ttl { get; set; } = 0;

    }
}