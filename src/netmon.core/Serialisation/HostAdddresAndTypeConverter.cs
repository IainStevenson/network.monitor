using netmon.core.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net;

namespace netmon.core.Serialisation
{
    public class HostAdddresAndTypeConverter : JsonConverter
    {
        /// <summary>
        /// Provide support for a dictionary of IPAddress and HostTypes
        /// </summary>
        /// <param name="objectType"></param>
        /// <returns></returns>
        public override bool CanConvert(Type objectType)
        {
            if (objectType == typeof(Dictionary<IPAddress, HostTypes>)) return true;
            return false;
        }
        /// <summary>
        /// Deserialise: Read the json back to an object
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="objectType"></param>
        /// <param name="existingValue"></param>
        /// <param name="serializer"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>

#pragma warning disable CS8602 // Dereference of a possibly null reference.
        public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
        {
            var items = JToken.Load(reader)
                 .Select(item => new KeyValuePair<string, string>(
                         item["address"].ToString(),
                         item["hostType"].ToString())
                 ).ToList().ToDictionary(t => IPAddress.Parse(t.Key), t => (HostTypes)Enum.Parse(typeof(HostTypes), t.Value));
            return items;
        }
#pragma warning restore CS8602 // Dereference of a possibly null reference.

        /// <summary>
        /// Serialise: Write the object to Json.
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="value"></param>
        /// <param name="serializer"></param>
        /// <exception cref="NotImplementedException"></exception>
        public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
        {
            JToken.FromObject((from n in value as Dictionary<IPAddress, HostTypes>
                               select
                               new { address = n.Key.ToString(), hostType = n.Value.ToString() }
                               ).ToList()).WriteTo(writer);
        }
    }
}