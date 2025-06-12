using System.Linq.Expressions;
using Microsoft.Extensions.Logging;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Records;
using OrchardCore.Modules;
using OrchardCore.Navigation;
using OrchardCore.Taxonomies.Indexing;
using OrchardCore.Taxonomies.Models;
using YesSql;

namespace OrchardCore.Taxonomies.Core;

public sealed class DefaultContentsTaxonomyListQueryService : IContentsTaxonomyListQueryService
{
    private readonly ISession _session;
    private readonly IEnumerable<IContentsTaxonomyListFilter> _contentTaxonomyListFilters;
    private readonly ILogger _logger;

    public DefaultContentsTaxonomyListQueryService(
        ISession session,
        IEnumerable<IContentsTaxonomyListFilter> contentTaxonomyListFilters,
        ILogger<DefaultContentsTaxonomyListQueryService> logger)
    {
        _session = session;
        _contentTaxonomyListFilters = contentTaxonomyListFilters;
        _logger = logger;
    }

    public async Task<IQuery<ContentItem>> QueryAsync(TermPart termPart, PagerSlim pager)
    {
        IQuery<ContentItem> query = _session.Query<ContentItem>()
            .With<TaxonomyIndex>(x => x.TermContentItemId == termPart.ContentItem.ContentItemId);

        await _contentTaxonomyListFilters.InvokeAsync((filter, q, part) => filter.FilterAsync(q, part), query, termPart, _logger);

        if (pager.Before != null)
        {
            var beforeValue = new DateTime(long.Parse(pager.Before));

            query = query
                .With(CreateContentIndexFilter(beforeValue, null))
                .ThenBy(x => x.CreatedUtc);
        }
        else if (pager.After != null)
        {
            var afterValue = new DateTime(long.Parse(pager.After));
            query = query.With(CreateContentIndexFilter(null, afterValue))
                .ThenByDescending(x => x.CreatedUtc);
        }
        else
        {
            query = query.With(CreateContentIndexFilter(null, null))
                .ThenByDescending(x => x.CreatedUtc);
        }

        return query.With<TaxonomyIndex>()
            .ThenBy(x => x.Id)
            .Take(pager.PageSize + 1);
    }

    private static Expression<Func<ContentItemIndex, bool>> CreateContentIndexFilter(DateTime? before, DateTime? after)
    {
        if (before != null)
        {
            return x => x.Published && x.CreatedUtc > before;
        }

        if (after != null)
        {
            return x => x.Published && x.CreatedUtc < after;
        }

        return x => x.Published;
    }
}
