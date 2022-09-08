using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.ContentManagement.Records;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Navigation;
using OrchardCore.Taxonomies.Indexing;
using OrchardCore.Taxonomies.Models;
using OrchardCore.Taxonomies.ViewModels;
using YesSql;

namespace OrchardCore.Taxonomies.Drivers
{
    public class TermPartContentDriver : ContentDisplayDriver
    {
        private readonly ISession _session;
        private readonly PagerOptions _pagerOptions;
        private readonly IContentManager _contentManager;

        public TermPartContentDriver(
            ISession session,
            IOptions<PagerOptions> pagerOptions,
            IContentManager contentManager)
        {
            _session = session;
            _pagerOptions = pagerOptions.Value;
            _contentManager = contentManager;
        }

        public override Task<IDisplayResult> DisplayAsync(ContentItem model, BuildDisplayContext context)
        {
            var part = model.As<TermPart>();
            if (part != null)
            {
                return Task.FromResult<IDisplayResult>(Initialize<TermPartViewModel>("TermPart", async m =>
                {
                    var pager = await GetPagerAsync(context.Updater, _pagerOptions.PageSize);
                    m.TaxonomyContentItemId = part.TaxonomyContentItemId;
                    m.ContentItem = part.ContentItem;
                    m.ContentItems = (await QueryTermItemsAsync(part, pager)).ToArray();
                    m.Pager = await context.New.PagerSlim(pager);
                })
                .Location("Detail", "Content:5"));
            }

            return Task.FromResult<IDisplayResult>(null);
        }

        private async Task<IEnumerable<ContentItem>> QueryTermItemsAsync(TermPart termPart, PagerSlim pager)
        {
            if (pager.Before != null)
            {
                var beforeValue = new DateTime(long.Parse(pager.Before));
                var query = _session.Query<ContentItem>()
                    .With<TaxonomyIndex>(x => x.TermContentItemId == termPart.ContentItem.ContentItemId)
                    .With<ContentItemIndex>(CreateContentIndexFilter(beforeValue, null))
                    .OrderBy(x => x.CreatedUtc)
                    .Take(pager.PageSize + 1);

                var containedItems = await query.ListAsync();

                if (containedItems.Count() == 0)
                {
                    return containedItems;
                }

                containedItems = containedItems.Reverse();

                // There is always an After as we clicked on Before
                pager.Before = null;
                pager.After = containedItems.Last().CreatedUtc.Value.Ticks.ToString();

                if (containedItems.Count() == pager.PageSize + 1)
                {
                    containedItems = containedItems.Skip(1);
                    pager.Before = containedItems.First().CreatedUtc.Value.Ticks.ToString();
                }

                return await _contentManager.LoadAsync(containedItems);
            }
            else if (pager.After != null)
            {
                var afterValue = new DateTime(long.Parse(pager.After));
                var query = _session.Query<ContentItem>()
                    .With<TaxonomyIndex>(x => x.TermContentItemId == termPart.ContentItem.ContentItemId)
                    .With<ContentItemIndex>(CreateContentIndexFilter(null, afterValue))
                    .OrderByDescending(x => x.CreatedUtc)
                    .Take(pager.PageSize + 1);

                var containedItems = await query.ListAsync();

                if (containedItems.Count() == 0)
                {
                    return containedItems;
                }

                // There is always a Before page as we clicked on After
                pager.Before = containedItems.First().CreatedUtc.Value.Ticks.ToString();
                pager.After = null;

                if (containedItems.Count() == pager.PageSize + 1)
                {
                    containedItems = containedItems.Take(pager.PageSize);
                    pager.After = containedItems.Last().CreatedUtc.Value.Ticks.ToString();
                }

                return await _contentManager.LoadAsync(containedItems);
            }
            else
            {
                var query = _session.Query<ContentItem>()
                    .With<TaxonomyIndex>(x => x.TermContentItemId == termPart.ContentItem.ContentItemId)
                    .With<ContentItemIndex>(CreateContentIndexFilter(null, null))
                    .OrderByDescending(x => x.CreatedUtc)
                    .Take(pager.PageSize + 1);

                var containedItems = await query.ListAsync();

                if (containedItems.Count() == 0)
                {
                    return containedItems;
                }

                pager.Before = null;
                pager.After = null;

                if (containedItems.Count() == pager.PageSize + 1)
                {
                    containedItems = containedItems.Take(pager.PageSize);
                    pager.After = containedItems.Last().CreatedUtc.Value.Ticks.ToString();
                }

                return await _contentManager.LoadAsync(containedItems);
            }
        }

        private static async Task<PagerSlim> GetPagerAsync(IUpdateModel updater, int pageSize)
        {
            var pagerParameters = new PagerSlimParameters();
            await updater.TryUpdateModelAsync(pagerParameters);

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
}
