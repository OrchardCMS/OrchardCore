using Orchard.Feeds.Models;

namespace Orchard.Feeds
{
    public interface IFeedBuilderProvider
    {
        FeedBuilderMatch Match(FeedContext context);
    }

    public class FeedBuilderMatch
    {
        public int Priority { get; set; }
        public IFeedBuilder FeedBuilder { get; set; }
    }
}
