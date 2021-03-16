using System;
using System.Threading.Tasks;
using System.Xml.Linq;
using OrchardCore.Feeds.Models;

namespace OrchardCore.Feeds
{
    public interface IFeedBuilder
    {
        Task<XDocument> ProcessAsync(FeedContext context, Func<Task> populate);

        FeedItem<TItem> AddItem<TItem>(FeedContext context, TItem contentItem);

        void AddProperty(FeedContext context, FeedItem feedItem, XElement element);
    }
}
