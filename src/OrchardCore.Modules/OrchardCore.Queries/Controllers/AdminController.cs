using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Localization;
using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Notify;
using OrchardCore.Navigation;
using OrchardCore.Queries.ViewModels;
using OrchardCore.Settings;
using YesSql;

namespace OrchardCore.Queries.Controllers
{
    public class AdminController : Controller, IUpdateModel
    {
        private readonly IAuthorizationService _authorizationService;
        private readonly ISiteService _siteService;
        private readonly INotifier _notifier;
        private readonly IQueryManager _queryManager;
        private readonly IEnumerable<IQuerySource> _querySources;
        private readonly IDisplayManager<Query> _displayManager;
        private readonly ISession _session;

        public AdminController(
            IDisplayManager<Query> displayManager,
            IAuthorizationService authorizationService,
            ISiteService siteService,
            IShapeFactory shapeFactory,
            IStringLocalizer<AdminController> stringLocalizer,
            IHtmlLocalizer<AdminController> htmlLocalizer,
            INotifier notifier,
            IQueryManager queryManager,
            IEnumerable<IQuerySource> querySources,
            ISession session)
        {
            _session = session;
            _displayManager = displayManager;
            _authorizationService = authorizationService;
            _siteService = siteService;
            _queryManager = queryManager;
            _querySources = querySources;
            New = shapeFactory;
            _notifier = notifier;

            T = stringLocalizer;
            H = htmlLocalizer;
        }

        public dynamic New { get; set; }
        public IStringLocalizer T { get; set; }
        public IHtmlLocalizer H { get; set; }

        public async Task<IActionResult> Index(QueryIndexOptions options, PagerParameters pagerParameters)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageQueries))
            {
                return Unauthorized();
            }

            var siteSettings = await _siteService.GetSiteSettingsAsync();
            var pager = new Pager(pagerParameters, siteSettings.PageSize);

            // default options
            if (options == null)
            {
                options = new QueryIndexOptions();
            }

            var queries = await _queryManager.ListQueriesAsync();

            if (!string.IsNullOrWhiteSpace(options.Search))
            {
                queries = queries.Where(q => q.Name.IndexOf(options.Search, StringComparison.OrdinalIgnoreCase) >= 0);
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
                model.Queries.Add(new QueryEntry {
                    Query = query,
                    Shape = await _displayManager.BuildDisplayAsync(query, this, "SummaryAdmin")
                });
            }

            return View(model);
        }

        public async Task<IActionResult> Create(string id)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageQueries))
            {
                return Unauthorized();
            }
            
            var query = _querySources.FirstOrDefault(x => x.Name == id)?.Create();

            if (query == null)
            {
                return NotFound();
            }

            var model = new QueriesCreateViewModel
            {
                Editor = await _displayManager.BuildEditorAsync(query, updater: this, isNew: true),
                SourceName = id
            };

            return View(model);
        }

        [HttpPost, ActionName(nameof(Create))]
        public async Task<IActionResult> CreatePost(QueriesCreateViewModel model)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageQueries))
            {
                return Unauthorized();
            }
            
            var query = _querySources.FirstOrDefault(x => x.Name == model.SourceName)?.Create();

            if (query == null)
            {
                return NotFound();
            }

            var editor = await _displayManager.UpdateEditorAsync(query, updater: this, isNew: true);

            if (ModelState.IsValid)
            {
                await _queryManager.SaveQueryAsync(query.Name, query);

                _notifier.Success(H["Query created successfully"]);
                return RedirectToAction("Index");
            }

            // If we got this far, something failed, redisplay form
            model.Editor = editor;

            return View(model);
        }

        public async Task<IActionResult> Edit(string id)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageQueries))
            {
                return Unauthorized();
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
                Editor = await _displayManager.BuildEditorAsync(query, updater: this, isNew: false)
            };   

            return View(model);
        }

        [HttpPost, ActionName("Edit")]
        public async Task<IActionResult> EditPost(QueriesEditViewModel model)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageQueries))
            {
                return Unauthorized();
            }

            var query = await _queryManager.GetQueryAsync(model.Name);

            if (query == null)
            {
                return NotFound();
            }

            var editor = await _displayManager.UpdateEditorAsync(query, updater: this, isNew: false);

            if (ModelState.IsValid)
            {
                await _queryManager.SaveQueryAsync(model.Name, query);

                _notifier.Success(H["Query updated successfully"]);
                return RedirectToAction("Index");
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
                return Unauthorized();
            }

            var query = await _queryManager.GetQueryAsync(id);

            if (query == null)
            {
                return NotFound();
            }

            await _queryManager.DeleteQueryAsync(id);

            _notifier.Success(H["Query deleted successfully"]);

            return RedirectToAction("Index");
        }
    }
}
