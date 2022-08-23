using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Display;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.ContentManagement.Metadata.Settings;
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

namespace OrchardCore.Contents.Controllers
{
    public class AdminController : Controller
    {
        private readonly IContentManager _contentManager;
        private readonly IContentDefinitionManager _contentDefinitionManager;
        private readonly PagerOptions _pagerOptions;
        private readonly ISession _session;
        private readonly IContentItemDisplayManager _contentItemDisplayManager;
        private readonly INotifier _notifier;
        private readonly IAuthorizationService _authorizationService;
        private readonly IDisplayManager<ContentOptionsViewModel> _contentOptionsDisplayManager;
        private readonly IContentsAdminListQueryService _contentsAdminListQueryService;
        private readonly IHtmlLocalizer H;
        private readonly IStringLocalizer S;
        private readonly IUpdateModelAccessor _updateModelAccessor;
        private readonly IShapeFactory _shapeFactory;
        private readonly dynamic New;
        private readonly ILogger _logger;

        public AdminController(
            IAuthorizationService authorizationService,
            IContentManager contentManager,
            IContentItemDisplayManager contentItemDisplayManager,
            IContentDefinitionManager contentDefinitionManager,
            IOptions<PagerOptions> pagerOptions,
            INotifier notifier,
            ISession session,
            IShapeFactory shapeFactory,
            IDisplayManager<ContentOptionsViewModel> contentOptionsDisplayManager,
            IContentsAdminListQueryService contentsAdminListQueryService,
            ILogger<AdminController> logger,
            IHtmlLocalizer<AdminController> htmlLocalizer,
            IStringLocalizer<AdminController> stringLocalizer,
            IUpdateModelAccessor updateModelAccessor)
        {
            _authorizationService = authorizationService;
            _notifier = notifier;
            _contentItemDisplayManager = contentItemDisplayManager;
            _session = session;
            _pagerOptions = pagerOptions.Value;
            _contentManager = contentManager;
            _contentDefinitionManager = contentDefinitionManager;
            _updateModelAccessor = updateModelAccessor;
            _contentOptionsDisplayManager = contentOptionsDisplayManager;
            _contentsAdminListQueryService = contentsAdminListQueryService;

            H = htmlLocalizer;
            S = stringLocalizer;
            _shapeFactory = shapeFactory;
            New = shapeFactory;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> List(
            [ModelBinder(BinderType = typeof(ContentItemFilterEngineModelBinder), Name = "q")] QueryFilterResult<ContentItem> queryFilterResult,
            ContentOptionsViewModel options,
            PagerParameters pagerParameters,
            string contentTypeId = "")
        {
            var contentTypeDefinitions = _contentDefinitionManager.ListTypeDefinitions()
                    .Where(ctd => IsCreatable(ctd))
                    .OrderBy(ctd => ctd.DisplayName);

            if (!await _authorizationService.AuthorizeContentTypeDefinitionsAsync(User, CommonPermissions.ViewContent, contentTypeDefinitions, _contentManager))
            {
                return Forbid();
            }

            var pager = new Pager(pagerParameters, _pagerOptions.PageSize);

            // This is used by the AdminMenus so needs to be passed into the options.
            if (!String.IsNullOrEmpty(contentTypeId))
            {
                options.SelectedContentType = contentTypeId;
            }

            // The filter is bound seperately and mapped to the options.
            // The options must still be bound so that options that are not filters are still bound
            options.FilterResult = queryFilterResult;

            // Populate the creatable types.
            if (!String.IsNullOrEmpty(options.SelectedContentType))
            {
                // When the selected content type is provided via the route or options a placeholder node is used to apply a filter.
                options.FilterResult.TryAddOrReplace(new ContentTypeFilterNode(options.SelectedContentType));

                var contentTypeDefinition = _contentDefinitionManager.GetTypeDefinition(options.SelectedContentType);
                if (contentTypeDefinition == null)
                {
                    return NotFound();
                }

                var creatableList = new List<SelectListItem>();

                // Allows non creatable types to be created by another admin page.
                if (IsCreatable(contentTypeDefinition) || options.CanCreateSelectedContentType)
                {
                    var contentItem = await CreateContentItemForOwnedByCurrentAsync(contentTypeDefinition.Name);

                    if (await IsAuthorizedAsync(CommonPermissions.EditContent, contentItem))
                    {
                        creatableList.Add(new SelectListItem(contentTypeDefinition.DisplayName, contentTypeDefinition.Name));
                    }
                }

                options.CreatableTypes = creatableList;
            }

            if (options.CreatableTypes == null)
            {
                var creatableList = new List<SelectListItem>();
                if (contentTypeDefinitions.Any())
                {
                    foreach (var contentTypeDefinition in contentTypeDefinitions)
                    {
                        var contentItem = await CreateContentItemForOwnedByCurrentAsync(contentTypeDefinition.Name);

                        if (IsCreatable(contentTypeDefinition) && await IsAuthorizedAsync(CommonPermissions.EditContent, contentItem))
                        {
                            creatableList.Add(new SelectListItem(contentTypeDefinition.DisplayName, contentTypeDefinition.Name));
                        }
                    }
                }

                options.CreatableTypes = creatableList;
            }

            // We populate the remaining SelectLists.
            options.ContentStatuses = new List<SelectListItem>()
            {
                new SelectListItem() { Text = S["Latest"], Value = nameof(ContentsStatus.Latest), Selected = options.ContentsStatus == ContentsStatus.Latest },
                new SelectListItem() { Text = S["Published"], Value = nameof(ContentsStatus.Published), Selected = options.ContentsStatus == ContentsStatus.Published },
                new SelectListItem() { Text = S["Unpublished"], Value = nameof(ContentsStatus.Draft), Selected = options.ContentsStatus == ContentsStatus.Draft },
                new SelectListItem() { Text = S["All versions"], Value = nameof(ContentsStatus.AllVersions), Selected = options.ContentsStatus == ContentsStatus.AllVersions }
            };

            if (await IsAuthorizedAsync(Permissions.ListContent))
            {
                options.ContentStatuses.Insert(1, new SelectListItem() { Text = S["Owned by me"], Value = nameof(ContentsStatus.Owner) });
            }

            options.ContentSorts = new List<SelectListItem>()
            {
                new SelectListItem() { Text = S["Recently created"], Value = nameof(ContentsOrder.Created), Selected = options.OrderBy == ContentsOrder.Created },
                new SelectListItem() { Text = S["Recently modified"], Value = nameof(ContentsOrder.Modified), Selected = options.OrderBy == ContentsOrder.Modified },
                new SelectListItem() { Text = S["Recently published"], Value = nameof(ContentsOrder.Published), Selected = options.OrderBy == ContentsOrder.Published },
                new SelectListItem() { Text = S["Title"], Value = nameof(ContentsOrder.Title), Selected = options.OrderBy == ContentsOrder.Title },
            };

            options.ContentsBulkAction = new List<SelectListItem>()
            {
                new SelectListItem() { Text = S["Publish Now"], Value = nameof(ContentsBulkAction.PublishNow) },
                new SelectListItem() { Text = S["Unpublish"], Value = nameof(ContentsBulkAction.Unpublish) },
                new SelectListItem() { Text = S["Delete"], Value = nameof(ContentsBulkAction.Remove) }
            };

            if ((String.IsNullOrEmpty(options.SelectedContentType) || String.IsNullOrEmpty(contentTypeId)) && options.ContentTypeOptions == null)
            {
                var listableTypes = new List<ContentTypeDefinition>();
                var userNameIdentifier = CurrentUserId();

                foreach (var ctd in _contentDefinitionManager.ListTypeDefinitions())
                {
                    if (!ctd.GetSettings<ContentTypeSettings>().Listable)
                    {
                        continue;
                    }

                    var contentItem = await CreateContentItemForOwnedByCurrentAsync(ctd.Name);

                    if (await IsAuthorizedAsync(CommonPermissions.ViewContent, contentItem))
                    {
                        listableTypes.Add(ctd);
                    }
                }

                var contentTypeOptions = listableTypes
                    .Select(ctd => new KeyValuePair<string, string>(ctd.Name, ctd.DisplayName))
                    .ToList().OrderBy(kvp => kvp.Value);

                options.ContentTypeOptions = new List<SelectListItem>
                {
                    new SelectListItem() { Text = S["All content types"], Value = "" }
                };

                foreach (var option in contentTypeOptions)
                {
                    options.ContentTypeOptions.Add(new SelectListItem() { Text = option.Value, Value = option.Key, Selected = (option.Value == options.SelectedContentType) });
                }
            }

            // If ContentTypeOptions is not initialized by query string or by the code above, initialize it
            if (options.ContentTypeOptions == null)
            {
                options.ContentTypeOptions = new List<SelectListItem>();
            }

            // With the options populated we filter the query, allowing the filters to alter the options.
            var query = await _contentsAdminListQueryService.QueryAsync(options, _updateModelAccessor.ModelUpdater);

            // The search text is provided back to the UI.
            options.SearchText = options.FilterResult.ToString();
            options.OriginalSearchText = options.SearchText;

            // Populate route values to maintain previous route data when generating page links.
            options.RouteValues.TryAdd("q", options.FilterResult.ToString());

            var routeData = new RouteData(options.RouteValues);

            if (_pagerOptions.MaxPagedCount > 0 && pager.PageSize > _pagerOptions.MaxPagedCount)
            {
                pager.PageSize = _pagerOptions.MaxPagedCount;
            }

            var pagerShape = (await New.Pager(pager)).TotalItemCount(_pagerOptions.MaxPagedCount > 0 ? _pagerOptions.MaxPagedCount : await query.CountAsync()).RouteData(routeData);

            // Load items so that loading handlers are invoked.
            var pageOfContentItems = await query.Skip(pager.GetStartIndex()).Take(pager.PageSize).ListAsync(_contentManager);

            // We prepare the content items SummaryAdmin shape
            var contentItemSummaries = new List<dynamic>();
            foreach (var contentItem in pageOfContentItems)
            {
                contentItemSummaries.Add(await _contentItemDisplayManager.BuildDisplayAsync(contentItem, _updateModelAccessor.ModelUpdater, "SummaryAdmin"));
            }

            // Populate options pager summary values.
            var startIndex = (pagerShape.Page - 1) * (pagerShape.PageSize) + 1;
            options.StartIndex = startIndex;
            options.EndIndex = startIndex + contentItemSummaries.Count - 1;
            options.ContentItemsCount = contentItemSummaries.Count;
            options.TotalItemCount = pagerShape.TotalItemCount;

            var header = await _contentOptionsDisplayManager.BuildEditorAsync(options, _updateModelAccessor.ModelUpdater, false);

            var shapeViewModel = await _shapeFactory.CreateAsync<ListContentsViewModel>("ContentsAdminList", viewModel =>
            {
                viewModel.ContentItems = contentItemSummaries;
                viewModel.Pager = pagerShape;
                viewModel.Options = options;
                viewModel.Header = header;
            });

            return View(shapeViewModel);
        }

        [HttpPost, ActionName("List")]
        [FormValueRequired("submit.Filter")]
        public async Task<ActionResult> ListFilterPOST(ContentOptionsViewModel options)
        {
            // When the user has typed something into the search input no further evaluation of the form post is required.
            if (!String.Equals(options.SearchText, options.OriginalSearchText, StringComparison.OrdinalIgnoreCase))
            {
                return RedirectToAction(nameof(List), new RouteValueDictionary { { "q", options.SearchText } });
            }

            // Evaluate the values provided in the form post and map them to the filter result and route values.
            await _contentOptionsDisplayManager.UpdateEditorAsync(options, _updateModelAccessor.ModelUpdater, false);

            // The route value must always be added after the editors have updated the models.
            options.RouteValues.TryAdd("q", options.FilterResult.ToString());

            return RedirectToAction(nameof(List), options.RouteValues);
        }

        [HttpPost, ActionName("List")]
        [FormValueRequired("submit.BulkAction")]
        public async Task<ActionResult> ListPOST(ContentOptionsViewModel options, IEnumerable<int> itemIds)
        {
            if (itemIds?.Count() > 0)
            {
                // Load items so that loading handlers are invoked.
                var checkedContentItems = await _session.Query<ContentItem, ContentItemIndex>().Where(x => x.DocumentId.IsIn(itemIds) && x.Latest).ListAsync(_contentManager);
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
                        throw new ArgumentOutOfRangeException();
                }
            }

            return RedirectToAction(nameof(List));
        }

