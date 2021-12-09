using System.Collections.Generic;

namespace OrchardCore.XmlRpc.Models
{
    public class XRpcArray
    {
        public XRpcArray()
        {
            Data = new List<XRpcData>();
        }
        public IList<XRpcData> Data { get; set; }

        public object this[int index]
        {
            get { return Data[index].Value; }
        }

        public XRpcArray Add<T>(T value)
        {
            Data.Add(XRpcData.For(value));
            return this;
        }
    }
}
