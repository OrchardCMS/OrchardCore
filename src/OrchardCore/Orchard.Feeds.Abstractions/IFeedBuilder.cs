using System;
using System.Threading.Tasks;
using System.Xml.Linq;
using Orchard.Feeds.Models;

namespace Orchard.Feeds
{
    public interface IFeedBuilder
    {
        Task<XDocument> ProcessAsync(FeedContext context, Func<Task> populate);
        FeedItem<TItem> AddItem<TItem>(FeedContext context, TItem contentItem);
        void AddProperty(FeedContext context, FeedItem feedItem, string name, string value);
    }
}