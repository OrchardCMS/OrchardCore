using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using OrchardCore.Admin;
using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Notify;
using OrchardCore.Navigation;
using OrchardCore.Queries.Indexes;
using OrchardCore.Queries.ViewModels;
using OrchardCore.Routing;
using YesSql;
using YesSql.Services;

namespace OrchardCore.Queries.Controllers
{
    [Admin("Queries/{action}/{id?}", "Queries{action}")]
    public class AdminController : Controller
    {
        private const string _optionsSearch = "Options.Search";

        private readonly IAuthorizationService _authorizationService;
        private readonly PagerOptions _pagerOptions;
        private readonly INotifier _notifier;
        private readonly IServiceProvider _serviceProvider;
        private readonly ISession _session;
        private readonly IDisplayManager<Query> _displayManager;
        private readonly IUpdateModelAccessor _updateModelAccessor;
        private readonly IShapeFactory _shapeFactory;

        protected readonly IStringLocalizer S;
        protected readonly IHtmlLocalizer H;

        public AdminController(
            ISession session,
            IDisplayManager<Query> displayManager,
            IAuthorizationService authorizationService,
            IOptions<PagerOptions> pagerOptions,
            IShapeFactory shapeFactory,
            IStringLocalizer<AdminController> stringLocalizer,
            IHtmlLocalizer<AdminController> htmlLocalizer,
            INotifier notifier,
            IServiceProvider serviceProvider,
            IUpdateModelAccessor updateModelAccessor)
        {
            _session = session;
            _displayManager = displayManager;
            _authorizationService = authorizationService;
            _pagerOptions = pagerOptions.Value;
            _serviceProvider = serviceProvider;
            _updateModelAccessor = updateModelAccessor;
            _shapeFactory = shapeFactory;
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

            var query = _session.Query<Query, QueryIndex>();

            // Maintain previous route data when generating page links.
            var routeData = new RouteData();

            if (!string.IsNullOrEmpty(options.Search))
            {
                routeData.Values.TryAdd(_optionsSearch, options.Search);

                query = query.Where(x => x.Name != null && x.Name.Contains(options.Search));
            }

            var skip = (pager.Page - 1) * pager.PageSize;

            var count = await query.CountAsync();
            var queries = await query.Skip(skip).Take(pager.PageSize).ListAsync();

            var model = new QueriesIndexViewModel
            {
                Queries = [],
                Options = options,
                Pager = await _shapeFactory.PagerAsync(pager, count, routeData),
                QuerySourceNames = _serviceProvider.GetServices<IQuerySource>().Select(x => x.Name).ToList()
            };

            foreach (var qry in queries)
            {
                model.Queries.Add(new QueryEntry
                {
                    Query = qry,
                    Shape = await _displayManager.BuildDisplayAsync(qry, _updateModelAccessor.ModelUpdater, "SummaryAdmin")
                });
            }

            model.Options.ContentsBulkAction =
            [
                new SelectListItem(S["Delete"], nameof(ContentsBulkAction.Remove)),
            ];

            return View(model);
        }

        [HttpPost]
        [ActionName(nameof(Index))]
        [FormValueRequired("submit.Filter")]
        public ActionResult IndexFilterPOST(QueriesIndexViewModel model)
            => RedirectToAction(nameof(Index), new RouteValueDictionary
            {
                { _optionsSearch, model.Options.Search }
            });

        public async Task<IActionResult> Create(string id)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageQueries))
            {
                return Forbid();
            }

            var query = _serviceProvider.GetKeyedService<IQuerySource>(id)?.Create();

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

            var query = _serviceProvider.GetKeyedService<IQuerySource>(model.SourceName)?.Create();

            if (query == null)
            {
                return NotFound();
            }

            var editor = await _displayManager.UpdateEditorAsync(query, updater: _updateModelAccessor.ModelUpdater, isNew: true, "", "");

            if (ModelState.IsValid)
            {
                await DeleteQueryInternalAsync(query.Name, false);
                await _session.SaveAsync(query);
                await _session.SaveChangesAsync();

                await _notifier.SuccessAsync(H["Query created successfully."]);

                return RedirectToAction(nameof(Index));
            }

            // If we got this far, something failed, redisplay form.
            model.Editor = editor;

            return View(model);
        }

        public async Task<IActionResult> Edit(string id)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageQueries))
            {
                return Forbid();
            }

            var query = await _session.Query<Query, QueryIndex>(x => x.Name == id).FirstOrDefaultAsync();

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

        [HttpPost, ActionName(nameof(Edit))]
        public async Task<IActionResult> EditPost(QueriesEditViewModel model)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageQueries))
            {
                return Forbid();
            }

            var query = await _session.Query<Query, QueryIndex>(x => x.Name == model.Name).FirstOrDefaultAsync();

            if (query == null)
            {
                return NotFound();
            }

            var editor = await _displayManager.UpdateEditorAsync(query, updater: _updateModelAccessor.ModelUpdater, isNew: false, string.Empty, string.Empty);

            if (ModelState.IsValid)
            {
                await _session.SaveAsync(query);
                await _session.SaveChangesAsync();
                await _notifier.SuccessAsync(H["Query updated successfully."]);

                return RedirectToAction(nameof(Index));
            }

            model.Editor = editor;

            // If we got this far, something failed, redisplay form.
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Delete(string id)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageQueries))
            {
                return Forbid();
            }

            if (!await DeleteQueryInternalAsync(id, true))
            {
                return NotFound();
            }

            await _notifier.SuccessAsync(H["Query deleted successfully."]);

            return RedirectToAction(nameof(Index));
        }

        [HttpPost, ActionName(nameof(Index))]
        [FormValueRequired("submit.BulkAction")]
        public async Task<ActionResult> IndexPost(ContentOptions options, IEnumerable<string> itemIds)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageQueries))
            {
                return Forbid();
            }

            if (itemIds?.Count() > 0)
            {
                switch (options.BulkAction)
                {
                    case ContentsBulkAction.None:
                        break;
                    case ContentsBulkAction.Remove:
                        var checkedContentItems = await _session.Query<Query, QueryIndex>(x => x.Name != null && x.Name.IsIn(itemIds)).ListAsync();

                        foreach (var item in checkedContentItems)
                        {
                            _session.Delete(item);
                        }

                        await _session.SaveChangesAsync();
                        await _notifier.SuccessAsync(H["Queries successfully removed."]);
                        break;
                    default:
                        return BadRequest();
                }
            }

            return RedirectToAction(nameof(Index));
        }

        private async Task<bool> DeleteQueryInternalAsync(string name, bool commit)
        {
            var queries = await _session.Query<Query, QueryIndex>(x => x.Name == name).ListAsync();

            var hasQueries = queries.Any();

            foreach (var query in queries)
            {
                _session.Delete(query);
            }

            if (commit && hasQueries)
            {
                await _session.SaveChangesAsync();
            }

            return hasQueries;
        }
    }
}
