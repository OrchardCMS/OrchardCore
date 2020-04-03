using OrchardCore.Feeds.Models;

namespace OrchardCore.Feeds.Rss
{
    public class RssFeedBuilderProvider : IFeedBuilderProvider
    {
        public FeedBuilderMatch Match(FeedContext context)
        {
            if (context.Format == "rss")
            {
                return new FeedBuilderMatch
                {
                    FeedBuilder = new RssFeedBuilder(),
                    Priority = -5
                };
            }

            return null;
        }
    }
}