        public async Task<IActionResult> Create(string id)
        {
            if (String.IsNullOrWhiteSpace(id))
            {
                return NotFound();
            }

            var contentItem = await CreateContentItemForOwnedByCurrentAsync(id);

            if (!await IsAuthorizedAsync(CommonPermissions.EditContent, contentItem))
            {
                return Forbid();
            }

            var model = await _contentItemDisplayManager.BuildEditorAsync(contentItem, _updateModelAccessor.ModelUpdater, true);

            return View(model);
        }

        [HttpPost, ActionName("Create")]
        [FormValueRequired("submit.Save")]
        public Task<IActionResult> CreatePOST(string id, [Bind(Prefix = "submit.Save")] string submitSave, string returnUrl)
        {
            var stayOnSamePage = submitSave == "submit.SaveAndContinue";
            return CreatePOST(id, returnUrl, stayOnSamePage, async contentItem =>
            {
                await _contentManager.SaveDraftAsync(contentItem);

                var typeDefinition = _contentDefinitionManager.GetTypeDefinition(contentItem.ContentType);

                await _notifier.SuccessAsync(string.IsNullOrWhiteSpace(typeDefinition.DisplayName)
                    ? H["Your content draft has been saved."]
                    : H["Your {0} draft has been saved.", typeDefinition.DisplayName]);
            });
        }

