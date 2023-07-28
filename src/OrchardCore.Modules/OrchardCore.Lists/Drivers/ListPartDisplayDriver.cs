using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.ContentManagement.Display.Models;
using OrchardCore.ContentManagement.Display.ViewModels;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Lists.Models;
using OrchardCore.Lists.Services;
using OrchardCore.Lists.ViewModels;
using OrchardCore.Navigation;

namespace OrchardCore.Lists.Drivers
{
    public class ListPartDisplayDriver : ContentPartDisplayDriver<ListPart>
    {
        private readonly IContentDefinitionManager _contentDefinitionManager;
        private readonly IContainerService _containerService;
        private readonly IUpdateModelAccessor _updateModelAccessor;

        public ListPartDisplayDriver(
            IContentDefinitionManager contentDefinitionManager,
            IContainerService containerService,
            IUpdateModelAccessor updateModelAccessor
            )
        {
            _contentDefinitionManager = contentDefinitionManager;
            _containerService = containerService;
            _updateModelAccessor = updateModelAccessor;
        }

        public override IDisplayResult Edit(ListPart part, BuildPartEditorContext context)
        {
            var settings = context.TypePartDefinition.GetSettings<ListPartSettings>();

            return
                Combine(
                    InitilizeEditListPartNavigationAdmin(part, context, settings),
                    InitilizeEditListPartHeaderAdmin(part, context, settings)
               );
        }

        public override IDisplayResult Display(ListPart listPart, BuildPartDisplayContext context)
        {
            var settings = context.TypePartDefinition.GetSettings<ListPartSettings>();

            return
                Combine(
                    InitilizeDisplayListPartDisplayShape(listPart, context),
                    InitilizeDisplayListPartDetailAdminShape(listPart, context),
                    InitilizeDisplayListPartNavigationAdminShape(listPart, context, settings),
                    InitilizeDisplayListPartHeaderAdminShape(listPart, settings),
                    InitilizeDisplayListPartSummaryAdmin(listPart)
                );
        }

        private ShapeResult InitilizeEditListPartHeaderAdmin(ListPart part, BuildPartEditorContext context, ListPartSettings settings)
        {
            return Initialize("ListPartHeaderAdmin", (Action<ListPartHeaderAdminViewModel>)(model =>
            {
                model.ContainerContentItem = part.ContentItem;
                model.ContainedContentTypeDefinitions = GetContainedContentTypes(settings).ToArray();
                model.EnableOrdering = settings.EnableOrdering;
            }))
                .Location("Content:1")
                .RenderWhen(() => Task.FromResult(!context.IsNew && settings.ShowHeader));
        }

        private ShapeResult InitilizeEditListPartNavigationAdmin(ListPart part, BuildPartEditorContext context, ListPartSettings settings)
        {
            return Initialize<ListPartNavigationAdminViewModel>("ListPartNavigationAdmin", model =>
            {
                model.Container = part.ContentItem;
                model.ContainedContentTypeDefinitions = GetContainedContentTypes(settings).ToArray();
                model.EnableOrdering = settings.EnableOrdering;
                model.ContainerContentTypeDefinition = context.TypePartDefinition.ContentTypeDefinition;
            })
                .Location("Content:1.5")
                .RenderWhen(() => Task.FromResult(!context.IsNew));
        }

        private ShapeResult InitilizeDisplayListPartSummaryAdmin(ListPart listPart)
        {
            return Initialize("ListPartSummaryAdmin", (Action<ContentItemViewModel>)(model => model.ContentItem = listPart.ContentItem))
                .Location("SummaryAdmin", "Actions:4");
        }

        private ShapeResult InitilizeDisplayListPartHeaderAdminShape(ListPart listPart, ListPartSettings settings)
        {
            return Initialize("ListPartHeaderAdmin", (Action<ListPartHeaderAdminViewModel>)(model =>
            {
                model.ContainerContentItem = listPart.ContentItem;
                model.ContainedContentTypeDefinitions = GetContainedContentTypes(settings).ToArray();
                model.EnableOrdering = settings.EnableOrdering;
            }))
                .Location("DetailAdmin", "Content:1")
                .RenderWhen(() => Task.FromResult(settings.ShowHeader));
        }

