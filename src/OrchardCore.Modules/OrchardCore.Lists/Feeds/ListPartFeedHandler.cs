using OrchardCore.ContentManagement.Handlers;
using OrchardCore.Feeds.Models;
using OrchardCore.Lists.Models;

namespace OrchardCore.Lists.Drivers
{
    public class ListPartFeedHandler : ContentPartHandler<ListPart>
    {
        public override void GetContentItemAspect(ContentItemAspectContext context, ListPart part)
        {
            context.For<FeedMetadata>(feedMetadata =>
            {
                // If the value is not defined, it will be represented as null
                feedMetadata.FeedProxyUrl = part.Content.FeedProxyUrl;
            });
        }
    }
}