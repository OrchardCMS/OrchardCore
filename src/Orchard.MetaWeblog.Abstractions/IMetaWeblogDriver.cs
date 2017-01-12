using System;
using Orchard.ContentManagement;
using Orchard.Core.XmlRpc;
using Orchard.Core.XmlRpc.Models;

namespace Orchard.MetaWeblog
{
    public interface IMetaWeblogDriver
    {
        void SetCapabilities(Action<string, string> setCapability);
        void BuildPost(XRpcStruct rpcStruct, XmlRpcContext context, ContentItem contentItem);
        void EditPost(XRpcStruct rpcStruct, ContentItem contentItem);
    }
}
