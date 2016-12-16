using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Display;
using Orchard.ContentManagement.Display.ContentDisplay;
using Orchard.ContentManagement.Display.Models;
using Orchard.ContentManagement.Metadata.Models;
using Orchard.ContentManagement.MetaData;
using Orchard.ContentManagement.Records;
using Orchard.DisplayManagement;
using Orchard.DisplayManagement.ModelBinding;
using Orchard.DisplayManagement.Views;
using Orchard.Lists.Indexes;
using Orchard.Lists.Models;
using Orchard.Navigation;
using YesSql.Core.Services;

namespace Orchard.Lists.Drivers
{
    public class ListPartDisplayDriver : ContentPartDisplayDriver<ListPart>
    {
        private readonly IContentManager _contentManager;
        private readonly ISession _session;
        private readonly IServiceProvider _serviceProvider;
        private readonly IContentDefinitionManager _contentDefinitionManager;

        public ListPartDisplayDriver(
            IContentDefinitionManager contentDefinitionManager,
            IContentManager contentManager,
            ISession session,
            IServiceProvider serviceProvider
            )
        {
            _contentDefinitionManager = contentDefinitionManager;
            _serviceProvider = serviceProvider;
            _session = session;
            _contentManager = contentManager;
        }

        public override IDisplayResult Display(ListPart listPart, BuildPartDisplayContext context)
        {
            return Combine(
                Shape("ListPart_DetailAdmin", async shape =>
                {
                    var pager = await GetPagerAsync(context.Updater);

                    var contentItemDisplayManager = _serviceProvider.GetService<IContentItemDisplayManager>();
                    var containedItems = await QueryListItemsAsync(listPart, pager);
                    var containedItemsSummaries = new List<dynamic>();

                    foreach (var contentItem in containedItems)
                    {
                        containedItemsSummaries.Add(await contentItemDisplayManager.BuildDisplayAsync(contentItem, context.Updater, "SummaryAdmin"));
                    }

                    shape.ContentItems = containedItemsSummaries;
                    shape.ContentItem = listPart.ContentItem;
                    shape.ContainedContentTypeDefinitions = GetContainedContentTypes(listPart);
                })
                .Location("DetailAdmin", "Content:10"),

                Shape("ListPart", async shape =>
                {
                    var pager = await GetPagerAsync(context.Updater);

                    var contentItemDisplayManager = _serviceProvider.GetService<IContentItemDisplayManager>();
                    var containedItems = await QueryListItemsAsync(listPart, pager);
                    var containedItemsSummaries = new List<dynamic>();
                    var listContentType = listPart.ContentItem.ContentType;

                    foreach (var contentItem in containedItems)
                    {
                        var itemShape = await contentItemDisplayManager.BuildDisplayAsync(contentItem, context.Updater, "Summary") as IShape;
                        itemShape.Metadata.Alternates.Add($"ListPart_Summary__{listContentType}");
                        containedItemsSummaries.Add(itemShape);
                    }

                    shape.ContentItems = containedItemsSummaries;
                    shape.ContentItem = listPart.ContentItem;
                    shape.ContainedContentTypeDefinitions = GetContainedContentTypes(listPart);
                    shape.Pager = context.New.PagerSlim(pager);
                })
                .Displaying(displaying =>
                {
                    var listContentType = listPart.ContentItem.ContentType;
                    displaying.Shape.Metadata.Alternates.Add($"ListPart__{listContentType}");
                })
                .Location("Detail", "Content:10")
            );
        }

        private async Task<PagerSlim> GetPagerAsync(IUpdateModel updater)
        {
            PagerSlimParameters pagerParameters = new PagerSlimParameters();
            await updater.TryUpdateModelAsync(pagerParameters);

            // TODO: Use list settings to define page size
            PagerSlim pager = new PagerSlim(pagerParameters, 10);

            return pager;
        }

        private async Task<IEnumerable<ContentItem>> QueryListItemsAsync(ListPart listPart, PagerSlim pager)
        {
            if (pager.Before != null)
            {
                var beforeValue = new DateTime(long.Parse(pager.Before));
                var query = _session.QueryAsync<ContentItem>()
                    .With<ContainedPartIndex>(x => x.ListContentItemId == listPart.ContentItem.ContentItemId)
                    .With<ContentItemIndex>(x => x.Published && x.CreatedUtc > beforeValue)
                    .OrderBy(x => x.CreatedUtc)
                    .Take(pager.PageSize + 1);

                var containedItems = await query.List();

                if (containedItems.Count() == 0)
                {
                    return containedItems;
                }

                containedItems = containedItems.Reverse();

                // There is always an After ras we clicked on Before
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
                var query = _session.QueryAsync<ContentItem>()
                    .With<ContainedPartIndex>(x => x.ListContentItemId == listPart.ContentItem.ContentItemId)
                    .With<ContentItemIndex>(x => x.Published && x.CreatedUtc < afterValue)
                    .OrderByDescending(x => x.CreatedUtc)
                    .Take(pager.PageSize + 1);

                var containedItems = await query.List();

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
                var query = _session.QueryAsync<ContentItem>()
                    .With<ContainedPartIndex>(x => x.ListContentItemId == listPart.ContentItem.ContentItemId)
                    .With<ContentItemIndex>(x => x.Published)
                    .OrderByDescending(x => x.CreatedUtc)
                    .Take(pager.PageSize + 1);

                var containedItems = await query.List();

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

        private IEnumerable<ContentTypeDefinition> GetContainedContentTypes(ListPart listPart)
        {
            var contentTypeDefinition = _contentDefinitionManager.GetTypeDefinition(listPart.ContentItem.ContentType);
            var contentTypePartDefinition = contentTypeDefinition.Parts.FirstOrDefault(x => String.Equals(x.PartDefinition.Name, "ListPart", StringComparison.Ordinal));
            var contentTypes = contentTypePartDefinition.Settings.ToObject<ListPartSettings>().ContainedContentTypes ?? Enumerable.Empty<string>();
            return contentTypes.Select(contentType => _contentDefinitionManager.GetTypeDefinition(contentType));
        }
    }
}