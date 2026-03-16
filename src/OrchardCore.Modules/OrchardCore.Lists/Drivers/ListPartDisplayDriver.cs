using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.ContentManagement.Display.Models;
using OrchardCore.ContentManagement.Display.ViewModels;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.ContentManagement.Records;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Lists.Indexes;
using OrchardCore.Lists.Models;
using OrchardCore.Lists.Services;
using OrchardCore.Lists.ViewModels;
using OrchardCore.Navigation;
using YesSql;
using Microsoft.AspNetCore.Http;

using ISession = YesSql.ISession;

namespace OrchardCore.Lists.Drivers;

public sealed class ListPartDisplayDriver : ContentPartDisplayDriver<ListPart>
{
    private readonly IContentDefinitionManager _contentDefinitionManager;
    private readonly IContainerService _containerService;
    private readonly IUpdateModelAccessor _updateModelAccessor;
    private readonly ISession _session;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public ListPartDisplayDriver(
        IContentDefinitionManager contentDefinitionManager,
        IContainerService containerService,
        IUpdateModelAccessor updateModelAccessor,
        ISession session,
        IHttpContextAccessor httpContextAccessor)
    {
        _contentDefinitionManager = contentDefinitionManager;
        _containerService = containerService;
        _updateModelAccessor = updateModelAccessor;
        _session = session;
        _httpContextAccessor = httpContextAccessor;
    }

    public override IDisplayResult Edit(ListPart part, BuildPartEditorContext context)
    {
        var settings = context.TypePartDefinition.GetSettings<ListPartSettings>();

        return Combine(
            InitializeEditListPartNavigationAdmin(part, context, settings),
            InitializeEditListPartHeaderAdmin(part, context, settings)
        );
    }

