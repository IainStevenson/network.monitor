using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net;

namespace netmon.core.Data
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
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            // convert an ipaddress represented as a string into an IPAddress object and return it to the caller
            if (objectType == typeof(IPAddress))
            {
                return IPAddress.Parse(JToken.Load(reader).ToString());
            }

            // convert an array of IPAddresses represented as strings into a List<IPAddress> object and return it to the caller
            if (objectType == typeof(List<IPAddress>))
            {
                return JToken.Load(reader).Select(address => IPAddress.Parse((string)address)).ToList();
            }

            throw new NotImplementedException();
        }

        /// <summary>
        /// Serialise: Write the object to Json.
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="value"></param>
        /// <param name="serializer"></param>
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (value.GetType() == typeof(IPAddress))
            {
                JToken.FromObject(value.ToString()).WriteTo(writer);
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