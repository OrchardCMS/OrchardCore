using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using OrchardCore.Admin;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Display;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.ContentManagement.Records;
using OrchardCore.Contents.Services;
using OrchardCore.Contents.ViewModels;
using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Notify;
using OrchardCore.Navigation;
using OrchardCore.Routing;
using OrchardCore.Security.Permissions;
using YesSql;
using YesSql.Filters.Query;
using YesSql.Services;

namespace OrchardCore.Contents.Controllers;

public sealed class AdminController : Controller, IUpdateModel
{
    private readonly IAuthorizationService _authorizationService;
    private readonly IContentManager _contentManager;
    private readonly IContentItemDisplayManager _contentItemDisplayManager;
    private readonly IContentDefinitionManager _contentDefinitionManager;
    private readonly IDisplayManager<ContentOptionsViewModel> _contentOptionsDisplayManager;
    private readonly ISession _session;
    private readonly INotifier _notifier;

    internal readonly IHtmlLocalizer H;
    internal readonly IStringLocalizer S;

    public AdminController(
        IAuthorizationService authorizationService,
        IContentManager contentManager,
        IContentItemDisplayManager contentItemDisplayManager,
        IContentDefinitionManager contentDefinitionManager,
        IDisplayManager<ContentOptionsViewModel> contentOptionsDisplayManager,
        ISession session,
        INotifier notifier,
        IHtmlLocalizer<AdminController> htmlLocalizer,
        IStringLocalizer<AdminController> stringLocalizer)
    {
        _authorizationService = authorizationService;
        _contentManager = contentManager;
        _contentItemDisplayManager = contentItemDisplayManager;
        _contentDefinitionManager = contentDefinitionManager;
        _contentOptionsDisplayManager = contentOptionsDisplayManager;
        _session = session;
        _notifier = notifier;

        H = htmlLocalizer;
        S = stringLocalizer;
    }

