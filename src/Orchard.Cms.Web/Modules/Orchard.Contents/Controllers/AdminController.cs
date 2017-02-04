using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Modules;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.AspNetCore.Mvc.Modules.ActionConstraints;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Display;
using Orchard.ContentManagement.Metadata.Models;
using Orchard.ContentManagement.Metadata.Settings;
using Orchard.ContentManagement.MetaData;
using Orchard.ContentManagement.Records;
using Orchard.Contents.Services;
using Orchard.Contents.ViewModels;
using Orchard.DisplayManagement;
using Orchard.DisplayManagement.ModelBinding;
using Orchard.DisplayManagement.Notify;
using Orchard.Navigation;
using Orchard.Settings;
using YesSql.Core.Services;

namespace Orchard.Contents.Controllers
{
    public class AdminController : Controller, IUpdateModel
    {
        private readonly IContentManager _contentManager;
        private readonly IContentDefinitionManager _contentDefinitionManager;
        private readonly ISiteService _siteService;
        private readonly ISession _session;
        private readonly IContentItemDisplayManager _contentItemDisplayManager;
        private readonly INotifier _notifier;
        private readonly IAuthorizationService _authorizationService;
        private readonly IEnumerable<IContentAdminFilter> _contentAdminFilters;

        public AdminController(
            IContentManager contentManager,
            IContentItemDisplayManager contentItemDisplayManager,
            IContentDefinitionManager contentDefinitionManager,
            ISiteService siteService,
            INotifier notifier,
            ISession session,
            IShapeFactory shapeFactory,
            ILogger<AdminController> logger,
            IHtmlLocalizer<AdminController> localizer,
            IAuthorizationService authorizationService,
            IEnumerable<IContentAdminFilter> contentAdminFilters
            )
        {
            _contentAdminFilters = contentAdminFilters;
            _authorizationService = authorizationService;
            _notifier = notifier;
            _contentItemDisplayManager = contentItemDisplayManager;
            _session = session;
            _siteService = siteService;
            _contentManager = contentManager;
            _contentDefinitionManager = contentDefinitionManager;

            T = localizer;
            New = shapeFactory;
            Logger = logger;
        }

        public IHtmlLocalizer T { get; }
        public dynamic New { get; set; }

        public ILogger Logger { get; set; }

        public async Task<IActionResult> List(ListContentsViewModel model, PagerParameters pagerParameters)
        {
            var siteSettings = await _siteService.GetSiteSettingsAsync();
            Pager pager = new Pager(pagerParameters, siteSettings.PageSize);

            var query = _session.QueryAsync<ContentItem, ContentItemIndex>();

            switch (model.Options.ContentsStatus)
            {
                case ContentsStatus.Published:
                    query = query.With<ContentItemIndex>(x => x.Published);
                    break;
                case ContentsStatus.Draft:
                    query = query.With<ContentItemIndex>(x => x.Latest && !x.Published);
                    break;
                case ContentsStatus.AllVersions:
                    query = query.With<ContentItemIndex>(x => x.Latest);
                    break;
                default:
                    query = query.With<ContentItemIndex>(x => x.Latest);
                    break;
            }

            if (!string.IsNullOrEmpty(model.TypeName))
            {
                var contentTypeDefinition = _contentDefinitionManager.GetTypeDefinition(model.TypeName);
                if (contentTypeDefinition == null)
                    return NotFound();

                model.TypeDisplayName = contentTypeDefinition.ToString();

                // We display a specific type even if it's not listable so that admin pages
                // can reuse the Content list page for specific types.
                query = query.With<ContentItemIndex>(x => x.ContentType == model.TypeName);
            }
            else
            {
                var listableTypes = (await GetListableTypesAsync()).Select(t => t.Name).ToArray();
                if(listableTypes.Any())
                {
                    query = query.With<ContentItemIndex>(x => x.ContentType.IsIn(listableTypes));
                }
            }

            switch (model.Options.OrderBy)
            {
                case ContentsOrder.Modified:
                    query = query.OrderByDescending(x => x.ModifiedUtc);
                    break;
                case ContentsOrder.Published:
                    query = query.OrderByDescending(cr => cr.PublishedUtc);
                    break;
                case ContentsOrder.Created:
                    query = query.OrderByDescending(cr => cr.CreatedUtc);
                    break;
                default:
                    query = query.OrderByDescending(cr => cr.ModifiedUtc);
                    break;
            }

            //if (!String.IsNullOrWhiteSpace(model.Options.SelectedCulture))
            //{
            //    query = _cultureFilter.FilterCulture(query, model.Options.SelectedCulture);
            //}

            //if (model.Options.ContentsStatus == ContentsStatus.Owner)
            //{
            //    query = query.Where<CommonPartRecord>(cr => cr.OwnerId == Services.WorkContext.CurrentUser.Id);
            //}

            model.Options.SelectedFilter = model.TypeName;
            model.Options.FilterOptions = (await GetListableTypesAsync())
                .Select(ctd => new KeyValuePair<string, string>(ctd.Name, ctd.DisplayName))
                .ToList().OrderBy(kvp => kvp.Value);

            //model.Options.Cultures = _cultureManager.ListCultures();

            // Invoke any service that could alter the query
            await _contentAdminFilters.InvokeAsync(x => x.FilterAsync(query, model, pagerParameters, this), Logger);

            var maxPagedCount = siteSettings.MaxPagedCount;
            if (maxPagedCount > 0 && pager.PageSize > maxPagedCount)
                pager.PageSize = maxPagedCount;

            var pagerShape = New.Pager(pager).TotalItemCount(maxPagedCount > 0 ? maxPagedCount : await query.Count());
            var pageOfContentItems = await query.Skip(pager.GetStartIndex()).Take(pager.PageSize).List();

            var contentItemSummaries = new List<dynamic>();
            foreach(var contentItem in pageOfContentItems)
            {
                contentItemSummaries.Add(await _contentItemDisplayManager.BuildDisplayAsync(contentItem, this, "SummaryAdmin"));
            }

            var viewModel = New.ViewModel()
                .ContentItems(contentItemSummaries)
                .Pager(pagerShape)
                .Options(model.Options)
                .TypeDisplayName(model.TypeDisplayName ?? "");

            return View(viewModel);
        }

