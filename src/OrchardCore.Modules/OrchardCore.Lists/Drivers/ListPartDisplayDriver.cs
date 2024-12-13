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

namespace OrchardCore.Lists.Drivers;

public sealed class ListPartDisplayDriver : ContentPartDisplayDriver<ListPart>
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
                InitializeEditListPartNavigationAdmin(part, context, settings),
                InitializeEditListPartHeaderAdmin(part, context, settings)
           );
    }

    public override IDisplayResult Display(ListPart listPart, BuildPartDisplayContext context)
    {
        var settings = context.TypePartDefinition.GetSettings<ListPartSettings>();

        return
            Combine(
                InitializeDisplayListPartDisplayShape(listPart, context),
                InitializeDisplayListPartDetailAdminShape(listPart, context),
                InitializeDisplayListPartNavigationAdminShape(listPart, context, settings),
                InitializeDisplayListPartDetailAdminSearchPanelShape(),
                InitializeDisplayListPartHeaderAdminShape(listPart, settings),
                InitializeDisplayListPartSummaryAdmin(listPart)
            );
    }

    private ShapeResult InitializeEditListPartHeaderAdmin(ListPart part, BuildPartEditorContext context, ListPartSettings settings)
    {
        return Initialize<ListPartHeaderAdminViewModel>("ListPartHeaderAdmin", async model =>
        {
            model.ContainerContentItem = part.ContentItem;
            model.ContainedContentTypeDefinitions = (await GetContainedContentTypesAsync(settings)).ToArray();
            model.EnableOrdering = settings.EnableOrdering;
        }).Location("Content:1")
        .RenderWhen(() => Task.FromResult(!context.IsNew && settings.ShowHeader));
    }

    private ShapeResult InitializeEditListPartNavigationAdmin(ListPart part, BuildPartEditorContext context, ListPartSettings settings)
    {
        return Initialize<ListPartNavigationAdminViewModel>("ListPartNavigationAdmin", async model =>
        {
            model.Container = part.ContentItem;
            model.ContainedContentTypeDefinitions = (await GetContainedContentTypesAsync(settings)).ToArray();
            model.EnableOrdering = settings.EnableOrdering;
            model.ContainerContentTypeDefinition = context.TypePartDefinition.ContentTypeDefinition;
        })
            .Location("Content:1.5")
            .RenderWhen(() => Task.FromResult(!context.IsNew));
    }

    private ShapeResult InitializeDisplayListPartSummaryAdmin(ListPart listPart)
    {
        return Initialize<ContentItemViewModel>("ListPartSummaryAdmin", model => model.ContentItem = listPart.ContentItem)
            .Location("SummaryAdmin", "Actions:4");
    }

    private ShapeResult InitializeDisplayListPartHeaderAdminShape(ListPart listPart, ListPartSettings settings)
    {
        return Initialize<ListPartHeaderAdminViewModel>("ListPartHeaderAdmin", async model =>
        {
            model.ContainerContentItem = listPart.ContentItem;
            model.ContainedContentTypeDefinitions = (await GetContainedContentTypesAsync(settings)).ToArray();
            model.EnableOrdering = settings.EnableOrdering;
        }).Location("DetailAdmin", "Content:1")
        .RenderWhen(() => Task.FromResult(settings.ShowHeader));
    }

    private ShapeResult InitializeDisplayListPartNavigationAdminShape(ListPart listPart, BuildPartDisplayContext context, ListPartSettings settings)
    {
        return Initialize<ListPartNavigationAdminViewModel>("ListPartNavigationAdmin", async model =>
        {
            model.ContainedContentTypeDefinitions = (await GetContainedContentTypesAsync(settings)).ToArray();
            model.Container = listPart.ContentItem;
            model.EnableOrdering = settings.EnableOrdering;
            model.ContainerContentTypeDefinition = context.TypePartDefinition.ContentTypeDefinition;
        }).Location("DetailAdmin", "Content:1.5");
    }

    private ShapeResult InitializeDisplayListPartDetailAdminShape(ListPart listPart, BuildPartDisplayContext context)
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

            model.ContainedContentTypeDefinitions = await GetContainedContentTypesAsync(settings);
            model.Context = context;
            model.EnableOrdering = settings.EnableOrdering;
            model.Pager = await context.New.PagerSlim(pager);
        }))
            .Location("DetailAdmin", "Content:10");
    }
    private ShapeResult InitializeDisplayListPartDetailAdminSearchPanelShape()
    {
        return Initialize<ListPartViewModel>("ListPartDetailAdminSearchPanel", async model =>
        {
            var listPartFilterViewModel = new ListPartFilterViewModel();
            await _updateModelAccessor.ModelUpdater.TryUpdateModelAsync(listPartFilterViewModel, Prefix);

            model.ListPartFilterViewModel = listPartFilterViewModel;

        }).Location("DetailAdmin", "Content:5");
    }

    private ShapeResult InitializeDisplayListPartDisplayShape(ListPart listPart, BuildPartDisplayContext context)
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

            model.ContainedContentTypeDefinitions = await GetContainedContentTypesAsync(settings);
            model.Context = context;
            model.Pager = await context.New.PagerSlim(pager);
            model.ListPart = listPart;
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

    private async Task<IEnumerable<ContentTypeDefinition>> GetContainedContentTypesAsync(ListPartSettings settings)
    {
        var contentTypes = settings.ContainedContentTypes ?? [];

        var definitions = new List<ContentTypeDefinition>();

        foreach (var contentType in contentTypes)
        {
            var definition = await _contentDefinitionManager.GetTypeDefinitionAsync(contentType);

            if (definition == null)
            {
                continue;
            }

            definitions.Add(definition);
        }

        return definitions;
    }
}
