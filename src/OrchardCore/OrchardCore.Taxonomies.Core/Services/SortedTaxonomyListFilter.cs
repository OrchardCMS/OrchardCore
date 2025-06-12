using OrchardCore.ContentManagement;
using OrchardCore.Taxonomies.Indexing;
using OrchardCore.Taxonomies.Models;
using YesSql;

namespace OrchardCore.Taxonomies.Core.Services;

public sealed class SortedTaxonomyListFilter : IContentTaxonomyListFilter
{
    public Task FilterAsync(IQuery<ContentItem> query, TermPart termPart)
    {
        query = query.With<SortedTaxonomyIndex>()
            .OrderBy(x => x.Order);

        return Task.CompletedTask;
    }
}
