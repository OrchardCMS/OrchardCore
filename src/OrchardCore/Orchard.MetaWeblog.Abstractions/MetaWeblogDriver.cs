using System;
using Orchard.ContentManagement;
using Orchard.Core.XmlRpc;
using Orchard.Core.XmlRpc.Models;

namespace Orchard.MetaWeblog
{
    public abstract class MetaWeblogDriver : IMetaWeblogDriver
    {
        public virtual void BuildPost(XRpcStruct rpcStruct, XmlRpcContext context, ContentItem contentItem)
        {
        }

        public virtual void EditPost(XRpcStruct rpcStruct, ContentItem contentItem)
        {
        }

        public virtual void SetCapabilities(Action<string, string> setCapability)
        {
        }
    }
}
