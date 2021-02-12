using OrchardCore.ContentManagement;
using OrchardCore.Lists.Models;
using YesSql.Indexes;

namespace OrchardCore.Lists.Indexes
{
    public class ContainedPartIndex : MapIndex
    {
        public string ListContentItemId { get; set; }
        public int Order { get; set; }
    }

    public class ContainedPartIndexProvider : IndexProvider<ContentItem>
    {
        public override void Describe(DescribeContext<ContentItem> context)
        {
            context.For<ContainedPartIndex>()
                // Keep index records of soft deleted items as they are contained items.
                .When(contentItem => contentItem.Has<ContainedPart>() && (contentItem.Published || contentItem.Latest))
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
                        Order = containedPart.Order
                    };
                });
        }
    }
}
