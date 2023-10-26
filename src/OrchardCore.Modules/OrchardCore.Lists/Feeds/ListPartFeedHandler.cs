using System.Text.Json.Nodes;
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
                // Enable the feed if the value is not defined. This is handy to migrate the old feeds
                feedMetadata.DisableRssFeed = (part.Content as JsonObject)["DisableRssFeed"]?.GetValue<bool>() ?? false;
                feedMetadata.FeedProxyUrl = (part.Content as JsonObject)["FeedProxyUrl"]?.ToString();

                return Task.CompletedTask;
            });
        }
    }
}
