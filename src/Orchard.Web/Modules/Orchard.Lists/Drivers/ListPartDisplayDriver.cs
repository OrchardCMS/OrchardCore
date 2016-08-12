using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Display;
using Orchard.ContentManagement.Display.ContentDisplay;
using Orchard.ContentManagement.Display.Models;
using Orchard.ContentManagement.MetaData;
using Orchard.ContentManagement.MetaData.Models;
using Orchard.ContentManagement.Records;
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
                    var contentItemDisplayManager = _serviceProvider.GetService<IContentItemDisplayManager>();
                    var containedItems = await QueryListItems(listPart);
                    var containedItemsSummaries = new List<dynamic>();

                    foreach (var contentItem in containedItems)
                    {
                        containedItemsSummaries.Add(await contentItemDisplayManager.BuildDisplayAsync(contentItem, context.Updater, "SummaryAdmin"));
                    }

                    shape.ContentItems = containedItemsSummaries;
                    shape.ContentItem = listPart.ContentItem;
                    shape.ContainedContentTypeDefinition = GetContainedContentType(listPart);
                })
                .Location("DetailAdmin", "Content:10"),

                Shape("ListPart_Detail", async shape =>
                {
                    var contentItemDisplayManager = _serviceProvider.GetService<IContentItemDisplayManager>();
                    var containedItems = await QueryListItems(listPart);
                    var containedItemsSummaries = new List<dynamic>();

                    foreach (var contentItem in containedItems)
                    {
                        var itemShape = await contentItemDisplayManager.BuildDisplayAsync(contentItem, context.Updater, "Summary") as IShape;
                        itemShape.Metadata.Alternates.Add("List_Summary__" + listPart.ContentItem.ContentType);
                        containedItemsSummaries.Add(itemShape);
                    }

                    shape.ContentItems = containedItemsSummaries;
                    shape.ContentItem = listPart.ContentItem;
                    shape.ContainedContentTypeDefinition = GetContainedContentType(listPart);
                })
                .Location("Detail", "Content:10")
            );
        }

        private async Task<IEnumerable<ContentItem>> QueryListItems(ListPart listPart)
        {
            var query = _session.QueryAsync<ContentItem>()
                .With<ContainedPartIndex>(x => x.ListContentItemId == listPart.ContentItem.ContentItemId)
                .With<ContentItemIndex>(x => x.Published)
                .OrderByDescending(x => x.CreatedUtc);

            var containedItems = await query.List();
            return containedItems;
        }

        private ContentTypeDefinition GetContainedContentType(ListPart listPart)
        {
            var contentTypeDefinition = _contentDefinitionManager.GetTypeDefinition(listPart.ContentItem.ContentType);
            var contentTypePartDefinition = contentTypeDefinition.Parts.FirstOrDefault(x => String.Equals(x.PartDefinition.Name, "ListPart", StringComparison.Ordinal));
            var contentType = contentTypePartDefinition.Settings.ToObject<ListPartSettings>().ContainedContentType;
            return _contentDefinitionManager.GetTypeDefinition(contentType);
        }
    }
}