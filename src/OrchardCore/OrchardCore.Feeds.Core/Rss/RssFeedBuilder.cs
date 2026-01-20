using System;
using System.Threading.Tasks;
using System.Xml.Linq;
using OrchardCore.Feeds.Models;

namespace OrchardCore.Feeds.Rss
{
    public class RssFeedBuilder : IFeedBuilder
    {
        public async Task<XDocument> ProcessAsync(FeedContext context, Func<Task> populate)
        {
            var rss = new XElement("rss");
            rss.SetAttributeValue("version", "2.0");

            var channel = new XElement("channel");
            context.Response.Element = channel;
            rss.Add(channel);

            await populate();

            return new XDocument(rss);
        }

        public FeedItem<TItem> AddItem<TItem>(FeedContext context, TItem item)
        {
            var feedItem = new FeedItem<TItem>
            {
                Item = item,
                Element = new XElement("item"),
            };
            context.Response.Items.Add(feedItem);
            context.Response.Element.Add(feedItem.Element);
            return feedItem;
        }

        public void AddProperty(FeedContext context, FeedItem feedItem, XElement element)
        {
            if (feedItem == null)
            {
                context.Response.Element.Add(element);
            }
            else
            {
                feedItem.Element.Add(element);
            }
        }
    }
}
