using System.Collections.Generic;

namespace OrchardCore.XmlRpc.Models
{
    public class XRpcMethodCall
    {
        public string MethodName { get; set; }
        public IList<XRpcData> Params { get; set; } = new List<XRpcData>();
    }
}