    public override IDisplayResult Display(ListPart listPart, BuildPartDisplayContext context)
    {
        var settings = context.TypePartDefinition.GetSettings<ListPartSettings>();

        return Combine(
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
            .Location(OrchardCoreConstants.DisplayType.SummaryAdmin, "Actions:4");
    }

    private ShapeResult InitializeDisplayListPartHeaderAdminShape(ListPart listPart, ListPartSettings settings)
    {
        return Initialize<ListPartHeaderAdminViewModel>("ListPartHeaderAdmin", async model =>
        {
            model.ContainerContentItem = listPart.ContentItem;
            model.ContainedContentTypeDefinitions = (await GetContainedContentTypesAsync(settings)).ToArray();
            model.EnableOrdering = settings.EnableOrdering;
        }).Location(OrchardCoreConstants.DisplayType.DetailAdmin, "Content:1")
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
        }).Location(OrchardCoreConstants.DisplayType.DetailAdmin, "Content:1.5");
    }

    private ShapeResult InitializeDisplayListPartDetailAdminSearchPanelShape()
    {
        return Initialize<ListPartViewModel>("ListPartDetailAdminSearchPanel", async model =>
        {
            var listPartFilterViewModel = new ListPartFilterViewModel();
            await _updateModelAccessor.ModelUpdater.TryUpdateModelAsync(listPartFilterViewModel, Prefix);

            model.ListPartFilterViewModel = listPartFilterViewModel;

        }).Location(OrchardCoreConstants.DisplayType.DetailAdmin, "Content:5");
    }


    private ShapeResult InitializeDisplayListPartDetailAdminShape(ListPart listPart, BuildPartDisplayContext context)
    {
        return Initialize("ListPartDetailAdmin", (Func<ListPartViewModel, ValueTask>)(async model =>
        {
            var settings = context.TypePartDefinition.GetSettings<ListPartSettings>();
            var containedItemOptions = new ContainedItemOptions();
            var listPartFilterViewModel = new ListPartFilterViewModel();

            await _updateModelAccessor.ModelUpdater.TryUpdateModelAsync(listPartFilterViewModel, Prefix);
            containedItemOptions.DisplayText = listPartFilterViewModel.DisplayText;
            containedItemOptions.Status = listPartFilterViewModel.Status;

            model.ListPart = listPart;
            model.ListPartFilterViewModel = listPartFilterViewModel;
            model.ContainedContentTypeDefinitions = await GetContainedContentTypesAsync(settings);
            model.Context = context;
            model.EnableOrdering = settings.EnableOrdering;

            if (settings.ShowPageNumbers)
            {
                var pager = await GetPagerAsync(context);

                model.ContentItems = (await _containerService.QueryContainedItemsAsync(
                    listPart.ContentItem.ContentItemId,
                    settings.EnableOrdering,
                    pager,
                    containedItemOptions)).ToArray();

                var query = BuildTotalItemCountQuery(listPart.ContentItem.ContentItemId, containedItemOptions);

                var totalItemCount = await query.CountAsync();

                model.Pager = (await context.New.Pager(pager)).TotalItemCount(totalItemCount);
            }
            else
            {
                var pagerSlim = await GetPagerSlimAsync(context);

                model.ContentItems = (await _containerService.QueryContainedItemsAsync(
                    listPart.ContentItem.ContentItemId,
                    settings.EnableOrdering,
                    pagerSlim,
                    containedItemOptions)).ToArray();

                model.Pager = await context.New.PagerSlim(pagerSlim);
            }
        }))
        .Location(OrchardCoreConstants.DisplayType.DetailAdmin, "Content:10");
    }

    private ShapeResult InitializeDisplayListPartDisplayShape(ListPart listPart, BuildPartDisplayContext context)
    {
        return Initialize<ListPartViewModel>(GetDisplayShapeType(context), async model =>
        {
            var settings = context.TypePartDefinition.GetSettings<ListPartSettings>();
            var containedItemOptions = new ContainedItemOptions(); 

            model.ContainedContentTypeDefinitions = await GetContainedContentTypesAsync(settings);
            model.Context = context;
            model.ListPart = listPart;

            if (settings.ShowPageNumbers)
            {
                var pager = await GetPagerAsync(context);

                model.ContentItems = (await _containerService.QueryContainedItemsAsync(
                    listPart.ContentItem.ContentItemId,
                    settings.EnableOrdering,
                    pager,
                    containedItemOptions)).ToArray();

                containedItemOptions.Status = ContentsStatus.Published;
                var query = BuildTotalItemCountQuery(listPart.ContentItem.ContentItemId, containedItemOptions);
                var totalItemCount = await query.CountAsync();

                model.Pager = (await context.New.Pager(pager)).TotalItemCount(totalItemCount);
            }
            else
            {
                var pagerSlim = await GetPagerSlimAsync(context);

                model.ContentItems = (await _containerService.QueryContainedItemsAsync(
                    listPart.ContentItem.ContentItemId,
                    settings.EnableOrdering,
                    pagerSlim,
                    containedItemOptions)).ToArray();

                model.Pager = await context.New.PagerSlim(pagerSlim);
            }
        })
        .Location(OrchardCoreConstants.DisplayType.Detail, "Content:10");
    }

    /// <summary>
    /// Builds a query that retrieves content items associated with a specified list content item ID, filtered according
    /// to the provided options.
    /// </summary>
    private IQuery<ContentItem> BuildTotalItemCountQuery(string listContentItemId, ContainedItemOptions options)
    {
        IQuery<ContentItem> query = _session.Query<ContentItem>()
            .With<ContainedPartIndex>(x => x.ListContentItemId == listContentItemId);

        if (options.Status == ContentsStatus.Published)
        {
            query = query.With<ContentItemIndex>(x => x.Published);
        }
        else if (options.Status == ContentsStatus.Latest)
        {
            query = query.With<ContentItemIndex>(x => x.Latest);
        }
        else if (options.Status == ContentsStatus.Draft)
        {
            query = query.With<ContentItemIndex>(x => x.Latest && !x.Published);
        }
        else if (options.Status == ContentsStatus.Owner)
        {
            var currentUserName = _httpContextAccessor.HttpContext?.User?.Identity?.Name;

            if (!string.IsNullOrEmpty(currentUserName))
            {
                query = query.With<ContentItemIndex>(x => x.Latest && x.Author == currentUserName);
            }
            else
            {
                query = query.With<ContentItemIndex>(x => x.Latest);
            }
        }

        if (!string.IsNullOrWhiteSpace(options.DisplayText))
        {
            query = query.With<ContentItemIndex>(x => x.DisplayText.Contains(options.DisplayText));
        }

        return query;
    }

    private static async Task<PagerSlim> GetPagerSlimAsync(BuildPartDisplayContext context)
    {
        var settings = context.TypePartDefinition.GetSettings<ListPartSettings>();
        var pagerParameters = new PagerSlimParameters();
        await context.Updater.TryUpdateModelAsync(pagerParameters);

        var pager = new PagerSlim(pagerParameters, settings.PageSize);
        return pager;
    }

    private static async Task<Pager> GetPagerAsync(BuildPartDisplayContext context)
    {
        var settings = context.TypePartDefinition.GetSettings<ListPartSettings>();
        var pagerParameters = new PagerParameters();
        await context.Updater.TryUpdateModelAsync(pagerParameters);

        var pager = new Pager(pagerParameters, settings.PageSize);
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
