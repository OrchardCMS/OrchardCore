using OrchardCore.ContentManagement;
using OrchardCore.Lists.Models;
using YesSql.Indexes;

namespace OrchardCore.Lists.Indexes
{
    public class ContainedPartIndex : MapIndex
    {
        public string ListContentItemId { get; set; }

        public int Order { get; set; }

        public string ContentItemId { get; set; }

        public string ListContentType { get; set; }

        public bool Published { get; set; }

        public bool Latest { get; set; }

        public string DisplayText { get; set; }
    }

    public class ContainedPartIndexProvider : IndexProvider<ContentItem>
    {
        public override void Describe(DescribeContext<ContentItem> context)
        {
            context.For<ContainedPartIndex>()
                .Map(contentItem =>
                {
                    if (!contentItem.Latest && !contentItem.Published)
                    {
                        return null;
                    }

                    var containedPart = contentItem.As<ContainedPart>();

                    if (containedPart == null)
                    {
                        return null;
                    }

                    return new ContainedPartIndex
                    {
                        ContentItemId = contentItem.ContentItemId,
                        ListContentType = containedPart.ListContentType,
                        ListContentItemId = containedPart.ListContentItemId,
                        Order = containedPart.Order,
                        Published = contentItem.Published,
                        Latest = contentItem.Latest,
                        DisplayText = contentItem.DisplayText,
                    };
                });
        }
    }
}
