using System.Xml.Linq;
using OrchardCore.Feeds.Models;

namespace OrchardCore.Feeds
{
    public static class FeedBuilderExtensions
    {
        public static void AddProperty(this IFeedBuilder feedBuilder, FeedContext context, FeedItem feedItem, string name, string value)
            => feedBuilder.AddProperty(context, feedItem, new XElement(name, value));
    }
}
