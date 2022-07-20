using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net;

namespace netmon.core.Data
{
    /// <summary>
    /// Serialisation converter for classes dependent on <see cref="IPEndPoint"/>.
    /// </summary>
    public class IPEndPointConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return (objectType == typeof(IPEndPoint));
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            IPEndPoint ep = (IPEndPoint)value;
            JObject jo = new JObject();
            jo.Add("address", JToken.FromObject(ep.Address, serializer));
            jo.Add("port", ep.Port);
            jo.WriteTo(writer);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            JObject jo = JObject.Load(reader);
            IPAddress address = jo["address"].ToObject<IPAddress>(serializer);
            int port = (int)jo["port"];
            return new IPEndPoint(address, port);
        }
    }
}