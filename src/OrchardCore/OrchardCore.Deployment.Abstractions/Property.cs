using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;

namespace OrchardCore.Deployment
{
    public enum PropertyHandler
    {
        UserSupplied,
        Encrypted,
        PlainText,
        Ignored
    }

    public class Property
    {
        [JsonConverter(typeof(StringEnumConverter))]
        public PropertyHandler Handler { get; set; }

        /// <summary>
        /// This takes a string value to remain compatable with IConfiguration keys
        /// Array values should be comma seperated.
        /// </summary>

        public string Value { get; set; }

    }
}
