using System;
using OrchardCore.ContentManagement;
using OrchardCore.XmlRpc;
using OrchardCore.XmlRpc.Models;

namespace OrchardCore.MetaWeblog
{
    public interface IMetaWeblogDriver
    {
        void SetCapabilities(Action<string, string> setCapability);
        void BuildPost(XRpcStruct rpcStruct, XmlRpcContext context, ContentItem contentItem);
        void EditPost(XRpcStruct rpcStruct, ContentItem contentItem);
    }
}
