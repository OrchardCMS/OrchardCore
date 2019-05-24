using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Records;
using OrchardCore.Lists.Indexes;
using OrchardCore.Lists.Models;
using OrchardCore.Navigation;
using YesSql;

namespace OrchardCore.Lists.Services
{
    public class ListPartQueryService : IListPartQueryService
    {
        private readonly ISession _session;

        public ListPartQueryService(ISession session)
        {
            _session = session;
        }

        public async Task<IEnumerable<ContentItem>> QueryListItemsAsync(string contentItemId, bool enableOrdering, PagerSlim pager, bool publishedOnly)
        {
            IQuery<ContentItem> query = null;
            if (pager.Before != null)
            {
                var beforeValue = new DateTime(long.Parse(pager.Before));

                if (enableOrdering)
                {
                    query = _session.Query<ContentItem>()
                       .With<ContainedPartIndex>(x => x.ListContentItemId == contentItemId)
                       .OrderBy(x => x.Order)
                       .With<ContentItemIndex>(CreateContentIndexFilter(beforeValue, null, publishedOnly))
                       .Take(pager.PageSize + 1);
                } else
                {
                    query = _session.Query<ContentItem>()
                       .With<ContainedPartIndex>(x => x.ListContentItemId == contentItemId)
                       .With<ContentItemIndex>(CreateContentIndexFilter(beforeValue, null, publishedOnly))
                       .OrderBy(x => x.CreatedUtc)
                       .Take(pager.PageSize + 1);
                }
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

                if (enableOrdering)
                {
                    query = _session.Query<ContentItem>()
                      .With<ContainedPartIndex>(x => x.ListContentItemId == contentItemId)
                      .OrderBy(x => x.Order)
                      .With<ContentItemIndex>(CreateContentIndexFilter(null, afterValue, publishedOnly))
                      .Take(pager.PageSize + 1);
                }
                else
                {
                    query = _session.Query<ContentItem>()
                        .With<ContainedPartIndex>(x => x.ListContentItemId == contentItemId)
                        .With<ContentItemIndex>(CreateContentIndexFilter(null, afterValue, publishedOnly))
                        .OrderByDescending(x => x.CreatedUtc)
                        .Take(pager.PageSize + 1);
                }

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
                if (enableOrdering)
                {
                    query = _session.Query<ContentItem>()
                        .With<ContainedPartIndex>(x => x.ListContentItemId == contentItemId)
                        .OrderBy(x => x.Order)
                        .With<ContentItemIndex>(CreateContentIndexFilter(null, null, publishedOnly))
                        .Take(pager.PageSize + 1);
                }
                else
                {
                    query = _session.Query<ContentItem>()
                        .With<ContainedPartIndex>(x => x.ListContentItemId == contentItemId)
                        .With<ContentItemIndex>(CreateContentIndexFilter(null, null, publishedOnly))
                        .OrderByDescending(x => x.CreatedUtc)
                        .Take(pager.PageSize + 1);
                }

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

        private static Expression<Func<ContentItemIndex, bool>> CreateContentIndexFilter(DateTime? before, DateTime? after, bool publishedOnly)
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