        private async Task<IEnumerable<ContentTypeDefinition>> GetCreatableTypesAsync()
        {
            var creatable = new List<ContentTypeDefinition>();
            foreach (var ctd in _contentDefinitionManager.ListTypeDefinitions())
            {
                if(ctd.Settings.ToObject<ContentTypeSettings>().Creatable)
                {
                    var authorized = await _authorizationService.AuthorizeAsync(User, Permissions.EditContent, _contentManager.New(ctd.Name));
                    if (authorized)
                    {
                        creatable.Add(ctd);
                    }
                }
            }
            return creatable;
        }

        private async Task<IEnumerable<ContentTypeDefinition>> GetListableTypesAsync()
        {
            var listable = new List<ContentTypeDefinition>();
            foreach (var ctd in _contentDefinitionManager.ListTypeDefinitions())
            {
                if (ctd.Settings.ToObject<ContentTypeSettings>().Listable)
                {
                    var authorized = await _authorizationService.AuthorizeAsync(User, Permissions.EditContent, _contentManager.New(ctd.Name));
                    if (authorized)
                    {
                        listable.Add(ctd);
                    }
                }
            }
            return listable;
        }

        //[HttpPost, ActionName("List")]
        //[Mvc.FormValueRequired("submit.Filter")]
        //public ActionResult ListFilterPOST(ContentOptions options)
        //{
        //    var routeValues = ControllerContext.RouteData.Values;
        //    if (options != null)
        //    {
        //        routeValues["Options.SelectedCulture"] = options.SelectedCulture; //todo: don't hard-code the key
        //        routeValues["Options.OrderBy"] = options.OrderBy; //todo: don't hard-code the key
        //        routeValues["Options.ContentsStatus"] = options.ContentsStatus; //todo: don't hard-code the key
        //        if (GetListableTypes(false).Any(ctd => string.Equals(ctd.Name, options.SelectedFilter, StringComparison.OrdinalIgnoreCase)))
        //        {
        //            routeValues["id"] = options.SelectedFilter;
        //        }
        //        else {
        //            routeValues.Remove("id");
        //        }
        //    }

        //    return RedirectToAction("List", routeValues);
        //}

        //[HttpPost, ActionName("List")]
        //[Mvc.FormValueRequired("submit.BulkEdit")]
        //public ActionResult ListPOST(ContentOptions options, IEnumerable<int> itemIds, string returnUrl)
        //{
        //    if (itemIds != null)
        //    {
        //        var checkedContentItems = _contentManager.GetMany<ContentItem>(itemIds, VersionOptions.Latest, QueryHints.Empty);
        //        switch (options.BulkAction)
        //        {
        //            case ContentsBulkAction.None:
        //                break;
        //            case ContentsBulkAction.PublishNow:
        //                foreach (var item in checkedContentItems)
        //                {
        //                    if (!await _authorizationService.Authorize(User, Permissions.PublishContent, item, T("Couldn't publish selected content.")))
        //                    {
        //                        _transactionManager.Cancel();
        //                        return Unauthorized();
        //                    }

