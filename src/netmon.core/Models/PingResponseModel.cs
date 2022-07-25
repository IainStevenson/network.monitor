using System.Diagnostics.CodeAnalysis;

namespace netmon.core.Models
{

    [ExcludeFromCodeCoverage]
    public class PingResponseModel
    {
        /// <summary>
        /// When it started
        /// </summary>
        public DateTimeOffset Start { get; internal set; } = DateTimeOffset.MinValue;
        /// <summary>
        /// When it finished
        /// </summary>
        public DateTimeOffset Finish { get; internal set; } = DateTimeOffset.MinValue;
        /// <summary>
        /// How long it took.
        /// </summary>
        public TimeSpan Duration { get { return new TimeSpan(Finish.Ticks - Start.Ticks); } }
        /// <summary>
        /// What was achieved.
        /// </summary>
        public PingReplyModel? Response { get; set; }
        /// <summary>
        /// Waht was asked for.
        /// </summary>
        public PingRequestModel Request { get; set; } = new PingRequestModel();
        /// <summary>
        /// The hop number for a Trace route ping
        /// </summary>
        public int? Hop { get; set; }
        /// <summary>
        /// The attemtp number for a trace route ping.
        /// </summary>
        public int? Attempt { get; set; }
        /// <summary>
        /// The maximum attempts number foa trace route operation
        /// </summary>
        public int? MaxAttempts { get; set; }
        public Exception? Exception { get; set; }
    }
}