using System.Linq.Expressions;
using Microsoft.Extensions.Options;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.ContentManagement.Records;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Navigation;
using OrchardCore.Taxonomies.Core;
using OrchardCore.Taxonomies.Models;
using OrchardCore.Taxonomies.ViewModels;
using YesSql;

namespace OrchardCore.Taxonomies.Drivers;

public sealed class TermPartContentDriver : ContentDisplayDriver
{
    private readonly ISession _session;
    private readonly IContentsTaxonomyListQueryService _contentsTaxonomyListQueryService;
    private readonly PagerOptions _pagerOptions;
    private readonly IContentManager _contentManager;

    public TermPartContentDriver(
        ISession session,
        IOptions<PagerOptions> pagerOptions,
        IContentsTaxonomyListQueryService contentsTaxonomyListQueryService,
        IContentManager contentManager)
    {
        _session = session;
        _contentsTaxonomyListQueryService = contentsTaxonomyListQueryService;
        _pagerOptions = pagerOptions.Value;
        _contentManager = contentManager;
    }

    public override IDisplayResult Display(ContentItem model, BuildDisplayContext context)
    {
        var part = model.As<TermPart>();
        if (part != null)
        {
            return Initialize<TermPartViewModel>("TermPart", async m =>
            {
                var pager = await GetPagerAsync(context.Updater, _pagerOptions.GetPageSize()).ConfigureAwait(false);
                m.TaxonomyContentItemId = part.TaxonomyContentItemId;
                m.ContentItem = part.ContentItem;
                m.ContentItems = (await QueryTermItemsAsync(part, pager).ConfigureAwait(false)).ToArray();
                m.Pager = await context.New.PagerSlim(pager);
            }).Location(OrchardCoreConstants.DisplayType.Detail, "Content:5");
        }

        return null;
    }

    private async Task<IEnumerable<ContentItem>> QueryTermItemsAsync(TermPart termPart, PagerSlim pager)
    {
        var query = await _contentsTaxonomyListQueryService.QueryAsync(termPart, pager).ConfigureAwait(false);

        var containedItems = await query.ListAsync().ConfigureAwait(false);

        if (!containedItems.Any())
        {
            return containedItems;
        }

        if (pager.Before != null)
        {
            containedItems = containedItems.Reverse();

            // There is always an After as we clicked on Before.
            pager.Before = null;
            pager.After = containedItems.Last().CreatedUtc.Value.Ticks.ToString();

            if (containedItems.Count() == pager.PageSize + 1)
            {
                containedItems = containedItems.Skip(1);
                pager.Before = containedItems.First().CreatedUtc.Value.Ticks.ToString();
            }

            return await _contentManager.LoadAsync(containedItems).ConfigureAwait(false);
        }

        if (pager.After != null)
        {
            // There is always a Before page as we clicked on After.
            pager.Before = containedItems.First().CreatedUtc.Value.Ticks.ToString();
            pager.After = null;

            if (containedItems.Count() == pager.PageSize + 1)
            {
                containedItems = containedItems.Take(pager.PageSize);
                pager.After = containedItems.Last().CreatedUtc.Value.Ticks.ToString();
            }

            return await _contentManager.LoadAsync(containedItems).ConfigureAwait(false);
        }

        pager.Before = null;
        pager.After = null;

        if (containedItems.Count() == pager.PageSize + 1)
        {
            containedItems = containedItems.Take(pager.PageSize);
            pager.After = containedItems.Last().CreatedUtc.Value.Ticks.ToString();
        }

        return await _contentManager.LoadAsync(containedItems).ConfigureAwait(false);
    }

    private static async Task<PagerSlim> GetPagerAsync(IUpdateModel updater, int pageSize)
    {
        var pagerParameters = new PagerSlimParameters();
        await updater.TryUpdateModelAsync(pagerParameters).ConfigureAwait(false);

        var pager = new PagerSlim(pagerParameters, pageSize);

        return pager;
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