        private ShapeResult InitilizeDisplayListPartNavigationAdminShape(ListPart listPart, BuildPartDisplayContext context, ListPartSettings settings)
        {
            return Initialize("ListPartNavigationAdmin", (Action<ListPartNavigationAdminViewModel>)(model =>
            {
                model.ContainedContentTypeDefinitions = GetContainedContentTypes(settings).ToArray();
                model.Container = listPart.ContentItem;
                model.EnableOrdering = settings.EnableOrdering;
                model.ContainerContentTypeDefinition = context.TypePartDefinition.ContentTypeDefinition;
            }))
                .Location("DetailAdmin", "Content:1.5");
        }

        private ShapeResult InitilizeDisplayListPartDetailAdminShape(ListPart listPart, BuildPartDisplayContext context)
        {
            return Initialize("ListPartDetailAdmin", (Func<ListPartViewModel, ValueTask>)(async model =>
            {
                var pager = await GetPagerSlimAsync(context);
                var settings = context.TypePartDefinition.GetSettings<ListPartSettings>();
                var listPartFilterViewModel = new ListPartFilterViewModel();
                var containedItemOptions = new ContainedItemOptions();

                await _updateModelAccessor.ModelUpdater.TryUpdateModelAsync(listPartFilterViewModel, Prefix);
                model.ListPart = listPart;
                containedItemOptions.DisplayText = listPartFilterViewModel.DisplayText;
                containedItemOptions.Status = listPartFilterViewModel.Status;
                model.ListPartFilterViewModel = listPartFilterViewModel;

                model.ContentItems = (await _containerService.QueryContainedItemsAsync(
                    listPart.ContentItem.ContentItemId,
                    settings.EnableOrdering,
                    pager,
                    containedItemOptions)).ToArray();

                model.ContainedContentTypeDefinitions = GetContainedContentTypes(settings);
                model.Context = context;
                model.EnableOrdering = settings.EnableOrdering;
                model.Pager = await context.New.PagerSlim(pager);
            }))
                .Location("DetailAdmin", "Content:10");
        }

        private ShapeResult InitilizeDisplayListPartDisplayShape(ListPart listPart, BuildPartDisplayContext context)
        {
            return Initialize<ListPartViewModel>(GetDisplayShapeType(context), async model =>
            {
                var pager = await GetPagerSlimAsync(context);
                var settings = context.TypePartDefinition.GetSettings<ListPartSettings>();
                var containedItemOptions = new ContainedItemOptions();
                model.ContentItems = (await _containerService.QueryContainedItemsAsync(
                    listPart.ContentItem.ContentItemId,
                    settings.EnableOrdering,
                    pager,
                    containedItemOptions)).ToArray();

                model.ContainedContentTypeDefinitions = GetContainedContentTypes(settings);
                model.Context = context;
                model.Pager = await context.New.PagerSlim(pager);
            })
                .Location("Detail", "Content:10");
        }

        private static async Task<PagerSlim> GetPagerSlimAsync(BuildPartDisplayContext context)
        {
            var settings = context.TypePartDefinition.GetSettings<ListPartSettings>();
            var pagerParameters = new PagerSlimParameters();
            await context.Updater.TryUpdateModelAsync(pagerParameters);

            var pager = new PagerSlim(pagerParameters, settings.PageSize);

            return pager;
        }

        private IEnumerable<ContentTypeDefinition> GetContainedContentTypes(ListPartSettings settings)
        {
            var contentTypes = settings.ContainedContentTypes ?? Enumerable.Empty<string>();

            return contentTypes.Select(contentType => _contentDefinitionManager.GetTypeDefinition(contentType));
        }
    }
}
