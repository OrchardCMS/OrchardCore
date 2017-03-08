using System;
using YesSql.Core.Indexes;

namespace Orchard.ContentManagement.Records
{
    public class ContentItemIndex : MapIndex
    {
        public int DocumentId { get; set; }
        public string ContentItemId { get; set; }
        public int Number { get; set; }
        public bool Published { get; set; }
        public bool Latest { get; set; }
        public string ContentType { get; set; }
        public DateTime? ModifiedUtc { get; set; }
        public DateTime? PublishedUtc { get; set; }
        public DateTime? CreatedUtc { get; set; }
        public string Owner { get; set; }
        public string Author { get; set; }
    }

    public class ContentItemIndexProvider : IndexProvider<ContentItem>
    {
        public override void Describe(DescribeContext<ContentItem> context)
        {
            context.For<ContentItemIndex>()
                .Map(contentItem => new ContentItemIndex
                {
                    Latest = contentItem.Latest,
                    Number = contentItem.Number,
                    Published = contentItem.Published,
                    ContentType = contentItem.ContentType,
                    ContentItemId = contentItem.ContentItemId,
                    ModifiedUtc = contentItem.ModifiedUtc,
                    PublishedUtc = contentItem.PublishedUtc,
                    CreatedUtc = contentItem.CreatedUtc,
                    Owner = contentItem.Owner,
                    Author = contentItem.Author
                });
        }
    }
}