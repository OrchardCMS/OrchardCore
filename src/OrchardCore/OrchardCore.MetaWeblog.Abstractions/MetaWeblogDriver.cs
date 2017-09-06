using System;
using OrchardCore.ContentManagement;
using OrchardCore.XmlRpc;
using OrchardCore.XmlRpc.Models;

namespace OrchardCore.MetaWeblog
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
