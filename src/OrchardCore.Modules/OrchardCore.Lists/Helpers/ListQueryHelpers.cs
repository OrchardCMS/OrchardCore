using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using OrchardCore.Navigation;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Records;
using OrchardCore.Lists.Models;
using OrchardCore.Lists.Indexes;
using System.Linq.Expressions;
using YesSql;

namespace OrchardCore.Lists.Helpers
{
    internal static class ListQueryHelpers
    {
        internal static async Task<int> QueryListItemsCountAsync(ISession session, string listContentItemId, Expression<Func<ContentItemIndex, bool>> itemPredicate)
        {
            return await session.Query<ContentItem>()
                    .With<ContainedPartIndex>(x => x.ListContentItemId == listContentItemId)
                    .With<ContentItemIndex>(itemPredicate)
                    .CountAsync();
        }

        internal static async Task<IEnumerable<ContentItem>> QueryListItemsAsync(ISession session, string listContentItemId, Expression<Func<ContentItemIndex, bool>> itemPredicate)
        {
            return await session.Query<ContentItem>()
                    .With<ContainedPartIndex>(x => x.ListContentItemId == listContentItemId)
                    .With<ContentItemIndex>(itemPredicate)
                    .ListAsync();
        }

        internal static async Task<IEnumerable<ContentItem>> QueryListItemsAsync(ISession session, ListPart listPart, PagerSlim pager, bool publishedOnly)
        {
            if (pager.Before != null)
            {
                var beforeValue = new DateTime(long.Parse(pager.Before));
                var query = session.Query<ContentItem>()
                    .With<ContainedPartIndex>(x => x.ListContentItemId == listPart.ContentItem.ContentItemId)
                    .With<ContentItemIndex>(CreateContentIndexFilter(beforeValue, null, publishedOnly))
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

                return containedItems;
            }
            else if (pager.After != null)
            {
                var afterValue = new DateTime(long.Parse(pager.After));
                var query = session.Query<ContentItem>()
                    .With<ContainedPartIndex>(x => x.ListContentItemId == listPart.ContentItem.ContentItemId)
                    .With<ContentItemIndex>(CreateContentIndexFilter(null, afterValue, publishedOnly))
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

                return containedItems;
            }
            else
            {
                var query = session.Query<ContentItem>()
                    .With<ContainedPartIndex>(x => x.ListContentItemId == listPart.ContentItem.ContentItemId)
                    .With<ContentItemIndex>(CreateContentIndexFilter(null, null, publishedOnly))
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

                return containedItems;
            }
        }

        internal static Expression<Func<ContentItemIndex, bool>> CreateContentIndexFilter(DateTime? before, DateTime? after, bool publishedOnly)
        {
            if (before != null)
            {
                if (publishedOnly)
                {
                    return x => x.Published && x.CreatedUtc > before;
                }
                else
                {
                    return x => x.Latest && x.CreatedUtc > before;
                }
            }

            if (after != null)
            {
                if (publishedOnly)
                {
                    return x => x.Published && x.CreatedUtc < after;
                }
                else
                {
                    return x => x.Latest && x.CreatedUtc < after;
                }
            }

            if (publishedOnly)
            {
                return x => x.Published;
            }
            else
            {
                return x => x.Latest;
            }
        }
    }
}
