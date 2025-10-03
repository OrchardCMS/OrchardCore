using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Localization;
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
    internal readonly IStringLocalizer S;
    private readonly IUpdateModelAccessor _updateModelAccessor;

    public ListPartDisplayDriver(
        IContentDefinitionManager contentDefinitionManager,
        IContainerService containerService,
        IStringLocalizer<ListPartDisplayDriver> stringLocalizer,
        IUpdateModelAccessor updateModelAccessor
        )
    {
        _contentDefinitionManager = contentDefinitionManager;
        _containerService = containerService;
        _updateModelAccessor = updateModelAccessor;
        S = stringLocalizer;
    }

    public override async Task<IDisplayResult> EditAsync(ListPart part, BuildPartEditorContext context)
    {
        var settings = context.TypePartDefinition.GetSettings<ListPartSettings>();
        var containedContentTypes = await GetContainedContentTypesAsync(settings);

        return
            Combine(
                InitializeEditListPartNavigationAdmin(part, context, settings, containedContentTypes),
                InitializeEditListPartHeaderAdmin(part, context, settings, containedContentTypes)
           );
    }

    public override async Task<IDisplayResult> DisplayAsync(ListPart listPart, BuildPartDisplayContext context)
    {
        var settings = context.TypePartDefinition.GetSettings<ListPartSettings>();
        var containedContentTypes = await GetContainedContentTypesAsync(settings);

        return
            Combine(
                InitializeDisplayListPartDisplayShape(listPart, context, containedContentTypes),
                InitializeDisplayListPartDetailAdminShape(listPart, context, containedContentTypes),
                InitializeDisplayListPartNavigationAdminShape(listPart, context, settings, containedContentTypes),
                InitializeDisplayListPartDetailAdminSearchPanelShape(containedContentTypes),
                InitializeDisplayListPartHeaderAdminShape(listPart, settings, containedContentTypes),
                InitializeDisplayListPartSummaryAdmin(listPart)
            );
    }

    private ShapeResult InitializeEditListPartHeaderAdmin(ListPart part, BuildPartEditorContext context, ListPartSettings settings, IEnumerable<ContentTypeDefinition> containedContentTypes)
    {
        return Initialize<ListPartHeaderAdminViewModel>("ListPartHeaderAdmin", model =>
        {
            model.ContainerContentItem = part.ContentItem;
            model.ContainedContentTypeDefinitions = containedContentTypes.ToArray();
            model.EnableOrdering = settings.EnableOrdering;
        })
        .Location("Content:1")
        .RenderWhen(() => Task.FromResult(!context.IsNew && settings.ShowHeader));
    }

    private ShapeResult InitializeEditListPartNavigationAdmin(ListPart part, BuildPartEditorContext context, ListPartSettings settings, IEnumerable<ContentTypeDefinition> containedContentTypes)
    {
        return Initialize<ListPartNavigationAdminViewModel>("ListPartNavigationAdmin", model =>
        {
            model.Container = part.ContentItem;
            model.ContainedContentTypeDefinitions = containedContentTypes.ToArray();
            model.EnableOrdering = settings.EnableOrdering;
            model.ContainerContentTypeDefinition = context.TypePartDefinition.ContentTypeDefinition;
        })
        .Location("Content:1.5")
        .RenderWhen(() => Task.FromResult(!context.IsNew));
    }

    private ShapeResult InitializeDisplayListPartSummaryAdmin(ListPart listPart)
    {
        return Initialize<ContentItemViewModel>("ListPartSummaryAdmin", model => model.ContentItem = listPart.ContentItem)
            .Location(OrchardCoreConstants.DisplayType.SummaryAdmin, "Actions:4");
    }

    private ShapeResult InitializeDisplayListPartHeaderAdminShape(ListPart listPart, ListPartSettings settings, IEnumerable<ContentTypeDefinition> containedContentTypes)
    {
        return Initialize<ListPartHeaderAdminViewModel>("ListPartHeaderAdmin", model =>
        {
            model.ContainerContentItem = listPart.ContentItem;
            model.ContainedContentTypeDefinitions = containedContentTypes.ToArray();
            model.EnableOrdering = settings.EnableOrdering;
        })
        .Location(OrchardCoreConstants.DisplayType.DetailAdmin, "Content:1")
        .RenderWhen(() => Task.FromResult(settings.ShowHeader));
    }

    private ShapeResult InitializeDisplayListPartNavigationAdminShape(ListPart listPart, BuildPartDisplayContext context, ListPartSettings settings, IEnumerable<ContentTypeDefinition> containedContentTypes)
    {
        return Initialize<ListPartNavigationAdminViewModel>("ListPartNavigationAdmin", model =>
        {
            model.ContainedContentTypeDefinitions = containedContentTypes.ToArray();
            model.Container = listPart.ContentItem;
            model.EnableOrdering = settings.EnableOrdering;
            model.ContainerContentTypeDefinition = context.TypePartDefinition.ContentTypeDefinition;
        }).Location(OrchardCoreConstants.DisplayType.DetailAdmin, "Content:1.5");
    }

    private ShapeResult InitializeDisplayListPartDetailAdminShape(ListPart listPart, BuildPartDisplayContext context, IEnumerable<ContentTypeDefinition> containedContentTypes)
    {
        return Initialize("ListPartDetailAdmin", (Func<ListPartViewModel, ValueTask>)(async model =>
        {
            var pager = await GetPagerSlimAsync(context);
            var settings = context.TypePartDefinition.GetSettings<ListPartSettings>();
            var containedItemOptions = new ContainedItemOptions();
            var listPartFilterViewModel = new ListPartFilterViewModel();

            await _updateModelAccessor.ModelUpdater.TryUpdateModelAsync(listPartFilterViewModel, Prefix);

            containedItemOptions.DisplayText = listPartFilterViewModel.DisplayText;
            containedItemOptions.Status = listPartFilterViewModel.Status;
            containedItemOptions.ContentType = listPartFilterViewModel.ContentType;

            listPartFilterViewModel.ContentTypeOptions = GetContainedContentTypesList(containedContentTypes, listPartFilterViewModel.ContentType);

            model.ListPartFilterViewModel = listPartFilterViewModel;
            model.ListPart = listPart;

            model.ContentItems = (await _containerService.QueryContainedItemsAsync(
                listPart.ContentItem.ContentItemId,
                settings.EnableOrdering,
                pager,
                containedItemOptions)).ToArray();

            model.ContainedContentTypeDefinitions = containedContentTypes;
            model.Context = context;
            model.EnableOrdering = settings.EnableOrdering;
            model.Pager = await context.New.PagerSlim(pager);
        }))
            .Location(OrchardCoreConstants.DisplayType.DetailAdmin, "Content:10");
    }
    private ShapeResult InitializeDisplayListPartDetailAdminSearchPanelShape(IEnumerable<ContentTypeDefinition> containedContentTypes)
    {
        return Initialize<ListPartViewModel>("ListPartDetailAdminSearchPanel", async model =>
        {
            var listPartFilterViewModel = new ListPartFilterViewModel();

            await _updateModelAccessor.ModelUpdater.TryUpdateModelAsync(listPartFilterViewModel, Prefix);

            listPartFilterViewModel.ContentTypeOptions = GetContainedContentTypesList(containedContentTypes, listPartFilterViewModel.ContentType);

            model.ListPartFilterViewModel = listPartFilterViewModel;

        }).Location(OrchardCoreConstants.DisplayType.DetailAdmin, "Content:5");
    }

    private ShapeResult InitializeDisplayListPartDisplayShape(ListPart listPart, BuildPartDisplayContext context, IEnumerable<ContentTypeDefinition> containedContentTypes)
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

            model.ContainedContentTypeDefinitions = containedContentTypes;
            model.Context = context;
            model.Pager = await context.New.PagerSlim(pager);
            model.ListPart = listPart;
        }).Location(OrchardCoreConstants.DisplayType.Detail, "Content:10");
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

    private List<SelectListItem> GetContainedContentTypesList(IEnumerable<ContentTypeDefinition> definitions, string selectedContentType)
    {
        var items = new List<SelectListItem>()
        {
            new SelectListItem(S["All content types"], string.Empty)
        };

        foreach (var definition in definitions)
        {
            items.Add(new SelectListItem(definition.DisplayName, definition.Name, string.Equals(definition.Name, selectedContentType, StringComparison.Ordinal)));
        }

        return items;
    }
}
