using System.Threading.Tasks;
using OrchardCore.ContentManagement.Handlers;
using OrchardCore.Feeds.Models;
using OrchardCore.Lists.Models;

namespace OrchardCore.Lists.Drivers
{
    public class ListPartFeedHandler : ContentPartHandler<ListPart>
    {
        public override Task GetContentItemAspectAsync(ContentItemAspectContext context, ListPart part)
        {
            return context.ForAsync<FeedMetadata>(feedMetadata =>
            {
                // Disable the feed if the value is not defined
                feedMetadata.EnableFeedProxyUrl = part.Content.EnableFeedProxyUrl ?? false;
                feedMetadata.FeedProxyUrl = part.Content.FeedProxyUrl;

                return Task.CompletedTask;
            });
        }
    }
}