        //                    _contentManager.Publish(item);
        //                }
        //                Services.Notifier.Information(T("Content successfully published."));
        //                break;
        //            case ContentsBulkAction.Unpublish:
        //                foreach (var item in checkedContentItems)
        //                {
        //                    if (!await _authorizationService.Authorize(User, Permissions.PublishContent, item, T("Couldn't unpublish selected content.")))
        //                    {
        //                        _transactionManager.Cancel();
        //                        return Unauthorized();
        //                    }

        //                    _contentManager.Unpublish(item);
        //                }
        //                Services.Notifier.Information(T("Content successfully unpublished."));
        //                break;
        //            case ContentsBulkAction.Remove:
        //                foreach (var item in checkedContentItems)
        //                {
        //                    if (!await _authorizationService.Authorize(User, Permissions.DeleteContent, item, T("Couldn't remove selected content.")))
        //                    {
        //                        _transactionManager.Cancel();
        //                        return Unauthorized();
        //                    }

        //                    _contentManager.Remove(item);
        //                }
        //                Services.Notifier.Information(T("Content successfully removed."));
        //                break;
        //            default:
        //                throw new ArgumentOutOfRangeException();
        //        }
        //    }

        //    return this.RedirectLocal(returnUrl, () => RedirectToAction("List"));
        //}

        //ActionResult ListableTypeList(int? containerId)
        //{
        //    var viewModel = Shape.ViewModel(ContentTypes: GetListableTypes(containerId.HasValue), ContainerId: containerId);

        //    return View("ListableTypeList", viewModel);
        //}

        public async Task<IActionResult> Create(string id)
        {
            if (String.IsNullOrWhiteSpace(id))
            {
                return NotFound();
            }

            var contentItem = _contentManager.New(id);

            if (!await _authorizationService.AuthorizeAsync(User, Permissions.EditContent, contentItem))
            {
                return Unauthorized();
            }

            var model = await _contentItemDisplayManager.BuildEditorAsync(contentItem, this);

            return View(model);
        }

        [HttpPost, ActionName("Create")]
        [FormValueRequired("submit.Save")]
        public Task<IActionResult> CreatePOST(string id, string returnUrl)
        {
            return CreatePOST(id, returnUrl, contentItem =>
            {
                var typeDefinition = _contentDefinitionManager.GetTypeDefinition(contentItem.ContentType);

                _notifier.Success(string.IsNullOrWhiteSpace(typeDefinition.DisplayName)
                    ? T["Your content draft has been saved."]
                    : T["Your {0} draft has been saved.", typeDefinition.DisplayName]);

                return Task.CompletedTask;
            });
        }

        [HttpPost, ActionName("Create")]
        [FormValueRequired("submit.Publish")]
        public async Task<IActionResult> CreateAndPublishPOST(string id, string returnUrl)
        {
            // pass a dummy content to the authorization check to check for "own" variations
            var dummyContent = _contentManager.New(id);

            if (!await _authorizationService.AuthorizeAsync(User, Permissions.PublishContent, dummyContent))
            {
                return Unauthorized();
            }

            return await CreatePOST(id, returnUrl, async contentItem => {
                await _contentManager.PublishAsync(contentItem);

                var typeDefinition = _contentDefinitionManager.GetTypeDefinition(contentItem.ContentType);

                _notifier.Success(string.IsNullOrWhiteSpace(typeDefinition.DisplayName)
                    ? T["Your content has been published."]
                    : T["Your {0} has been published.", typeDefinition.DisplayName]);
            });
        }

        private async Task<IActionResult> CreatePOST(string id, string returnUrl, Func<ContentItem, Task> conditionallyPublish)
        {
            var contentItem = _contentManager.New(id);

            if (!await _authorizationService.AuthorizeAsync(User, Permissions.EditContent, contentItem))
            {
                return Unauthorized();
            }

            var model = await _contentItemDisplayManager.UpdateEditorAsync(contentItem, this);

            if (!ModelState.IsValid)
            {
                _session.Cancel();
                return View(model);
            }

            _contentManager.Create(contentItem, VersionOptions.Draft);

            await conditionallyPublish(contentItem);

            if (!string.IsNullOrEmpty(returnUrl))
            {
                return LocalRedirect(returnUrl);
            }

            var adminRouteValues = _contentManager.PopulateAspect<ContentItemMetadata>(contentItem).AdminRouteValues;
            return RedirectToRoute(adminRouteValues);
        }

