using OrchardCore.ContentManagement;
using OrchardCore.Lists.Models;
using YesSql.Indexes;

namespace OrchardCore.Lists.Indexes
{
    public class ContainedPartIndex : MapIndex
    {
        public string ListContentItemId { get; set; }

        public int Order { get; set; }

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
                .When(contentItem => contentItem.Has<ContainedPart>())
                .Map(contentItem =>
                {
                    var containedPart = contentItem.As<ContainedPart>();
                    if (containedPart == null)
                    {
                        return null;
                    }

                    return new ContainedPartIndex
                    {
                        ListContentItemId = containedPart.ListContentItemId,
                        Order = containedPart.Order,
                        ListContentType = containedPart.ListContentType,
                        DisplayText = contentItem.DisplayText,
                        Published = contentItem.Published,
                        Latest = contentItem.Latest,
                    };
                });
        }
    }
}
