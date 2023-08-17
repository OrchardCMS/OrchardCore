using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Records;
using OrchardCore.Lists.Indexes;
using OrchardCore.Lists.Models;
using OrchardCore.Navigation;
using YesSql;
using YesSql.Services;

namespace OrchardCore.Lists.Services
{
    public class ContainerService : IContainerService
    {
        private readonly YesSql.ISession _session;
        private readonly IContentManager _contentManager;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ContainerService(
            YesSql.ISession session,
            IContentManager contentManager,
            IHttpContextAccessor httpContextAccessor)
        {
            _session = session;
            _contentManager = contentManager;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<int> GetNextOrderNumberAsync(string contentItemId)
        {
            var index = await _session.QueryIndex<ContainedPartIndex>(x => x.ListContentItemId == contentItemId)
                .OrderByDescending(x => x.Order)
                .FirstOrDefaultAsync();

            if (index != null)
            {
                return index.Order + 1;
            }
            else
            {
                return 0;
            }
        }

        public async Task UpdateContentItemOrdersAsync(IEnumerable<ContentItem> contentItems, int orderOfFirstItem)
        {
            var i = 0;
            foreach (var contentItem in contentItems)
            {
                var containedPart = contentItem.As<ContainedPart>();
                if (containedPart != null && containedPart.Order != orderOfFirstItem + i)
                {
                    containedPart.Order = orderOfFirstItem + i;
                    containedPart.Apply();

                    // Keep the published and draft orders the same to avoid confusion in the admin list.
                    if (!contentItem.IsPublished())
                    {
                        var publishedItem = await _contentManager.GetAsync(contentItem.ContentItemId, VersionOptions.Published);
                        if (publishedItem != null)
                        {
                            publishedItem.Alter<ContainedPart>(x => x.Order = orderOfFirstItem + i);
                            await _contentManager.UpdateAsync(publishedItem);
                        }
                    }

                    await _contentManager.UpdateAsync(contentItem);
                }

                i++;
            }
        }

        public async Task SetInitialOrder(string contentType)
        {
            // Set initial order for published and drafts if they have never been published.
            var contanerContentItemsQuery = _session.QueryIndex<ContentItemIndex>(x => x.ContentType == contentType && (x.Published || x.Latest));
            var containerContentItems = await contanerContentItemsQuery.ListAsync();

            if (!containerContentItems.Any())
            {
                return;
            }

            // Reduce duplicates to only set order for the published container item and the draft item if it has not been published.
            var containerContentItemIds = containerContentItems.Select(x => x.ContentItemId).Distinct();

            var containedItemsQuery = _session.Query<ContentItem>()
                .With<ContainedPartIndex>(x => x.ListContentItemId.IsIn(containerContentItemIds))
                .With<ContentItemIndex>(ci => ci.Latest || ci.Published)
                .OrderByDescending(x => x.CreatedUtc);

            // Load items so that loading handlers are invoked.
            var contentItemGroups = (await containedItemsQuery.ListAsync(_contentManager)).ToLookup(l => l.As<ContainedPart>()?.ListContentItemId);

            foreach (var contentItemGroup in contentItemGroups)
            {
                var i = 0;
                foreach (var contentItem in contentItemGroup)
                {
                    var containedPart = contentItem.As<ContainedPart>();
                    if (containedPart != null)
                    {
                        if (contentItem.Published && contentItem.Latest)
                        {
                            containedPart.Order = i;
                            containedPart.Apply();
                        }
                        else if (contentItem.Latest && !contentItem.Published)
                        {
                            // Update the latest order.
                            containedPart.Order = i;
                            containedPart.Apply();

                            // If a published version exists, find it, and set it to the same order as the draft.
                            var publishedItem = contentItemGroup.FirstOrDefault(p => p.Published == true && p.ContentItemId == contentItem.ContentItemId);
                            var publishedContainedPart = publishedItem?.As<ContainedPart>();
                            if (publishedContainedPart != null)
                            {
                                publishedContainedPart.Order = i;
                                publishedContainedPart.Apply();
                            }
                        }
                        else if (contentItem.Published && !contentItem.Latest)
                        {
                            // If a latest version exists, it will handle updating the order.
                            var latestItem = contentItemGroup.FirstOrDefault(l => l.Latest == true && l.ContentItemId == contentItem.ContentItemId);
                            if (latestItem == null)
                            {
                                // Apply order to the published item.
                                containedPart.Order = i;
                                containedPart.Apply();
                            }
                            else
                            {
                                // Order of this item will be updated when latest is iterated.
                                continue;
                            }
                        }

                        _session.Save(contentItem);
                    }

                    i++;
                }
            }
        }

        public async Task<IEnumerable<ContentItem>> QueryContainedItemsAsync(
            string contentItemId,
            bool enableOrdering,
            PagerSlim pager,
            ContainedItemOptions containedItemOptions)
        {
            if (containedItemOptions == null)
            {
                throw new ArgumentNullException(nameof(containedItemOptions));
            }

            IQuery<ContentItem> query = null;
            if (pager.Before != null)
            {
                if (enableOrdering)
                {
                    var beforeValue = Int32.Parse(pager.Before);
                    query = _session.Query<ContentItem>()
                        .With(CreateOrderedContainedPartIndexFilter(beforeValue, null, contentItemId))
                        .OrderByDescending(x => x.Order);
                }
                else
                {
                    var beforeValue = new DateTime(Int64.Parse(pager.Before));
                    query = _session.Query<ContentItem>()
                        .With<ContainedPartIndex>(x => x.ListContentItemId == contentItemId);

                    ApplyPagingContentIndexFilter(beforeValue, null, true, query);
                }

                ApplyContainedItemOptionsFilter(containedItemOptions, query);

                // Take() needs to be the last expression in the query otherwise the ORDER BY clause will be
                // syntactically incorrect.
                query.Take(pager.PageSize + 1);

                // Load items so that loading handlers are invoked.
                var containedItems = await query.ListAsync(_contentManager);

                if (!containedItems.Any())
                {
                    return containedItems;
                }

                containedItems = containedItems.Reverse();

                // There is always an After as we clicked on Before
                pager.Before = null;
                if (enableOrdering)
                {
                    pager.After = containedItems.Last().As<ContainedPart>().Order.ToString();
                }
                else
                {
                    pager.After = containedItems.Last().CreatedUtc.Value.Ticks.ToString();
                }
                if (containedItems.Count() == pager.PageSize + 1)
                {
                    containedItems = containedItems.Skip(1);
                    if (enableOrdering)
                    {
                        pager.Before = containedItems.First().As<ContainedPart>().Order.ToString();
                    }
                    else
                    {
                        pager.Before = containedItems.First().CreatedUtc.Value.Ticks.ToString();
                    }
                }

                return containedItems;
            }
            else if (pager.After != null)
            {
                if (enableOrdering)
                {
                    var afterValue = Int32.Parse(pager.After);
                    query = _session.Query<ContentItem>()
                        .With(CreateOrderedContainedPartIndexFilter(null, afterValue, contentItemId))
                        .OrderBy(x => x.Order);
                }
                else
                {
                    var afterValue = new DateTime(Int64.Parse(pager.After));
                    query = _session.Query<ContentItem>()
                        .With(CreateOrderedContainedPartIndexFilter(null, null, contentItemId));

                    ApplyPagingContentIndexFilter(null, afterValue, false, query);
                }

                ApplyContainedItemOptionsFilter(containedItemOptions, query);

                query.Take(pager.PageSize + 1);

                // Load items so that loading handlers are invoked.
                var containedItems = await query.ListAsync(_contentManager);

                if (!containedItems.Any())
                {
                    return containedItems;
                }

                // There is always a Before page as we clicked on After
                if (enableOrdering)
                {
                    pager.Before = containedItems.First().As<ContainedPart>().Order.ToString();
                }
                else
                {
                    pager.Before = containedItems.First().CreatedUtc.Value.Ticks.ToString();
                }
                pager.After = null;

                if (containedItems.Count() == pager.PageSize + 1)
                {
                    containedItems = containedItems.Take(pager.PageSize);
                    if (enableOrdering)
                    {
                        pager.After = containedItems.Last().As<ContainedPart>().Order.ToString();
                    }
                    else
                    {
                        pager.After = containedItems.Last().CreatedUtc.Value.Ticks.ToString();
                    }
                }

                return containedItems;
            }
            else
            {
                if (enableOrdering)
                {
                    query = _session.Query<ContentItem>()
                        .With(CreateOrderedContainedPartIndexFilter(null, null, contentItemId))
                        .OrderBy(x => x.Order);
                }
                else
                {
                    query = _session.Query<ContentItem>()
                        .With<ContainedPartIndex>(x => x.ListContentItemId == contentItemId);

                    ApplyPagingContentIndexFilter(null, null, false, query);
                }

                ApplyContainedItemOptionsFilter(containedItemOptions, query);

                query.Take(pager.PageSize + 1);

                // Load items so that loading handlers are invoked.
                var containedItems = await query.ListAsync(_contentManager);

                if (!containedItems.Any())
                {
                    return containedItems;
                }

                pager.Before = null;
                pager.After = null;

                if (containedItems.Count() == pager.PageSize + 1)
                {
                    containedItems = containedItems.Take(pager.PageSize);
                    if (enableOrdering)
                    {
                        pager.After = containedItems.Last().As<ContainedPart>().Order.ToString();
                    }
                    else
                    {
                        pager.After = containedItems.Last().CreatedUtc.Value.Ticks.ToString();
                    }
                }

                return containedItems;
            }
        }

        private static void ApplyPagingContentIndexFilter(DateTime? before, DateTime? after, bool orderByAsc, IQuery<ContentItem> query)
        {
            var indexQuery = query.With<ContentItemIndex>();

            if (before != null)
            {
                indexQuery.Where(i => i.CreatedUtc > before);
            }

            if (after != null)
            {
                indexQuery.Where(i => i.CreatedUtc < after);
            }

            if (orderByAsc)
            {
                indexQuery.OrderBy(i => i.CreatedUtc);
            }
            else
            {
                indexQuery.OrderByDescending(i => i.CreatedUtc);
            }
        }

        private void ApplyContainedItemOptionsFilter(ContainedItemOptions containedItemOptions, IQuery<ContentItem> query)
        {
            if (!String.IsNullOrEmpty(containedItemOptions.DisplayText))
            {
                query.With<ContentItemIndex>(i => i.DisplayText.Contains(containedItemOptions.DisplayText));
            }

            switch (containedItemOptions.Status)
            {
                case ContentsStatus.Published:
                    query.With<ContentItemIndex>(i => i.Published);
                    break;
                case ContentsStatus.Latest:
                    query.With<ContentItemIndex>(i => i.Latest);
                    break;
                case ContentsStatus.Draft:
                    query.With<ContentItemIndex>(i => !i.Published && i.Latest);
                    break;
                case ContentsStatus.Owner:
                    var currentUserName = _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);

                    if (currentUserName != null)
                    {
                        query.With<ContentItemIndex>(i => (i.Published || i.Latest) && i.Owner == currentUserName);
                    }

                    break;
                default:
                    throw new NotSupportedException("Unknown status filter.");
            }
        }

        private static Expression<Func<ContainedPartIndex, bool>> CreateOrderedContainedPartIndexFilter(int? before, int? after, string contentItemId)
        {
            if (before != null)
            {
                return x => x.Order < before && x.ListContentItemId == contentItemId;
            }

            if (after != null)
            {
                return x => x.Order > after && x.ListContentItemId == contentItemId;
            }

            return x => x.ListContentItemId == contentItemId;
        }
    }
}