        [HttpPost, ActionName("Create")]
        [FormValueRequired("submit.Publish")]
        public async Task<IActionResult> CreateAndPublishPOST(string id, [Bind(Prefix = "submit.Publish")] string submitPublish, string returnUrl)
        {
            var stayOnSamePage = submitPublish == "submit.PublishAndContinue";
            // pass a dummy content to the authorization check to check for "own" variations
            var dummyContent = await CreateContentItemForOwnedByCurrentAsync(id);

            if (!await IsAuthorizedAsync(CommonPermissions.PublishContent, dummyContent))
            {
                return Forbid();
            }

            return await CreatePOST(id, returnUrl, stayOnSamePage, async contentItem =>
            {
                await _contentManager.PublishAsync(contentItem);

                var typeDefinition = _contentDefinitionManager.GetTypeDefinition(contentItem.ContentType);

                await _notifier.SuccessAsync(String.IsNullOrWhiteSpace(typeDefinition.DisplayName)
                    ? H["Your content has been published."]
                    : H["Your {0} has been published.", typeDefinition.DisplayName]);
            });
        }

        private async Task<IActionResult> CreatePOST(string id, string returnUrl, bool stayOnSamePage, Func<ContentItem, Task> conditionallyPublish)
        {
            var contentItem = await CreateContentItemForOwnedByCurrentAsync(id);

            if (!await IsAuthorizedAsync(CommonPermissions.EditContent, contentItem))
            {
                return Forbid();
            }

            var model = await _contentItemDisplayManager.UpdateEditorAsync(contentItem, _updateModelAccessor.ModelUpdater, true);

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

            if (!String.IsNullOrEmpty(returnUrl) && !stayOnSamePage)
            {
                return this.LocalRedirect(returnUrl, true);
            }

            var adminRouteValues = (await _contentManager.PopulateAspectAsync<ContentItemMetadata>(contentItem)).AdminRouteValues;

            if (!String.IsNullOrEmpty(returnUrl))
            {
                adminRouteValues.Add("returnUrl", returnUrl);
            }

            return RedirectToRoute(adminRouteValues);
        }

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

