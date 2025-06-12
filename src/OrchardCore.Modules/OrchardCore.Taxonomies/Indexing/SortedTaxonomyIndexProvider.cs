using OrchardCore.ContentManagement;
using OrchardCore.Taxonomies.Core.Models;
using YesSql.Indexes;

namespace OrchardCore.Taxonomies.Indexing;

public sealed class SortedTaxonomyIndexProvider : IndexProvider<ContentItem>
{
    public override void Describe(DescribeContext<ContentItem> context)
    {
        context.For<SortedTaxonomyIndex>()
            .Map(contentItem =>
            {
                // Remove index records of soft deleted items.
                if (!contentItem.Published)
                {
                    return null;
                }

                var part = contentItem.As<TaxonomySortPart>();

                return new SortedTaxonomyIndex
                {
                    ContentItemId = contentItem.ContentItemId,
                    Order = part?.Sort ?? 0,
                };
            });
    }
}
