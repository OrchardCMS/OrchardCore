using System;
using DotLiquid;
using Orchard.ContentManagement;
using Newtonsoft.Json.Linq;

namespace Orchard.Liquid.Drops
{
    public class ContentItemDrop : Drop
    {
        public ContentItemDrop(ContentItem contentItem)
        {
            ContentItem = contentItem;
        }

        public ContentItem ContentItem { get; }

        public int Id => ContentItem.Id;
        public string ContentItemId => ContentItem.ContentItemId;
        public string ContentItemVersionId => ContentItem.ContentItemVersionId;
        public int Number => ContentItem.Number;
        public string Owner => ContentItem.Owner;
        public string Author => ContentItem.Author;
        public bool Published => ContentItem.Published;
        public bool Latest => ContentItem.Latest;
        public string ContentType => ContentItem.ContentType;
        public DateTime? CreatedUtc => ContentItem.CreatedUtc;
        public DateTime? ModifiedUtc => ContentItem.ModifiedUtc;
        public DateTime? PublishedUtc => ContentItem.PublishedUtc;
        public JTokenDrop Content => new JTokenDrop((JToken)ContentItem.Content);

    }
}
