using Newtonsoft.Json;
using System.Net;

namespace netmon.core.Data
{ 
    /// <summary>
    /// Serialisation converter for classes dependent on <see cref="IPAddress"/>.
    /// </summary>
    public class IPAddressConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return (objectType == typeof(IPAddress));
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            writer.WriteValue(value.ToString());
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            return IPAddress.Parse((string)reader.Value);
        }

    }
}