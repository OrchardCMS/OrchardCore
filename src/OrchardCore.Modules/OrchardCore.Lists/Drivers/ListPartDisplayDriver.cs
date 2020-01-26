using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.ContentManagement.Display.Models;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Lists.Models;
using OrchardCore.Lists.Services;
using OrchardCore.Lists.ViewModels;
using OrchardCore.Navigation;
using YesSql;

namespace OrchardCore.Lists.Drivers
{
    public class ListPartDisplayDriver : ContentPartDisplayDriver<ListPart>
    {
        private readonly IContentManager _contentManager;
        private readonly ISession _session;
        private readonly IServiceProvider _serviceProvider;
        private readonly IContentDefinitionManager _contentDefinitionManager;
        private readonly IContainerService _containerService;

        public ListPartDisplayDriver(
            IContentDefinitionManager contentDefinitionManager,
            IContentManager contentManager,
            ISession session,
            IServiceProvider serviceProvider,
            IContainerService containerService
            )
        {
            _contentDefinitionManager = contentDefinitionManager;
            _serviceProvider = serviceProvider;
            _session = session;
            _contentManager = contentManager;
            _containerService = containerService;
        }

        public override IDisplayResult Display(ListPart listPart, BuildPartDisplayContext context)
        {
            return
                Combine(
                    Initialize<ListPartViewModel>("ListPart", async model =>
                    {
                        var pager = await GetPagerAsync(context.Updater, listPart);
                        var settings = GetSettings(listPart);

                        model.ListPart = listPart;
                        model.ContentItems = (await _containerService.QueryContainedItemsAsync(listPart.ContentItem.ContentItemId, settings.EnableOrdering, pager, true)).ToArray();
                        model.ContainedContentTypeDefinitions = GetContainedContentTypes(listPart);
                        model.Context = context;
                        model.Pager = await context.New.PagerSlim(pager);
                    })
                    .Location("Detail", "Content:10"),
                    Initialize<ListPartViewModel>("ListPart", async model =>
                    {
                        var pager = await GetPagerAsync(context.Updater, listPart);
                        var settings = GetSettings(listPart);

                        model.ListPart = listPart;
                        model.ContentItems = (await _containerService.QueryContainedItemsAsync(listPart.ContentItem.ContentItemId, settings.EnableOrdering, pager, false)).ToArray();
                        model.ContainedContentTypeDefinitions = GetContainedContentTypes(listPart);
                        model.Context = context;
                        model.EnableOrdering = settings.EnableOrdering;
                        model.Pager = await context.New.PagerSlim(pager);
                    })
                    .Location("DetailAdmin", "Content:10")
                );
        }

        private async Task<PagerSlim> GetPagerAsync(IUpdateModel updater, ListPart part)
        {
            var settings = GetSettings(part);
            PagerSlimParameters pagerParameters = new PagerSlimParameters();
            await updater.TryUpdateModelAsync(pagerParameters);

            PagerSlim pager = new PagerSlim(pagerParameters, settings.PageSize);

            return pager;
        }

        private IEnumerable<ContentTypeDefinition> GetContainedContentTypes(ListPart listPart)
        {
            var settings = GetSettings(listPart);
            var contentTypes = settings.ContainedContentTypes ?? Enumerable.Empty<string>();
            return contentTypes.Select(contentType => _contentDefinitionManager.GetTypeDefinition(contentType));
        }

        private ListPartSettings GetSettings(ListPart listPart)
        {
            var contentTypeDefinition = _contentDefinitionManager.GetTypeDefinition(listPart.ContentItem.ContentType);
            var contentTypePartDefinition = contentTypeDefinition.Parts.FirstOrDefault(x => String.Equals(x.PartDefinition.Name, "ListPart"));
            return contentTypePartDefinition.GetSettings<ListPartSettings>();
        }
    }
}
