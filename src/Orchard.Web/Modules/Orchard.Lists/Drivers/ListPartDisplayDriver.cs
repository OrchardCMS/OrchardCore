using Orchard.ContentManagement;
using Orchard.ContentManagement.Display;
using Orchard.ContentManagement.Display.ContentDisplay;
using Orchard.DisplayManagement.Views;
using Orchard.Lists.Indexes;
using Orchard.Lists.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using YesSql.Core.Services;
using Microsoft.Extensions.DependencyInjection;
using Orchard.DisplayManagement.ModelBinding;

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

                    var query = _session.QueryAsync<ContentItem, ContainedPartIndex>(x => x.ListContentItemId == listPart.ContentItem.ContentItemId);

                    var containedItemsSummaries = new List<dynamic>();
                    var containedItems = await query.List();
                    foreach (var contentItem in containedItems)
                    {
                        containedItemsSummaries.Add(await contentItemDisplayManager.BuildDisplayAsync(contentItem, updater, "SummaryAdmin"));
                    }

                    shape.ContentItems = containedItemsSummaries;
                    shape.ContentItem = listPart.ContentItem;
                })
                .Location("DetailAdmin", "Content")
            );
        }
    }
}