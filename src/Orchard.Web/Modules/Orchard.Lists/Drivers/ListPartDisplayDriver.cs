using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Display;
using Orchard.ContentManagement.Display.ContentDisplay;
using Orchard.DisplayManagement;
using Orchard.DisplayManagement.ModelBinding;
using Orchard.DisplayManagement.Views;
using Orchard.Lists.Indexes;
using Orchard.Lists.Models;
using YesSql.Core.Services;

namespace Orchard.Lists.Drivers
{
    public class ListPartDisplayDriver : ContentPartDisplayDriver<ListPart>
    {
        private readonly IContentManager _contentManager;
        private readonly ISession _session;
        private readonly IServiceProvider _serviceProvider;

        public ListPartDisplayDriver(
            IContentManager contentManager,
            ISession session,
            IServiceProvider serviceProvider
            )
        {
            _serviceProvider = serviceProvider;
            _session = session;
            _contentManager = contentManager;
        }

        public override IDisplayResult Display(ListPart listPart, IUpdateModel updater)
        {
            return Combine(
                Shape("ListPart_DetailAdmin", async shape =>
                {
                    var contentItemDisplayManager = _serviceProvider.GetService<IContentItemDisplayManager>();
                    var containedItems = await QueryListItems(listPart);
                    var containedItemsSummaries = new List<dynamic>();

                    foreach (var contentItem in containedItems)
                    {
                        containedItemsSummaries.Add(await contentItemDisplayManager.BuildDisplayAsync(contentItem, updater, "SummaryAdmin"));
                    }

                    shape.ContentItems = containedItemsSummaries;
                    shape.ContentItem = listPart.ContentItem;
                })
                .Location("DetailAdmin", "Content:10"),

                Shape("ListPart_Detail", async shape =>
                {
                    var contentItemDisplayManager = _serviceProvider.GetService<IContentItemDisplayManager>();
                    var containedItems = await QueryListItems(listPart);
                    var containedItemsSummaries = new List<dynamic>();

                    foreach (var contentItem in containedItems)
                    {
                        var itemShape = await contentItemDisplayManager.BuildDisplayAsync(contentItem, updater, "Summary") as IShape;
                        itemShape.Metadata.Alternates.Add("List_Summary__" + listPart.ContentItem.ContentType);
                        containedItemsSummaries.Add(itemShape);
                    }

                    shape.ContentItems = containedItemsSummaries;
                    shape.ContentItem = listPart.ContentItem;
                })
                .Location("Detail", "Content:10")
            );
        }

        private async Task<IEnumerable<ContentItem>> QueryListItems(ListPart listPart)
        {
            var query = _session.QueryAsync<ContentItem, ContainedPartIndex>(x => x.ListContentItemId == listPart.ContentItem.ContentItemId);
            var containedItems = await query.List();
            return containedItems;
        }
    }
}