        public async Task<IActionResult> Display(string contentItemId)
        {
            var contentItem = await _contentManager.GetAsync(contentItemId, VersionOptions.Latest);

            if (contentItem == null)
            {
                return NotFound();
            }

            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ViewContent, contentItem))
            {
                return Unauthorized();
            }

            var model = await _contentItemDisplayManager.BuildDisplayAsync(contentItem, this, "DetailAdmin");

            return View(model);
        }

        public async Task<IActionResult> Edit(string contentItemId)
        {
            var contentItem = await _contentManager.GetAsync(contentItemId, VersionOptions.Latest);

            if (contentItem == null)
                return NotFound();

            if (!await _authorizationService.AuthorizeAsync(User, Permissions.EditContent, contentItem))
            {
                return Unauthorized();
            }

            var model = await _contentItemDisplayManager.BuildEditorAsync(contentItem, this);

            return View(model);
        }

        [HttpPost, ActionName("Edit")]
        [FormValueRequired("submit.Save")]
        public Task<IActionResult> EditPOST(string contentItemId, string returnUrl)
        {
            return EditPOST(contentItemId, returnUrl, contentItem =>
            {
                var typeDefinition = _contentDefinitionManager.GetTypeDefinition(contentItem.ContentType);

                _notifier.Success(string.IsNullOrWhiteSpace(typeDefinition.DisplayName)
                    ? T["Your content draft has been saved."]
                    : T["Your {0} draft has been saved.", typeDefinition.DisplayName]);

                return Task.CompletedTask;
            });
        }

        [HttpPost, ActionName("Edit")]
        [FormValueRequired("submit.Publish")]
        public async Task<IActionResult> EditAndPublishPOST(string contentItemId, string returnUrl)
        {
            var content = await _contentManager.GetAsync(contentItemId, VersionOptions.Latest);

            if (content == null)
            {
                return NotFound();
            }

            if (!await _authorizationService.AuthorizeAsync(User, Permissions.PublishContent, content))
            {
                return Unauthorized();
            }

            return await EditPOST(contentItemId, returnUrl, async contentItem =>
            {
                await _contentManager.PublishAsync(contentItem);

                var typeDefinition = _contentDefinitionManager.GetTypeDefinition(contentItem.ContentType);

                _notifier.Success(string.IsNullOrWhiteSpace(typeDefinition.DisplayName)
                    ? T["Your content has been published."]
                    : T["Your {0} has been published.", typeDefinition.DisplayName]);
            });
        }

        private async Task<IActionResult> EditPOST(string contentItemId, string returnUrl, Func<ContentItem, Task> conditionallyPublish)
        {
            var contentItem = await _contentManager.GetAsync(contentItemId, VersionOptions.DraftRequired);

            if (contentItem == null)
            {
                return NotFound();
            }

            if (!await _authorizationService.AuthorizeAsync(User, Permissions.EditContent, contentItem))
            {
                return Unauthorized();
            }

            //string previousRoute = null;
            //if (contentItem.Has<IAliasAspect>() &&
            //    !string.IsNullOrWhiteSpace(returnUrl)
            //    && Request.IsLocalUrl(returnUrl)
            //    // only if the original returnUrl is the content itself
            //    && String.Equals(returnUrl, Url.ItemDisplayUrl(contentItem), StringComparison.OrdinalIgnoreCase)
            //    )
            //{
            //    previousRoute = contentItem.As<IAliasAspect>().Path;
            //}

            var model = await _contentItemDisplayManager.UpdateEditorAsync(contentItem, this);
            if (!ModelState.IsValid)
            {
                _session.Cancel();
                return View("Edit", model);
            }

            await conditionallyPublish(contentItem);

            //if (!string.IsNullOrWhiteSpace(returnUrl)
            //    && previousRoute != null
            //    && !String.Equals(contentItem.As<IAliasAspect>().Path, previousRoute, StringComparison.OrdinalIgnoreCase))
            //{
            //    returnUrl = Url.ItemDisplayUrl(contentItem);
            //}

            var typeDefinition = _contentDefinitionManager.GetTypeDefinition(contentItem.ContentType);
            
            if (returnUrl == null)
            {
                return RedirectToAction("Edit", new RouteValueDictionary { { "ContentItemId", contentItem.ContentItemId } });
            }
            else
            {
                return LocalRedirect(returnUrl);
            }
        }

        //[HttpPost]
        //public ActionResult Clone(int id, string returnUrl)
        //{
        //    var contentItem = _contentManager.GetLatest(id);

        //    if (contentItem == null)
        //        return NotFound();

        //    if (!await _authorizationService.Authorize(User, Permissions.EditContent, contentItem, T("Couldn't clone content")))
        //        return Unauthorized();

        //    try
        //    {
        //        Services.ContentManager.Clone(contentItem);
        //    }
        //    catch (InvalidOperationException)
        //    {
        //        Services.Notifier.Warning(T("Could not clone the content item."));
        //        return this.RedirectLocal(returnUrl, () => RedirectToAction("List"));
        //    }

        //    Services.Notifier.Information(T("Successfully cloned. The clone was saved as a draft."));

        //    return this.RedirectLocal(returnUrl, () => RedirectToAction("List"));
        //}


        [HttpPost]
        public async Task<IActionResult> DiscardDraft(string contentItemId, string returnUrl)
        {
            var contentItem = await _contentManager.GetAsync(contentItemId, VersionOptions.Latest);

            if (contentItem == null || contentItem.Published)
            {
                return NotFound();
            }

            if (!await _authorizationService.AuthorizeAsync(User, Permissions.DeleteContent, contentItem))
            {
                return Unauthorized();
            }

            if (contentItem != null)
            {
                var typeDefinition = _contentDefinitionManager.GetTypeDefinition(contentItem.ContentType);

                await _contentManager.DiscardDraftAsync(contentItem);

                _notifier.Success(string.IsNullOrWhiteSpace(typeDefinition.DisplayName)
                    ? T["The draft has been removed."]
                    : T["The {0} draft has been removed.", typeDefinition.DisplayName]);
            }

            return Url.IsLocalUrl(returnUrl) ? (IActionResult)LocalRedirect(returnUrl) : RedirectToAction("List");
        }

        [HttpPost]
        public async Task<IActionResult> Remove(string contentItemId, string returnUrl)
        {
            var contentItem = await _contentManager.GetAsync(contentItemId, VersionOptions.Latest);

            if (!await _authorizationService.AuthorizeAsync(User, Permissions.DeleteContent, contentItem))
            {
                return Unauthorized();
            }

            if (contentItem != null)
            {
                var typeDefinition = _contentDefinitionManager.GetTypeDefinition(contentItem.ContentType);

                await _contentManager.RemoveAsync(contentItem);

                _notifier.Success(string.IsNullOrWhiteSpace(typeDefinition.DisplayName)
                    ? T["That content has been removed."]
                    : T["That {0} has been removed.", typeDefinition.DisplayName]);
            }

            return Url.IsLocalUrl(returnUrl) ? (IActionResult)LocalRedirect(returnUrl) : RedirectToAction("List");
        }

        [HttpPost]
        public async Task<IActionResult> Publish(string contentItemId, string returnUrl)
        {
            var contentItem = await _contentManager.GetAsync(contentItemId, VersionOptions.Latest);
            if (contentItem == null)
            {
                return NotFound();
            }

            if (!await _authorizationService.AuthorizeAsync(User, Permissions.PublishContent, contentItem))
            {
                return Unauthorized();
            }

            await _contentManager.PublishAsync(contentItem);

            var typeDefinition = _contentDefinitionManager.GetTypeDefinition(contentItem.ContentType);

            if (string.IsNullOrEmpty(typeDefinition.DisplayName))
            {
                _notifier.Success(T["That content has been published."]);
            }
            else
            {
                _notifier.Success(T["That {0} has been published.", typeDefinition.DisplayName]);
            }

            return Url.IsLocalUrl(returnUrl) ? (IActionResult)LocalRedirect(returnUrl) : RedirectToAction("List");
        }

        [HttpPost]
        public async Task<IActionResult> Unpublish(string contentItemId, string returnUrl)
        {
            var contentItem = await _contentManager.GetAsync(contentItemId, VersionOptions.Latest);
            if (contentItem == null)
            {
                return NotFound();
            }

            if (!await _authorizationService.AuthorizeAsync(User, Permissions.PublishContent, contentItem))
            {
                return Unauthorized();
            }

            await _contentManager.UnpublishAsync(contentItem);

            var typeDefinition = _contentDefinitionManager.GetTypeDefinition(contentItem.ContentType);

            if (string.IsNullOrEmpty(typeDefinition.DisplayName))
            {
                _notifier.Success(T["The content has been unpublished."]);
            }
            else
            {
                _notifier.Success(T["The {0} has been unpublished.", typeDefinition.DisplayName]);
            }

            return Url.IsLocalUrl(returnUrl) ? (IActionResult)LocalRedirect(returnUrl) : RedirectToAction("List");
        }
    }
}
