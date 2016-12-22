using Orchard.Feeds.Models;

namespace Orchard.Feeds
{
    public interface IFeedItemBuilder
    {
        void Populate(FeedContext context);
    }
}
