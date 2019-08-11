using OrchardCore.ContentManagement;
using OrchardCore.XmlRpc;
using OrchardCore.XmlRpc.Models;
using OrchardCore.MetaWeblog;
using OrchardCore.Html.Model;

namespace OrchardCore.Html.RemotePublishing
{
    public class HtmlBodyMetaWeblogDriver : MetaWeblogDriver
    {
        public override void BuildPost(XRpcStruct rpcStruct, XmlRpcContext context, ContentItem contentItem)
        {
            var bodyPart = contentItem.As<HtmlBodyPart>();
            if (bodyPart == null)
            {
                return;
            }

            rpcStruct.Set("description", bodyPart.Html);
        }

        public override void EditPost(XRpcStruct rpcStruct, ContentItem contentItem)
        {
            if (contentItem.As<HtmlBodyPart>() != null)
            {
                contentItem.Alter<HtmlBodyPart>(x => x.Html = rpcStruct.Optional<string>("description"));
            }
        }
    }
}
