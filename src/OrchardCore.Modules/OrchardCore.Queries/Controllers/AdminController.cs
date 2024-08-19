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
using OrchardCore.Queries.ViewModels;
using OrchardCore.Routing;
using YesSql.Services;

namespace OrchardCore.Queries.Controllers;

[Admin("Queries/{action}/{id?}", "Queries{action}")]
public sealed class AdminController : Controller
{
    private const string _optionsSearch = "Options.Search";

    private readonly IAuthorizationService _authorizationService;
    private readonly PagerOptions _pagerOptions;
    private readonly INotifier _notifier;
    private readonly IServiceProvider _serviceProvider;
    private readonly IQueryManager _queryManager;
    private readonly IDisplayManager<Query> _displayManager;
    private readonly IUpdateModelAccessor _updateModelAccessor;
    private readonly IShapeFactory _shapeFactory;

    internal readonly IStringLocalizer S;
    internal readonly IHtmlLocalizer H;

    public AdminController(
        IDisplayManager<Query> displayManager,
        IAuthorizationService authorizationService,
        IOptions<PagerOptions> pagerOptions,
        IShapeFactory shapeFactory,
        IStringLocalizer<AdminController> stringLocalizer,
        IHtmlLocalizer<AdminController> htmlLocalizer,
        INotifier notifier,
        IQueryManager queryManager,
        IServiceProvider serviceProvider,
        IUpdateModelAccessor updateModelAccessor)
    {
        _displayManager = displayManager;
        _authorizationService = authorizationService;
        _pagerOptions = pagerOptions.Value;
        _queryManager = queryManager;
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

        // Maintain previous route data when generating page links.
        var routeData = new RouteData();

        if (!string.IsNullOrEmpty(options.Search))
        {
            routeData.Values.TryAdd(_optionsSearch, options.Search);
        }

        var result = await _queryManager.PageQueriesAsync(pager.Page, pager.PageSize, new QueryContext()
        {
            Name = options.Search,
        });

        var model = new QueriesIndexViewModel
        {
            Queries = [],
            Options = options,
            Pager = await _shapeFactory.PagerAsync(pager, result.Count, routeData),
            QuerySourceNames = _serviceProvider.GetServices<IQuerySource>().Select(x => x.Name).ToArray(),
        };

        foreach (var query in result.Records)
        {
            model.Queries.Add(new QueryEntry
            {
                Query = query,
                Shape = await _displayManager.BuildDisplayAsync(query, _updateModelAccessor.ModelUpdater, "SummaryAdmin")
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

        var query = await _queryManager.NewAsync(id);

        if (query == null)
        {
            return NotFound();
        }

        var model = new QueriesCreateViewModel
        {
            Editor = await _displayManager.BuildEditorAsync(query, _updateModelAccessor.ModelUpdater, true),
            SourceName = id
        };

        return View(model);
    }

    [HttpPost]
    [ActionName(nameof(Create))]
    public async Task<IActionResult> CreatePost(QueriesCreateViewModel model)
    {
        if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageQueries))
        {
            return Forbid();
        }

        var query = await _queryManager.NewAsync(model.SourceName);

        if (query == null)
        {
            return NotFound();
        }

        var editor = await _displayManager.UpdateEditorAsync(query, _updateModelAccessor.ModelUpdater, true);

        if (ModelState.IsValid)
        {
            await _queryManager.SaveAsync(query);

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

        var query = await _queryManager.GetQueryAsync(id);

        if (query == null)
        {
            return NotFound();
        }

        var model = new QueriesEditViewModel
        {
            QueryId = id,
            Name = query.Name,
            Editor = await _displayManager.BuildEditorAsync(query, _updateModelAccessor.ModelUpdater, false)
        };

        return View(model);
    }

    [HttpPost]
    [ActionName(nameof(Edit))]
    public async Task<IActionResult> EditPost(QueriesEditViewModel model)
    {
        if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageQueries))
        {
            return Forbid();
        }

        if (string.IsNullOrEmpty(model.QueryId))
        {
            return BadRequest();
        }

        var query = await _queryManager.GetQueryAsync(model.QueryId);

        if (query == null)
        {
            return NotFound();
        }

        var editor = await _displayManager.UpdateEditorAsync(query, _updateModelAccessor.ModelUpdater, false);

        if (ModelState.IsValid)
        {
            await _queryManager.DeleteQueryAsync(model.QueryId);
            await _queryManager.UpdateAsync(query);
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

        if (!await _queryManager.DeleteQueryAsync(id))
        {
            return NotFound();
        }

        await _notifier.SuccessAsync(H["Query deleted successfully."]);

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ActionName(nameof(Index))]
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
                    await _queryManager.DeleteQueryAsync(itemIds.ToArray());
                    await _notifier.SuccessAsync(H["Queries successfully removed."]);
                    break;
                default:
                    return BadRequest();
            }
        }

        return RedirectToAction(nameof(Index));
    }
}
