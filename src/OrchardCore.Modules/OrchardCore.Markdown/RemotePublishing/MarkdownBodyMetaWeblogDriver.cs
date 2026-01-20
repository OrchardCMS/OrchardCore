using OrchardCore.ContentManagement;
using OrchardCore.Markdown.Models;
using OrchardCore.MetaWeblog;
using OrchardCore.XmlRpc;
using OrchardCore.XmlRpc.Models;

namespace OrchardCore.Markdown.RemotePublishing
{
    public class MarkdownBodyMetaWeblogDriver : MetaWeblogDriver
    {
        public override void BuildPost(XRpcStruct rpcStruct, XmlRpcContext context, ContentItem contentItem)
        {
            var bodyPart = contentItem.As<MarkdownBodyPart>();
            if (bodyPart == null)
            {
                return;
            }

            rpcStruct.Set("description", bodyPart.Markdown);
        }

        public override void EditPost(XRpcStruct rpcStruct, ContentItem contentItem)
        {
            if (contentItem.As<MarkdownBodyPart>() != null)
            {
                contentItem.Alter<MarkdownBodyPart>(x => x.Markdown = rpcStruct.Optional<string>("description"));
            }
        }
    }
}
