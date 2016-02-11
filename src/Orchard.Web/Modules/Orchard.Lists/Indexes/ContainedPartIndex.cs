using Orchard.ContentManagement;
using Orchard.DependencyInjection;
using Orchard.Lists.Models;
using System;
using YesSql.Core.Indexes;

namespace Orchard.Lists.Indexes
{
    public class ContainedPartIndex : MapIndex
    {
        public int ListContentItemId { get; set; }
        public int Order { get; set; }
    }

    public class ContentItemIndexProvider : IndexProvider<ContentItem>, IDependency
    {
        public override void Describe(DescribeContext<ContentItem> context)
        {
            context.For<ContainedPartIndex>()
                .Map(contentItem =>
                {
                    var containedPart = contentItem.As<ContainedPart>();
                    if(containedPart != null)
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