using DotLiquid;
using Newtonsoft.Json.Linq;

namespace Orchard.Liquid
{
    /// <summary>
    /// Proxy for types not derived from DropBase
    /// </summary>
    public class JTokenDrop : Drop
    {
        private readonly JToken _proxiedObject;

        public JTokenDrop(JToken obj)
        {
            _proxiedObject = obj;
        }

        public override object BeforeMethod(string method)
        {
            if (method == "ToString")
            {
                return _proxiedObject.ToString();
            }

            var value = _proxiedObject[method];

            if (value == null)
            {
                return null;
            }

            if (value is JObject)
            {
                return new JTokenDrop(value);
            }

            if (value is JArray)
            {
                return new JArrayDrop((JArray)value);
            }

            if (value is JValue)
            {
                return ((JValue)value).Value;
            }

            return null;
        }
    }
}
