using OrchardCore.ContentManagement;
using OrchardCore.Markdown.Models;
using OrchardCore.MetaWeblog;
using OrchardCore.XmlRpc;
using OrchardCore.XmlRpc.Models;

namespace OrchardCore.Markdown.RemotePublishing;

public sealed class MarkdownBodyMetaWeblogDriver : MetaWeblogDriver
{
    public override void BuildPost(XRpcStruct rpcStruct, XmlRpcContext context, ContentItem contentItem)
    {
        if (!contentItem.TryGet<MarkdownBodyPart>(out var bodyPart))
        {
            return;
        }

        rpcStruct.Set("description", bodyPart.Markdown);
    }

    public override void EditPost(XRpcStruct rpcStruct, ContentItem contentItem)
    {
        if (contentItem.TryGet<MarkdownBodyPart>(out _))
        {
            contentItem.Alter<MarkdownBodyPart>(x => x.Markdown = rpcStruct.Optional<string>("description"));
        }
    }
}
