using netmon.core.Models;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Net;

namespace netmon.core.Messaging
{
    /// <summary>
    /// A time and <see cref="IPAddress "/> keyed dictionary of <see cref="PingRequestModel"/> data from completed ping requests.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class PingResponses : ConcurrentDictionary<Tuple<DateTimeOffset, IPAddress>, PingResponseModel>
    {
        /// <summary>
        /// Sorts the dictionary items ordered by Hop #, Attempt #, <see cref="DateTimeOffset"/>, <see cref="IPAddress"/> and converts to a sinmple list.
        /// </summary>
        /// <returns>A sorted list of <see cref="List{T}"/> of <see cref="PingResponseModel"/></returns>
        public List<PingResponseModel> AsOrderedList()
        {
            return this
                .OrderBy(a => a.Value.Start) // order of execution
                .Select(s => s.Value).ToList();
        }
    }
}