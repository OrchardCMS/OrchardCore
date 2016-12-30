using Orchard.ContentManagement.Handlers;
using Orchard.Feeds.Models;
using Orchard.Lists.Models;

namespace Orchard.Lists.Drivers
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