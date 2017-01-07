using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
    }
}
