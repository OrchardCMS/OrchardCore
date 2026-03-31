using OrchardCore.ContentManagement;
using OrchardCore.Html.Models;
using OrchardCore.MetaWeblog;
using OrchardCore.XmlRpc;
using OrchardCore.XmlRpc.Models;

namespace OrchardCore.Html.RemotePublishing;

public sealed class HtmlBodyMetaWeblogDriver : MetaWeblogDriver
{
    public override void BuildPost(XRpcStruct rpcStruct, XmlRpcContext context, ContentItem contentItem)
    {
        if (!contentItem.TryGet<HtmlBodyPart>(out var bodyPart))
        {
            return;
        }

        rpcStruct.Set("description", bodyPart.Html);
    }

    public override void EditPost(XRpcStruct rpcStruct, ContentItem contentItem)
    {
        if (contentItem.TryGet<HtmlBodyPart>(out _))
        {
            contentItem.Alter<HtmlBodyPart>(x => x.Html = rpcStruct.Optional<string>("description"));
        }
    }
}
