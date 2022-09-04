using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace netmon.domain.Models
{

    [ExcludeFromCodeCoverage]
    public class PingResponseModel : StorageBase
    {

        /// <summary>
        /// When it started
        /// </summary>
        [BsonRepresentation(BsonType.String)] public DateTimeOffset Start { get; set; } = DateTimeOffset.MinValue;
        /// <summary>
        /// When it finished
        /// </summary>
        [BsonRepresentation(BsonType.String)] public DateTimeOffset Finish { get; set; } = DateTimeOffset.MinValue;
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

        public TraceInfoModel? TraceInfo { get; set; } 

        public Exception? Exception { get; set; }
    }
}