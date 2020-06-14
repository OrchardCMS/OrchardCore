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
        public string Value { get; set; }

    }
}
