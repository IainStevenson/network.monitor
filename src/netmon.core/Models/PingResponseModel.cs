using Newtonsoft.Json.Converters;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.NetworkInformation;
using System.Text.Json.Serialization;

namespace netmon.core.Models
{
    [ExcludeFromCodeCoverage]
    public class PingResponseModel
    {
        public DateTimeOffset Start { get; internal set; } = DateTimeOffset.MinValue;
        public DateTimeOffset Finish { get; internal set; } = DateTimeOffset.MinValue;
        public TimeSpan Duration { get { return new TimeSpan(Finish.Ticks - Start.Ticks); } }

        public PingReply Response { get; set; }
        public PingRequestModel Request { get; internal set; }
    }
}