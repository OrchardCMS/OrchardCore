using OrchardCore.Feeds.Models;

namespace OrchardCore.Feeds
{
    public interface IFeedItemBuilder
    {
        void Populate(FeedContext context);
    }
}
