using System.Collections.Generic;

namespace OrchardCore.XmlRpc.Models
{
    public class XRpcMethodResponse
    {
        public XRpcMethodResponse()
        {
            Params = new List<XRpcData>();
        }

        public IList<XRpcData> Params { get; set; }
        public XRpcFault Fault { get; set; }

        public XRpcMethodResponse Add<T>(T value)
        {
            Params.Add(XRpcData.For(value));
            return this;
        }
    }
}