    [Admin("Contents/ContentItems/{contentTypeId?}", "ListContentItems")]
    public async Task<IActionResult> List(
        [FromServices] IOptions<PagerOptions> pagerOptions,
        [FromServices] IShapeFactory shapeFactory,
        [FromServices] IContentsAdminListQueryService contentsAdminListQueryService,
        [ModelBinder(BinderType = typeof(ContentItemFilterEngineModelBinder), Name = "q")] QueryFilterResult<ContentItem> queryFilterResult,
        ContentOptionsViewModel options,
        PagerParameters pagerParameters,
        string contentTypeId = "",
        string stereotype = "")
    {
        var contentTypeDefinitions = (await _contentDefinitionManager.ListTypeDefinitionsAsync())
            .OrderBy(ctd => ctd.DisplayName)
            .ToArray();

        if (!await _authorizationService.AuthorizeContentTypeDefinitionsAsync(User, CommonPermissions.ListContent, contentTypeDefinitions, _contentManager))
        {
            return Forbid();
        }

        // The parameter contentTypeId is used by the AdminMenus. Pass it to the options.
        if (!string.IsNullOrEmpty(contentTypeId))
        {
            options.SelectedContentType = contentTypeId;
        }

        // The filter is bound separately and mapped to the options.
        // The options must still be bound so that options that are not filters are still bound.
        options.FilterResult = queryFilterResult;

        var hasSelectedContentType = !string.IsNullOrEmpty(options.SelectedContentType);

        if (hasSelectedContentType)
        {
            // When the selected content type is provided via the route or options a placeholder node is used to apply a filter.
            options.FilterResult.TryAddOrReplace(new ContentTypeFilterNode(options.SelectedContentType));

            var contentTypeDefinition = await _contentDefinitionManager.GetTypeDefinitionAsync(options.SelectedContentType);
            if (contentTypeDefinition == null)
            {
                return NotFound();
            }

            options.CreatableTypes = await GetCreatableTypeOptionsAsync(options.CanCreateSelectedContentType, contentTypeDefinition);
        }

        if (!hasSelectedContentType && !string.IsNullOrEmpty(stereotype))
        {
            // When a stereotype is provided via the query parameter or options a placeholder node is used to apply a filter.
            options.FilterResult.TryAddOrReplace(new StereotypeFilterNode(stereotype));

            var availableContentTypeDefinitions = contentTypeDefinitions
                .Where(definition => definition.StereotypeEquals(stereotype, StringComparison.OrdinalIgnoreCase))
                .ToArray();

            if (availableContentTypeDefinitions.Length > 0)
            {
                options.ContentTypeOptions = await GetListableContentTypeOptionsAsync(availableContentTypeDefinitions, options.SelectedContentType, false);
                options.CreatableTypes = await GetCreatableTypeOptionsAsync(options.CanCreateSelectedContentType, availableContentTypeDefinitions);
            }
        }

        if (options.CreatableTypes == null)
        {
            // At this point, the creatable types were not yet populated. Populate them using all creatable types.
            var creatableContentTypeDefinitions = contentTypeDefinitions
                .Where(ctd => ctd.IsCreatable())
                .ToArray();

            options.CreatableTypes = await GetCreatableTypeOptionsAsync(false, creatableContentTypeDefinitions);
        }

        // We populate the remaining SelectLists.
        options.ContentStatuses =
        [
            new SelectListItem(S["Latest"], nameof(ContentsStatus.Latest)),
            new SelectListItem(S["Published"], nameof(ContentsStatus.Published)),
            new SelectListItem(S["Unpublished"], nameof(ContentsStatus.Draft)),
            new SelectListItem(S["All versions"], nameof(ContentsStatus.AllVersions)),
        ];

        if (await IsAuthorizedAsync(CommonPermissions.ListContent))
        {
            options.ContentStatuses.Insert(1, new SelectListItem(S["Owned by me"], nameof(ContentsStatus.Owner)));
        }

        options.ContentSorts =
        [
            new SelectListItem(S["Recently created"], nameof(ContentsOrder.Created)),
            new SelectListItem(S["Recently modified"], nameof(ContentsOrder.Modified)),
            new SelectListItem(S["Recently published"], nameof(ContentsOrder.Published)),
            new SelectListItem(S["Title"], nameof(ContentsOrder.Title)),
        ];

        options.ContentsBulkAction =
        [
            new SelectListItem(S["Publish Now"], nameof(ContentsBulkAction.PublishNow)),
            new SelectListItem(S["Unpublish"], nameof(ContentsBulkAction.Unpublish)),
            new SelectListItem(S["Delete"], nameof(ContentsBulkAction.Remove)),
        ];

        if (options.ContentTypeOptions == null
            && (string.IsNullOrEmpty(options.SelectedContentType) || string.IsNullOrEmpty(contentTypeId)))
        {
            options.ContentTypeOptions = await GetListableContentTypeOptionsAsync(contentTypeDefinitions, options.SelectedContentType, true);
        }

        // If ContentTypeOptions is not initialized by query string or by the code above, initialize it.
        options.ContentTypeOptions ??= [];

        // With the populated options, filter the query allowing the filters to alter the options.
        var query = await contentsAdminListQueryService.QueryAsync(options, this);

        // The search text is provided back to the UI.
        options.SearchText = options.FilterResult.ToString();
        options.OriginalSearchText = options.SearchText;

        // Populate route values to maintain previous route data when generating page links.
        options.RouteValues.TryAdd("q", options.FilterResult.ToString());

        var pager = new Pager(pagerParameters, pagerOptions.Value.GetPageSize());

        var itemsPerPage = pagerOptions.Value.MaxPagedCount > 0
            ? pagerOptions.Value.MaxPagedCount
            : await query.CountAsync();

        var pagerShape = await shapeFactory.PagerAsync(pager, itemsPerPage, options.RouteValues);

        // Load items so that loading handlers are invoked.
        var pageOfContentItems = await query.Skip(pager.GetStartIndex())
            .Take(pager.PageSize)
            .ListAsync(_contentManager);

        // We prepare the content items SummaryAdmin shape.
        var contentItemSummaries = new List<dynamic>();
        foreach (var contentItem in pageOfContentItems)
        {
            contentItemSummaries.Add(await _contentItemDisplayManager.BuildDisplayAsync(contentItem, this, "SummaryAdmin"));
        }

        // Populate options pager summary values.
        var startIndex = (pager.Page - 1) * pager.PageSize + 1;
        options.StartIndex = startIndex;
        options.EndIndex = startIndex + contentItemSummaries.Count - 1;
        options.ContentItemsCount = contentItemSummaries.Count;
        options.TotalItemCount = itemsPerPage;

        var header = await _contentOptionsDisplayManager.BuildEditorAsync(options, this, false, string.Empty, string.Empty);

        var shapeViewModel = await shapeFactory.CreateAsync<ListContentsViewModel>("ContentsAdminList", viewModel =>
        {
            viewModel.ContentItems = contentItemSummaries;
            viewModel.Pager = pagerShape;
            viewModel.Options = options;
            viewModel.Header = header;
        });

        return View(shapeViewModel);
    }

