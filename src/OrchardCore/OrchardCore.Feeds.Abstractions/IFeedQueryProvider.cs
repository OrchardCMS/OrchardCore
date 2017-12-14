using OrchardCore.Feeds.Models;

namespace OrchardCore.Feeds
{
    public interface IFeedQueryProvider
    {
        FeedQueryMatch Match(FeedContext context);
    }

    public class FeedQueryMatch
    {
        public int Priority { get; set; }
        public IFeedQuery FeedQuery { get; set; }
    }
}