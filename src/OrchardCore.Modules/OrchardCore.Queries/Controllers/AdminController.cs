using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Notify;
using OrchardCore.Navigation;
using OrchardCore.Queries.ViewModels;
using OrchardCore.Routing;

namespace OrchardCore.Queries.Controllers
{
    public class AdminController : Controller
    {
        private readonly IAuthorizationService _authorizationService;
        private readonly PagerOptions _pagerOptions;
        private readonly INotifier _notifier;
        private readonly IQueryManager _queryManager;
        private readonly IEnumerable<IQuerySource> _querySources;
        private readonly IDisplayManager<Query> _displayManager;
        private readonly IUpdateModelAccessor _updateModelAccessor;
        protected readonly IStringLocalizer S;
        protected readonly IHtmlLocalizer H;
        protected readonly dynamic New;

        public AdminController(
            IDisplayManager<Query> displayManager,
            IAuthorizationService authorizationService,
            IOptions<PagerOptions> pagerOptions,
            IShapeFactory shapeFactory,
            IStringLocalizer<AdminController> stringLocalizer,
            IHtmlLocalizer<AdminController> htmlLocalizer,
            INotifier notifier,
            IQueryManager queryManager,
            IEnumerable<IQuerySource> querySources,
            IUpdateModelAccessor updateModelAccessor)
        {
            _displayManager = displayManager;
            _authorizationService = authorizationService;
            _pagerOptions = pagerOptions.Value;
            _queryManager = queryManager;
            _querySources = querySources;
            _updateModelAccessor = updateModelAccessor;
            New = shapeFactory;
            _notifier = notifier;
            S = stringLocalizer;
            H = htmlLocalizer;
        }

        public async Task<IActionResult> Index(ContentOptions options, PagerParameters pagerParameters)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageQueries))
            {
                return Forbid();
            }

            var pager = new Pager(pagerParameters, _pagerOptions.GetPageSize());

            var queries = await _queryManager.ListQueriesAsync();
            queries = queries.OrderBy(x => x.Name);

            if (!String.IsNullOrWhiteSpace(options.Search))
            {
                queries = queries.Where(q => q.Name.Contains(options.Search, StringComparison.OrdinalIgnoreCase));
            }

            var results = queries
                .Skip(pager.GetStartIndex())
                .Take(pager.PageSize)
                .ToList();

            // Maintain previous route data when generating page links
            var routeData = new RouteData();
            routeData.Values.Add("Options.Search", options.Search);

            var pagerShape = (await New.Pager(pager)).TotalItemCount(queries.Count()).RouteData(routeData);

            var model = new QueriesIndexViewModel
            {
                Queries = new List<QueryEntry>(),
                Options = options,
                Pager = pagerShape,
                QuerySourceNames = _querySources.Select(x => x.Name).ToList()
            };

            foreach (var query in results)
            {
                model.Queries.Add(new QueryEntry
                {
                    Query = query,
                    Shape = await _displayManager.BuildDisplayAsync(query, _updateModelAccessor.ModelUpdater, "SummaryAdmin")
                });
            }

            model.Options.ContentsBulkAction = new List<SelectListItem>() {
                new SelectListItem() { Text = S["Delete"], Value = nameof(ContentsBulkAction.Remove) }
            };

            return View(model);
        }

        [HttpPost, ActionName("Index")]
        [FormValueRequired("submit.Filter")]
        public ActionResult IndexFilterPOST(QueriesIndexViewModel model)
        {
            return RedirectToAction(nameof(Index), new RouteValueDictionary {
                { "Options.Search", model.Options.Search }
            });
        }

        public async Task<IActionResult> Create(string id)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageQueries))
            {
                return Forbid();
            }

            var query = _querySources.FirstOrDefault(x => x.Name == id)?.Create();

            if (query == null)
            {
                return NotFound();
            }

            var model = new QueriesCreateViewModel
            {
                Editor = await _displayManager.BuildEditorAsync(query, updater: _updateModelAccessor.ModelUpdater, isNew: true, "", ""),
                SourceName = id
            };

            return View(model);
        }

        [HttpPost, ActionName(nameof(Create))]
        public async Task<IActionResult> CreatePost(QueriesCreateViewModel model)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageQueries))
            {
                return Forbid();
            }

            var query = _querySources.FirstOrDefault(x => x.Name == model.SourceName)?.Create();

            if (query == null)
            {
                return NotFound();
            }

            var editor = await _displayManager.UpdateEditorAsync(query, updater: _updateModelAccessor.ModelUpdater, isNew: true, "", "");

            if (ModelState.IsValid)
            {
                await _queryManager.SaveQueryAsync(query.Name, query);

                await _notifier.SuccessAsync(H["Query created successfully."]);
                return RedirectToAction(nameof(Index));
            }

            // If we got this far, something failed, redisplay form
            model.Editor = editor;

            return View(model);
        }

        public async Task<IActionResult> Edit(string id)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageQueries))
            {
                return Forbid();
            }

            var query = await _queryManager.GetQueryAsync(id);

            if (query == null)
            {
                return NotFound();
            }

            var model = new QueriesEditViewModel
            {
                SourceName = query.Source,
                Name = query.Name,
                Schema = query.Schema,
                Editor = await _displayManager.BuildEditorAsync(query, updater: _updateModelAccessor.ModelUpdater, isNew: false, "", "")
            };

            return View(model);
        }

        [HttpPost, ActionName("Edit")]
        public async Task<IActionResult> EditPost(QueriesEditViewModel model)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageQueries))
            {
                return Forbid();
            }

            var query = (await _queryManager.LoadQueryAsync(model.Name));

            if (query == null)
            {
                return NotFound();
            }

            var editor = await _displayManager.UpdateEditorAsync(query, updater: _updateModelAccessor.ModelUpdater, isNew: false, "", "");

            if (ModelState.IsValid)
            {
                await _queryManager.SaveQueryAsync(model.Name, query);

                await _notifier.SuccessAsync(H["Query updated successfully."]);
                return RedirectToAction(nameof(Index));
            }

            model.Editor = editor;

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Delete(string id)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageQueries))
            {
                return Forbid();
            }

            var query = await _queryManager.LoadQueryAsync(id);

            if (query == null)
            {
                return NotFound();
            }

            await _queryManager.DeleteQueryAsync(id);

            await _notifier.SuccessAsync(H["Query deleted successfully."]);

            return RedirectToAction(nameof(Index));
        }

        [HttpPost, ActionName("Index")]
        [FormValueRequired("submit.BulkAction")]
        public async Task<ActionResult> IndexPost(ViewModels.ContentOptions options, IEnumerable<string> itemIds)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageQueries))
            {
                return Forbid();
            }

            if (itemIds?.Count() > 0)
            {
                var queriesList = await _queryManager.ListQueriesAsync();
                var checkedContentItems = queriesList.Where(x => itemIds.Contains(x.Name));
                switch (options.BulkAction)
                {
                    case ContentsBulkAction.None:
                        break;
                    case ContentsBulkAction.Remove:
                        foreach (var item in checkedContentItems)
                        {
                            await _queryManager.DeleteQueryAsync(item.Name);
                        }
                        await _notifier.SuccessAsync(H["Queries successfully removed."]);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(options.BulkAction), "Invalid bulk action.");
                }
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
