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
                .Map(contentItem =>
                {
                    var containedPart = contentItem.As<ContainedPart>();
                    if (containedPart != null)
                    {
                        return new ContainedPartIndex
                        {
                            ListContentItemId = containedPart.ListContentItemId,
                            Order = containedPart.Order
                        };
                    }

                    return null;
                });
        }
    }
}
