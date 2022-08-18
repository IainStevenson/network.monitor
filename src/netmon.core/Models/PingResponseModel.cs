using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace netmon.core.Models
{

    [ExcludeFromCodeCoverage]
    public class PingResponseModel: StorageBase
    {

        /// <summary>
        /// When it started
        /// </summary>
        [BsonRepresentation(BsonType.String)] public DateTimeOffset Start { get;  set; } = DateTimeOffset.MinValue;
        /// <summary>
        /// When it finished
        /// </summary>
        [BsonRepresentation(BsonType.String)] public DateTimeOffset Finish { get;  set; } = DateTimeOffset.MinValue;
        /// <summary>
        /// How long it took.
        /// </summary>
        /// 
        [JsonIgnore]
        [BsonIgnore]
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