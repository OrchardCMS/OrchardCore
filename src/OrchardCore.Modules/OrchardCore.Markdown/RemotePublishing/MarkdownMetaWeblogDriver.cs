using OrchardCore.ContentManagement;
using OrchardCore.XmlRpc;
using OrchardCore.XmlRpc.Models;
using OrchardCore.MetaWeblog;
using OrchardCore.Markdown.Model;

namespace OrchardCore.Markdown.RemotePublishing
{
    public class MarkdownMetaWeblogDriver : MetaWeblogDriver
    {
        public override void BuildPost(XRpcStruct rpcStruct, XmlRpcContext context, ContentItem contentItem)
        {
            var bodyPart = contentItem.As<MarkdownPart>();
            if (bodyPart == null)
            {
                return;
            }

            rpcStruct.Set("description", bodyPart.Markdown);
        }

        public override void EditPost(XRpcStruct rpcStruct, ContentItem contentItem)
        {
            if (contentItem.As<MarkdownPart>() != null)
            {
                contentItem.Alter<MarkdownPart>(x => x.Markdown = rpcStruct.Optional<string>("description"));
            }
        }
    }
}