            var model = await _contentItemDisplayManager.BuildDisplayAsync(contentItem, _updateModelAccessor.ModelUpdater, "DetailAdmin");

            return View(model);
        }

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

            var model = await _contentItemDisplayManager.BuildEditorAsync(contentItem, _updateModelAccessor.ModelUpdater, false);

            return View(model);
        }

        [HttpPost, ActionName("Edit")]
        [FormValueRequired("submit.Save")]
        public Task<IActionResult> EditPOST(string contentItemId, [Bind(Prefix = "submit.Save")] string submitSave, string returnUrl)
        {
            var stayOnSamePage = submitSave == "submit.SaveAndContinue";
            return EditPOST(contentItemId, returnUrl, stayOnSamePage, async contentItem =>
            {
                await _contentManager.SaveDraftAsync(contentItem);

                var typeDefinition = _contentDefinitionManager.GetTypeDefinition(contentItem.ContentType);

                await _notifier.SuccessAsync(string.IsNullOrWhiteSpace(typeDefinition.DisplayName)
                    ? H["Your content draft has been saved."]
                    : H["Your {0} draft has been saved.", typeDefinition.DisplayName]);
            });
        }

        [HttpPost, ActionName("Edit")]
        [FormValueRequired("submit.Publish")]
        public async Task<IActionResult> EditAndPublishPOST(string contentItemId, [Bind(Prefix = "submit.Publish")] string submitPublish, string returnUrl)
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

                var typeDefinition = _contentDefinitionManager.GetTypeDefinition(contentItem.ContentType);

                await _notifier.SuccessAsync(string.IsNullOrWhiteSpace(typeDefinition.DisplayName)
                    ? H["Your content has been published."]
                    : H["Your {0} has been published.", typeDefinition.DisplayName]);
            });
        }

        private async Task<IActionResult> EditPOST(string contentItemId, string returnUrl, bool stayOnSamePage, Func<ContentItem, Task> conditionallyPublish)
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

            var model = await _contentItemDisplayManager.UpdateEditorAsync(contentItem, _updateModelAccessor.ModelUpdater, false);

            if (!ModelState.IsValid)
            {
                await _session.CancelAsync();
                return View(nameof(Edit), model);
            }

            await conditionallyPublish(contentItem);

            if (returnUrl == null)
            {
                return RedirectToAction(nameof(Edit), new RouteValueDictionary { { "ContentItemId", contentItem.ContentItemId } });
            }

            if (stayOnSamePage)
            {
                return RedirectToAction(nameof(Edit), new RouteValueDictionary { { "ContentItemId", contentItem.ContentItemId }, { "returnUrl", returnUrl } });
            }

            return this.LocalRedirect(returnUrl, true);
        }

        [HttpPost]
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
                return Url.IsLocalUrl(returnUrl) ? (IActionResult)this.LocalRedirect(returnUrl, true) : RedirectToAction(nameof(List));
            }

            await _notifier.InformationAsync(H["Successfully cloned. The clone was saved as a draft."]);

            return Url.IsLocalUrl(returnUrl) ? (IActionResult)this.LocalRedirect(returnUrl, true) : RedirectToAction(nameof(List));
        }

        [HttpPost]
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
                var typeDefinition = _contentDefinitionManager.GetTypeDefinition(contentItem.ContentType);

                await _contentManager.DiscardDraftAsync(contentItem);

                await _notifier.SuccessAsync(string.IsNullOrWhiteSpace(typeDefinition.DisplayName)
                    ? H["The draft has been removed."]
                    : H["The {0} draft has been removed.", typeDefinition.DisplayName]);
            }

            return Url.IsLocalUrl(returnUrl) ? (IActionResult)this.LocalRedirect(returnUrl, true) : RedirectToAction(nameof(List));
        }

        [HttpPost]
        public async Task<IActionResult> Remove(string contentItemId, string returnUrl)
        {
            var contentItem = await _contentManager.GetAsync(contentItemId, VersionOptions.Latest);

            if (!await IsAuthorizedAsync(CommonPermissions.DeleteContent, contentItem))
            {
                return Forbid();
            }

            if (contentItem != null)
            {
                var typeDefinition = _contentDefinitionManager.GetTypeDefinition(contentItem.ContentType);

                await _contentManager.RemoveAsync(contentItem);

                await _notifier.SuccessAsync(String.IsNullOrWhiteSpace(typeDefinition.DisplayName)
                    ? H["That content has been removed."]
                    : H["That {0} has been removed.", typeDefinition.DisplayName]);
            }

            return Url.IsLocalUrl(returnUrl) ? (IActionResult)this.LocalRedirect(returnUrl, true) : RedirectToAction(nameof(List));
        }

        [HttpPost]
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

            var typeDefinition = _contentDefinitionManager.GetTypeDefinition(contentItem.ContentType);

            if (string.IsNullOrEmpty(typeDefinition.DisplayName))
            {
                await _notifier.SuccessAsync(H["That content has been published."]);
            }
            else
            {
                await _notifier.SuccessAsync(H["That {0} has been published.", typeDefinition.DisplayName]);
            }

            return Url.IsLocalUrl(returnUrl) ? (IActionResult)this.LocalRedirect(returnUrl, true) : RedirectToAction(nameof(List));
        }

        [HttpPost]
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

            var typeDefinition = _contentDefinitionManager.GetTypeDefinition(contentItem.ContentType);

            if (String.IsNullOrEmpty(typeDefinition.DisplayName))
            {
                await _notifier.SuccessAsync(H["The content has been unpublished."]);
            }
            else
            {
                await _notifier.SuccessAsync(H["The {0} has been unpublished.", typeDefinition.DisplayName]);
            }

            return Url.IsLocalUrl(returnUrl) ? (IActionResult)this.LocalRedirect(returnUrl, true) : RedirectToAction(nameof(List));
        }

        private static bool IsCreatable(ContentTypeDefinition contentTypeDefinition)
        {
            return contentTypeDefinition.GetSettings<ContentTypeSettings>().Creatable;
        }

        private async Task<ContentItem> CreateContentItemForOwnedByCurrentAsync(string contentType)
        {
            var contentItem = await _contentManager.NewAsync(contentType);
            contentItem.Owner = CurrentUserId();

            return contentItem;
        }

        private string CurrentUserId()
        {
            return User.FindFirstValue(ClaimTypes.NameIdentifier);
        }

        private async Task<bool> IsAuthorizedAsync(Permission permission)
        {
            return await _authorizationService.AuthorizeAsync(User, permission);
        }

        private async Task<bool> IsAuthorizedAsync(Permission permission, object resource)
        {
            return await _authorizationService.AuthorizeAsync(User, permission, resource);
        }

    }
}
