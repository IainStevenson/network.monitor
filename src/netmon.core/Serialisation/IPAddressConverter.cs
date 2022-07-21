using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net;

namespace netmon.core.Serialisation
{
    /// <summary>
    /// Class lifted from: https://pingfu.net/how-to-serialise-ipaddress-ipendpoint
    /// </summary>
    public class IPAddressConverter : JsonConverter
    {
        /// <summary>
        /// Provide support for IPAddress and Lists of IP Addresses.
        /// </summary>
        /// <param name="objectType"></param>
        /// <returns></returns>
        public override bool CanConvert(Type objectType)
        {
            if (objectType == typeof(IPAddress)) return true;
            if (objectType == typeof(List<IPAddress>)) return true;
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
#pragma warning disable CS8765 // Nullability of type of parameter doesn't match overridden member (possibly because of nullability attributes).
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
#pragma warning restore CS8765 // Nullability of type of parameter doesn't match overridden member (possibly because of nullability attributes).
        {
            // convert an ipaddress represented as a string into an IPAddress object and return it to the caller
            if (objectType == typeof(IPAddress))
            {
                return IPAddress.Parse(JToken.Load(reader).ToString());
            }

            // convert an array of IPAddresses represented as strings into a List<IPAddress> object and return it to the caller
            if (objectType == typeof(List<IPAddress>))
            {
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
#pragma warning disable CS8604 // Possible null reference argument.
                return JToken.Load(reader).Select(address => IPAddress.Parse((string)address)).ToList();
#pragma warning restore CS8604 // Possible null reference argument.
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
            }

            throw new NotImplementedException();
        }

        /// <summary>
        /// Serialise: Write the object to Json.
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="value"></param>
        /// <param name="serializer"></param>
#pragma warning disable CS8765 // Nullability of type of parameter doesn't match overridden member (possibly because of nullability attributes).
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
#pragma warning restore CS8765 // Nullability of type of parameter doesn't match overridden member (possibly because of nullability attributes).
        {
            if (value.GetType() == typeof(IPAddress))
            {
#pragma warning disable CS8604 // Possible null reference argument.
                JToken.FromObject(value.ToString()).WriteTo(writer);
#pragma warning restore CS8604 // Possible null reference argument.
                return;
            }

            if (value.GetType() == typeof(List<IPAddress>))
            {
                JToken.FromObject((from n in (List<IPAddress>)value select n.ToString()).ToList()).WriteTo(writer);
                return;
            }

            throw new NotImplementedException();
        }
    }
}