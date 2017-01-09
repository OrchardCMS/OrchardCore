using Orchard.ContentManagement;
using Orchard.Core.XmlRpc;
using Orchard.Core.XmlRpc.Models;
using Orchard.MetaWeblog;
using Orchard.Body.Model;

namespace Orchard.Body.RemotePublishing
{
    public class BodyMetaWeblogDriver : MetaWeblogDriver
    {
        public override void BuildPost(XRpcStruct rpcStruct, XmlRpcContext context, ContentItem contentItem)
        {
            var bodyPart = contentItem.As<BodyPart>();
            if (bodyPart == null)
            {
                return;
            }

            rpcStruct.Set("description", bodyPart.Body);
        }

        public override void EditPost(XRpcStruct rpcStruct, ContentItem contentItem)
        {
            if (contentItem.As<BodyPart>() != null)
            {
                contentItem.Alter<BodyPart>(x => x.Body = rpcStruct.Optional<string>("description"));
            }
        }
    }
}
