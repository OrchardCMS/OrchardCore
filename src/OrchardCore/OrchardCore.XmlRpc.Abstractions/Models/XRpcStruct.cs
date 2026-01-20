using System.Collections.Generic;

namespace OrchardCore.XmlRpc.Models
{
    public class XRpcStruct
    {
        public XRpcStruct()
        {
            Members = new Dictionary<string, XRpcData>();
        }
        public IDictionary<string, XRpcData> Members { get; set; }

        public object this[string index]
        {
            get { return Members[index].Value; }
        }

        public XRpcStruct Set<T>(string name, T value)
        {
            Members[name] = XRpcData.For(value);
            return this;
        }

        public T Optional<T>(string name)
        {
            if (Members.TryGetValue(name, out var data))
            {
                return (T)data.Value;
            }

            return default;
        }
    }
}
