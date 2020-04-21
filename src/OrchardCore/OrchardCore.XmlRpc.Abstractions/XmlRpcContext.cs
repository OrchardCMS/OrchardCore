using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OrchardCore.XmlRpc.Models;

namespace OrchardCore.XmlRpc
{
    public class XmlRpcContext
    {
        public ControllerContext ControllerContext { get; set; }
        public HttpContext HttpContext { get; set; }
        public IUrlHelper Url { get; set; }
        public XRpcMethodCall RpcMethodCall { get; set; }
        public XRpcMethodResponse RpcMethodResponse { get; set; }
        public ICollection<IXmlRpcDriver> Drivers = new List<IXmlRpcDriver>();
    }
}
