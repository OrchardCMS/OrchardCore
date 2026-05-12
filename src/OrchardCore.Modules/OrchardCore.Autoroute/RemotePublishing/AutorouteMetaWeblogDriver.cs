using OrchardCore.Autoroute.Models;
using OrchardCore.ContentManagement;
using OrchardCore.MetaWeblog;
using OrchardCore.XmlRpc;
using OrchardCore.XmlRpc.Models;

namespace OrchardCore.Autoroute.RemotePublishing;

public sealed class AutorouteMetaWeblogDriver : MetaWeblogDriver
{
    public override void SetCapabilities(Action<string, string> setCapability)
    {
        setCapability("supportsSlug", "Yes");
    }

    public override void BuildPost(XRpcStruct rpcStruct, XmlRpcContext context, ContentItem contentItem)
    {
        if (!contentItem.TryGet<AutoroutePart>(out var autoroutePart))
        {
            return;
        }

        rpcStruct.Set("wp_slug", autoroutePart.Path);
    }

    public override void EditPost(XRpcStruct rpcStruct, ContentItem contentItem)
    {
        if (contentItem.TryGet<AutoroutePart>(out _))
        {
            var slug = rpcStruct.Optional<string>("wp_slug");

            if (!string.IsNullOrWhiteSpace(slug))
            {
                contentItem.Alter<AutoroutePart>(x => x.Path = slug);
            }
        }
    }
}
