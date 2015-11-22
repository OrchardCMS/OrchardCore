using Orchard.DependencyInjection;
using YesSql.Core.Indexes;

namespace Orchard.ContentManagement.Records
{
    public class ContentItemIndex : MapIndex
    {
        public int ContentItemId { get; set; }
        public int Number { get; set; }
        public bool Published { get; set; }
        public bool Latest { get; set; }
        public string ContentType { get; set; }
    }

    public class ContentItemIndexProvider : IndexProvider<ContentItem>, IDependency
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
                    ContentItemId = contentItem.ContentItemId
                });
        }
    }
}