    [HttpPost]
    [ActionName(nameof(List))]
    [FormValueRequired("submit.Filter")]
    public async Task<ActionResult> ListFilterPOST(ContentOptionsViewModel options)
    {
        // When the user has typed something into the search input no further evaluation of the form post is required.
        if (!string.Equals(options.SearchText, options.OriginalSearchText, StringComparison.OrdinalIgnoreCase))
        {
            return RedirectToAction(nameof(List), new RouteValueDictionary
            {
                { "q", options.SearchText },
            });
        }

        // Evaluate the values provided in the form post and map them to the filter result and route values.
        await _contentOptionsDisplayManager.UpdateEditorAsync(options, this, false, string.Empty, string.Empty);

        // The route value must always be added after the editors have updated the models.
        options.RouteValues.TryAdd("q", options.FilterResult.ToString());

        return RedirectToAction(nameof(List), options.RouteValues);
    }

    [HttpPost]
    [ActionName(nameof(List))]
    [FormValueRequired("submit.BulkAction")]
    public async Task<ActionResult> ListPOST(ContentOptionsViewModel options, long[] itemIds)
    {
        if (itemIds.Length > 0)
        {
            // Load items so that loading handlers are invoked.
            var checkedContentItems = await _session.Query<ContentItem, ContentItemIndex>()
                .Where(x => x.DocumentId.IsIn(itemIds) && x.Latest)
                .ListAsync(_contentManager);

            switch (options.BulkAction)
            {
                case ContentsBulkAction.None:
                    break;
                case ContentsBulkAction.PublishNow:
                    foreach (var item in checkedContentItems)
                    {
                        if (!await _authorizationService.AuthorizeAsync(User, CommonPermissions.PublishContent, item))
                        {
                            await _notifier.WarningAsync(H["Couldn't publish selected content."]);
                            await _session.CancelAsync();
                            return Forbid();
                        }

                        await _contentManager.PublishAsync(item);
                    }
                    await _notifier.SuccessAsync(H["Content published successfully."]);
                    break;
                case ContentsBulkAction.Unpublish:
                    foreach (var item in checkedContentItems)
                    {
                        if (!await IsAuthorizedAsync(CommonPermissions.PublishContent, item))
                        {
                            await _notifier.WarningAsync(H["Couldn't unpublish selected content."]);
                            await _session.CancelAsync();
                            return Forbid();
                        }

                        await _contentManager.UnpublishAsync(item);
                    }
                    await _notifier.SuccessAsync(H["Content unpublished successfully."]);
                    break;
                case ContentsBulkAction.Remove:
                    foreach (var item in checkedContentItems)
                    {
                        if (!await IsAuthorizedAsync(CommonPermissions.DeleteContent, item))
                        {
                            await _notifier.WarningAsync(H["Couldn't remove selected content."]);
                            await _session.CancelAsync();
                            return Forbid();
                        }

                        await _contentManager.RemoveAsync(item);
                    }
                    await _notifier.SuccessAsync(H["Content removed successfully."]);
                    break;
                default:
                    return BadRequest();
            }
        }

        return RedirectToAction(nameof(List));
    }

