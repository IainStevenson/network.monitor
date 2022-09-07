namespace netmon.domain.Models
{
    public class TraceInfoModel
    {
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
    }
}