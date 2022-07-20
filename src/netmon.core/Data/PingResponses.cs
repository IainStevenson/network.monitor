using netmon.core.Models;
using System.Collections.Concurrent;
using System.Net;

namespace netmon.core.Data
{
    /// <summary>
    /// A time and <see cref="IPAddress "/> keyed dictionary of <see cref="PingRequestModel"/> data from completed ping requests.
    /// </summary>
    public class PingResponses : ConcurrentDictionary<Tuple<DateTimeOffset, IPAddress>, PingResponseModel>
    {
        /// <summary>
        /// Sorts the dictionary items ordered by Hop #, Attempt #, <see cref="DateTimeOffset"/>, <see cref="IPAddress"/> and converts to a sinmple list.
        /// </summary>
        /// <returns>A sorted list of <see cref="List{T}"/> of <see cref="PingResponseModel"/></returns>
        public List<PingResponseModel> AsOrderedList()
        {
            return this
                .OrderBy(a => a.Value.Hop)
                .ThenBy(b=>b.Value.Attempt)
                .ThenBy(c => c.Key.Item1)
                .ThenBy(c => c.Key.Item2)
                .Select( s=> s.Value).ToList();
        }
    }
}