    [Admin("Contents/ContentTypes/{id}/Create", "CreateContentItem")]
    public async Task<IActionResult> Create(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            return NotFound();
        }

        var contentItem = await CreateContentItemOwnedByCurrentUserAsync(id);

        if (!await IsAuthorizedAsync(CommonPermissions.EditContent, contentItem))
        {
            return Forbid();
        }

        var model = await _contentItemDisplayManager.BuildEditorAsync(contentItem, this, true);

        return View(model);
    }

    [HttpPost]
    [ActionName(nameof(Create))]
    [FormValueRequired("submit.Save")]
    public Task<IActionResult> CreatePOST(
        string id,
        [Bind(Prefix = "submit.Save")] string submitSave,
        string returnUrl)
    {
        var stayOnSamePage = submitSave == "submit.SaveAndContinue";
        return CreatePOST(id, returnUrl, stayOnSamePage, async contentItem =>
        {
            await _contentManager.SaveDraftAsync(contentItem);

            var typeDefinition = await _contentDefinitionManager.GetTypeDefinitionAsync(contentItem.ContentType);

            await _notifier.SuccessAsync(string.IsNullOrWhiteSpace(typeDefinition?.DisplayName)
                ? H["Your content draft has been saved."]
                : H["Your {0} draft has been saved.", typeDefinition.DisplayName]);
        });
    }

    [HttpPost]
    [ActionName(nameof(Create))]
    [FormValueRequired("submit.Publish")]
    public async Task<IActionResult> CreateAndPublishPOST(
        string id,
        [Bind(Prefix = "submit.Publish")] string submitPublish,
        string returnUrl)
    {
        if (string.IsNullOrEmpty(id))
        {
            return NotFound();
        }

        var stayOnSamePage = submitPublish == "submit.PublishAndContinue";
        // Pass a dummy content item to the authorization check to check for "own" variations permissions.
        if (!await _authorizationService.AuthorizeContentTypeAsync(User, CommonPermissions.PublishContent, id, CurrentUserId()))
        {
            return Forbid();
        }

        return await CreatePOST(id, returnUrl, stayOnSamePage, async contentItem =>
        {
            await _contentManager.PublishAsync(contentItem);

            var typeDefinition = await _contentDefinitionManager.GetTypeDefinitionAsync(contentItem.ContentType);

            await _notifier.SuccessAsync(string.IsNullOrWhiteSpace(typeDefinition.DisplayName)
                ? H["Your content has been published."]
                : H["Your {0} has been published.", typeDefinition.DisplayName]);
        });
    }

    [Admin("Contents/ContentItems/{contentItemId}/Display", "AdminContentItem")]
    public async Task<IActionResult> Display(string contentItemId)
    {
        var contentItem = await _contentManager.GetAsync(contentItemId, VersionOptions.Latest);

        if (contentItem == null)
        {
            return NotFound();
        }

        if (!await IsAuthorizedAsync(CommonPermissions.ViewContent, contentItem))
        {
            return Forbid();
        }

        var model = await _contentItemDisplayManager.BuildDisplayAsync(contentItem, this, "DetailAdmin");

        return View(model);
    }

    [Admin("Contents/ContentItems/{contentItemId}/Edit", "EditContentItem")]
    public async Task<IActionResult> Edit(string contentItemId)
    {
        var contentItem = await _contentManager.GetAsync(contentItemId, VersionOptions.Latest);

        if (contentItem == null)
        {
            return NotFound();
        }

        if (!await IsAuthorizedAsync(CommonPermissions.EditContent, contentItem))
        {
            return Forbid();
        }

        var model = await _contentItemDisplayManager.BuildEditorAsync(contentItem, this, false);

        return View(model);
    }

    [HttpPost]
    [ActionName(nameof(Edit))]
    [FormValueRequired("submit.Save")]
    public Task<IActionResult> EditPOST(
        string contentItemId,
        [Bind(Prefix = "submit.Save")] string submitSave,
        string returnUrl)
    {
        var stayOnSamePage = submitSave == "submit.SaveAndContinue";
        return EditPOST(contentItemId, returnUrl, stayOnSamePage, async contentItem =>
        {
            await _contentManager.SaveDraftAsync(contentItem);

            var typeDefinition = await _contentDefinitionManager.GetTypeDefinitionAsync(contentItem.ContentType);

            await _notifier.SuccessAsync(string.IsNullOrWhiteSpace(typeDefinition?.DisplayName)
                ? H["Your content draft has been saved."]
                : H["Your {0} draft has been saved.", typeDefinition.DisplayName]);
        });
    }

    [HttpPost]
    [ActionName(nameof(Edit))]
    [FormValueRequired("submit.Publish")]
    public async Task<IActionResult> EditAndPublishPOST(
        string contentItemId,
        [Bind(Prefix = "submit.Publish")] string submitPublish,
        string returnUrl)
    {
        var stayOnSamePage = submitPublish == "submit.PublishAndContinue";

        var content = await _contentManager.GetAsync(contentItemId, VersionOptions.Latest);

        if (content == null)
        {
            return NotFound();
        }

        if (!await IsAuthorizedAsync(CommonPermissions.PublishContent, content))
        {
            return Forbid();
        }

        return await EditPOST(contentItemId, returnUrl, stayOnSamePage, async contentItem =>
        {
            await _contentManager.PublishAsync(contentItem);

            var typeDefinition = await _contentDefinitionManager.GetTypeDefinitionAsync(contentItem.ContentType);

            await _notifier.SuccessAsync(string.IsNullOrWhiteSpace(typeDefinition?.DisplayName)
                ? H["Your content has been published."]
                : H["Your {0} has been published.", typeDefinition.DisplayName]);
        });
    }

    [HttpPost]
    [Admin("Contents/ContentItems/{contentItemId}/Clone", "AdminCloneContentItem")]
    public async Task<IActionResult> Clone(string contentItemId, string returnUrl)
    {
        var contentItem = await _contentManager.GetAsync(contentItemId, VersionOptions.Latest);

        if (contentItem == null)
        {
            return NotFound();
        }

        if (!await IsAuthorizedAsync(CommonPermissions.CloneContent, contentItem))
        {
            return Forbid();
        }

        try
        {
            await _contentManager.CloneAsync(contentItem);
        }
        catch (InvalidOperationException)
        {
            await _notifier.WarningAsync(H["Could not clone the content item."]);
            return Url.IsLocalUrl(returnUrl)
                ? this.LocalRedirect(returnUrl, true)
                : RedirectToAction(nameof(List));
        }

        await _notifier.InformationAsync(H["Successfully cloned. The clone was saved as a draft."]);

        return Url.IsLocalUrl(returnUrl)
            ? this.LocalRedirect(returnUrl, true)
            : RedirectToAction(nameof(List));
    }

    [HttpPost]
    [Admin("Contents/ContentItems/{contentItemId}/DiscardDraft", "AdminDiscardDraftContentItem")]
    public async Task<IActionResult> DiscardDraft(string contentItemId, string returnUrl)
    {
        var contentItem = await _contentManager.GetAsync(contentItemId, VersionOptions.Latest);

        if (contentItem == null || contentItem.Published)
        {
            return NotFound();
        }

        if (!await IsAuthorizedAsync(CommonPermissions.DeleteContent, contentItem))
        {
            return Forbid();
        }

        if (contentItem != null)
        {
            await _contentManager.DiscardDraftAsync(contentItem);

            var typeDefinition = await _contentDefinitionManager.GetTypeDefinitionAsync(contentItem.ContentType);

            await _notifier.SuccessAsync(string.IsNullOrWhiteSpace(typeDefinition?.DisplayName)
                ? H["The draft has been removed."]
                : H["The {0} draft has been removed.", typeDefinition.DisplayName]);
        }

        return Url.IsLocalUrl(returnUrl)
            ? this.LocalRedirect(returnUrl, true)
            : RedirectToAction(nameof(List));
    }

    [HttpPost]
    [Admin("Contents/ContentItems/{contentItemId}/Delete", "AdminDeleteContentItem")]
    public async Task<IActionResult> Remove(string contentItemId, string returnUrl)
    {
        var contentItem = await _contentManager.GetAsync(contentItemId, VersionOptions.Latest);

        if (!await IsAuthorizedAsync(CommonPermissions.DeleteContent, contentItem))
        {
            return Forbid();
        }

        if (contentItem != null)
        {
            await _contentManager.RemoveAsync(contentItem);

            var typeDefinition = await _contentDefinitionManager.GetTypeDefinitionAsync(contentItem.ContentType);

            await _notifier.SuccessAsync(string.IsNullOrWhiteSpace(typeDefinition?.DisplayName)
                ? H["That content has been removed."]
                : H["That {0} has been removed.", typeDefinition.DisplayName]);
        }

        return Url.IsLocalUrl(returnUrl)
            ? this.LocalRedirect(returnUrl, true)
            : RedirectToAction(nameof(List));
    }

    [HttpPost]
    [Admin("Contents/ContentItems/{contentItemId}/Publish", "AdminPublishContentItem")]
    public async Task<IActionResult> Publish(string contentItemId, string returnUrl)
    {
        var contentItem = await _contentManager.GetAsync(contentItemId, VersionOptions.Latest);
        if (contentItem == null)
        {
            return NotFound();
        }

        if (!await IsAuthorizedAsync(CommonPermissions.PublishContent, contentItem))
        {
            return Forbid();
        }

        await _contentManager.PublishAsync(contentItem);

        var typeDefinition = await _contentDefinitionManager.GetTypeDefinitionAsync(contentItem.ContentType);

        if (string.IsNullOrEmpty(typeDefinition?.DisplayName))
        {
            await _notifier.SuccessAsync(H["That content has been published."]);
        }
        else
        {
            await _notifier.SuccessAsync(H["That {0} has been published.", typeDefinition.DisplayName]);
        }

        return Url.IsLocalUrl(returnUrl)
            ? this.LocalRedirect(returnUrl, true)
            : RedirectToAction(nameof(List));
    }

    [HttpPost]
    [Admin("Contents/ContentItems/{contentItemId}/Unpublish", "AdminUnpublishContentItem")]
    public async Task<IActionResult> Unpublish(string contentItemId, string returnUrl)
    {
        var contentItem = await _contentManager.GetAsync(contentItemId, VersionOptions.Latest);
        if (contentItem == null)
        {
            return NotFound();
        }

        if (!await IsAuthorizedAsync(CommonPermissions.PublishContent, contentItem))
        {
            return Forbid();
        }

        await _contentManager.UnpublishAsync(contentItem);

        var typeDefinition = await _contentDefinitionManager.GetTypeDefinitionAsync(contentItem.ContentType);

        if (string.IsNullOrEmpty(typeDefinition?.DisplayName))
        {
            await _notifier.SuccessAsync(H["The content has been unpublished."]);
        }
        else
        {
            await _notifier.SuccessAsync(H["The {0} has been unpublished.", typeDefinition.DisplayName]);
        }

        return Url.IsLocalUrl(returnUrl)
            ? this.LocalRedirect(returnUrl, true)
            : RedirectToAction(nameof(List));
    }

    private async Task<IActionResult> CreatePOST(
        string id,
        string returnUrl,
        bool stayOnSamePage,
        Func<ContentItem, Task> conditionallyPublish)
    {
        var contentItem = await CreateContentItemOwnedByCurrentUserAsync(id);

        if (!await IsAuthorizedAsync(CommonPermissions.EditContent, contentItem))
        {
            return Forbid();
        }

        var model = await _contentItemDisplayManager.UpdateEditorAsync(contentItem, this, true);

        if (ModelState.IsValid)
        {
            await _contentManager.CreateAsync(contentItem, VersionOptions.Draft);
        }

        if (!ModelState.IsValid)
        {
            await _session.CancelAsync();
            return View(model);
        }

        await conditionallyPublish(contentItem);

        if (!string.IsNullOrEmpty(returnUrl) && !stayOnSamePage)
        {
            return this.LocalRedirect(returnUrl, true);
        }

        var adminRouteValues = (await _contentManager.PopulateAspectAsync<ContentItemMetadata>(contentItem)).AdminRouteValues;

        if (!string.IsNullOrEmpty(returnUrl))
        {
            adminRouteValues.Add("returnUrl", returnUrl);
        }

        return RedirectToRoute(adminRouteValues);
    }

    private async Task<IActionResult> EditPOST(
        string contentItemId,
        string returnUrl,
        bool stayOnSamePage,
        Func<ContentItem, Task> conditionallyPublish)
    {
        var contentItem = await _contentManager.GetAsync(contentItemId, VersionOptions.DraftRequired);

        if (contentItem == null)
        {
            return NotFound();
        }

        if (!await IsAuthorizedAsync(CommonPermissions.EditContent, contentItem))
        {
            return Forbid();
        }

        var model = await _contentItemDisplayManager.UpdateEditorAsync(contentItem, this, false);

        if (!ModelState.IsValid)
        {
            await _session.CancelAsync();
            return View(nameof(Edit), model);
        }

        await conditionallyPublish(contentItem);

        if (returnUrl == null)
        {
            return RedirectToAction(nameof(Edit), new RouteValueDictionary
            {
                { "ContentItemId", contentItem.ContentItemId },
            });
        }

        if (stayOnSamePage)
        {
            return RedirectToAction(nameof(Edit), new RouteValueDictionary
            {
                { "ContentItemId", contentItem.ContentItemId },
                { "returnUrl", returnUrl },
            });
        }

        return this.LocalRedirect(returnUrl, true);
    }

    private async Task<List<SelectListItem>> GetCreatableTypeOptionsAsync(
        bool canCreateSelectedContentType,
        params ContentTypeDefinition[] contentTypeDefinitions)
    {
        var options = new List<SelectListItem>();

        foreach (var contentTypeDefinition in contentTypeDefinitions)
        {
            // Allows non creatable types to be created by another admin page.
            var creatable = contentTypeDefinition.IsCreatable() || canCreateSelectedContentType;

            if (creatable && await _authorizationService.AuthorizeContentTypeAsync(User, CommonPermissions.EditContent, contentTypeDefinition, CurrentUserId()))
            {
                // Populate the creatable types.
                options.Add(new SelectListItem(contentTypeDefinition.DisplayName, contentTypeDefinition.Name));
            }
        }

        return options;
    }

    private async Task<List<SelectListItem>> GetListableContentTypeOptionsAsync(
        IEnumerable<ContentTypeDefinition> definitions,
        string selectedContentType,
        bool showSelectAll = true)
    {
        var currentUserId = CurrentUserId();

        var items = new List<SelectListItem>();

        if (showSelectAll)
        {
            items.Add(new SelectListItem(S["All content types"], string.Empty));
        };

        foreach (var definition in definitions)
        {
            if (!definition.IsListable()
                || !await _authorizationService.AuthorizeContentTypeAsync(User, CommonPermissions.ListContent, definition.Name, currentUserId))
            {
                continue;
            }

            items.Add(new SelectListItem(definition.DisplayName, definition.Name, string.Equals(definition.Name, selectedContentType, StringComparison.Ordinal)));
        }

        return items;
    }

    private async Task<ContentItem> CreateContentItemOwnedByCurrentUserAsync(string contentType)
    {
        var contentItem = await _contentManager.NewAsync(contentType);
        contentItem.Owner = CurrentUserId();

        return contentItem;
    }

    private string _currentUserId;

    private string CurrentUserId()
        => _currentUserId ??= User.FindFirstValue(ClaimTypes.NameIdentifier);

    private async Task<bool> IsAuthorizedAsync(Permission permission)
        => await _authorizationService.AuthorizeAsync(User, permission);

    private async Task<bool> IsAuthorizedAsync(Permission permission, object resource)
        => await _authorizationService.AuthorizeAsync(User, permission, resource);
}
