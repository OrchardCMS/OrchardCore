using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using OrchardCore.Admin;
using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Notify;
using OrchardCore.Indexing.Core;
using OrchardCore.Indexing.Models;
using OrchardCore.Indexing.ViewModels;
using OrchardCore.Navigation;
using OrchardCore.Routing;

namespace OrchardCore.Indexing.Controllers;

public sealed class AdminController : Controller
{
    private const string _optionsSearch = "Options.Search";

    private readonly IAuthorizationService _authorizationService;
    private readonly IUpdateModelAccessor _updateModelAccessor;
    private readonly IIndexEntityManager _indexManager;
    private readonly IndexingOptions _indexingOptions;
    private readonly IDisplayManager<IndexEntity> _displayManager;
    private readonly INotifier _notifier;

    internal readonly IHtmlLocalizer H;
    internal readonly IStringLocalizer S;

    public AdminController(
        IAuthorizationService authorizationService,
        IUpdateModelAccessor updateModelAccessor,
        IIndexEntityManager indexManager,
        IDisplayManager<IndexEntity> displayManager,
        IOptions<IndexingOptions> indexingOptions,
        INotifier notifier,
        IHtmlLocalizer<AdminController> htmlLocalizer,
        IStringLocalizer<AdminController> stringLocalizer)
    {
        _authorizationService = authorizationService;
        _updateModelAccessor = updateModelAccessor;
        _indexManager = indexManager;
        _displayManager = displayManager;
        _indexingOptions = indexingOptions.Value;
        _notifier = notifier;
        H = htmlLocalizer;
        S = stringLocalizer;
    }

    [Admin("indexing", "IndexingIndex")]
    public async Task<IActionResult> Index(
        ModelOptions options,
        PagerParameters pagerParameters,
        [FromServices] IOptions<PagerOptions> pagerOptions,
        [FromServices] IShapeFactory shapeFactory)
    {
        if (!await _authorizationService.AuthorizeAsync(User, IndexingPermissions.ManageIndexes))
        {
            return Forbid();
        }

        var pager = new Pager(pagerParameters, pagerOptions.Value.GetPageSize());

        var result = await _indexManager.PageAsync(pager.Page, pager.PageSize, new QueryContext
        {
            Sorted = true,
            Name = options.Search,
        });

        // Maintain previous route data when generating page links.
        var routeData = new RouteData();

        if (!string.IsNullOrEmpty(options.Search))
        {
            routeData.Values.TryAdd(_optionsSearch, options.Search);
        }

        var viewModel = new ListSourceModelEntryViewModel<IndexEntity, IndexEntityKey>
        {
            Models = [],
            Options = options,
            Pager = await shapeFactory.PagerAsync(pager, result.Count, routeData),
            Sources = _indexingOptions.Sources.Select(x => x.Key)
            .OrderBy(x => x.ProviderName)
            .ThenBy(x => x.Type),
        };

        foreach (var record in result.Models)
        {
            viewModel.Models.Add(new ModelEntry<IndexEntity>
            {
                Model = record,
                Shape = await _displayManager.BuildDisplayAsync(record, _updateModelAccessor.ModelUpdater, "SummaryAdmin"),
            });
        }

        viewModel.Options.BulkActions =
        [
            new SelectListItem(S["Delete"], nameof(ModelAction.Remove)),
        ];

        return View(viewModel);
    }

    [HttpPost]
    [ActionName(nameof(Index))]
    [FormValueRequired("submit.Filter")]
    [Admin("indexing", "IndexingIndex")]
    public ActionResult IndexFilterPost(ListModelViewModel model)
    {
        return RedirectToAction(nameof(Index), new RouteValueDictionary
        {
            { _optionsSearch, model.Options?.Search },
        });
    }

    [Admin("indexing/create/{source}/{type}", "IndexingCreate")]
    public async Task<ActionResult> Create(string source, string type)
    {
        if (!await _authorizationService.AuthorizeAsync(User, IndexingPermissions.ManageIndexes))
        {
            return Forbid();
        }

        if (!_indexingOptions.Sources.TryGetValue(new IndexEntityKey(source, type), out var service))
        {
            await _notifier.ErrorAsync(H["Unable to find a provider named '{0}' with the type '{1}'.", source, type]);

            return RedirectToAction(nameof(Index));
        }

        var dataSource = await _indexManager.NewAsync(source, type);

        if (dataSource == null)
        {
            await _notifier.ErrorAsync(H["Invalid provider or type."]);

            return RedirectToAction(nameof(Index));
        }

        var model = new ModelViewModel
        {
            DisplayName = service.DisplayName,
            Editor = await _displayManager.BuildEditorAsync(dataSource, _updateModelAccessor.ModelUpdater, isNew: true),
        };

        return View(model);
    }

    [HttpPost]
    [ActionName(nameof(Create))]
    [Admin("indexing/create/{source}/{type}", "IndexingCreate")]
    public async Task<ActionResult> CreatePost(string source, string type)
    {
        if (!await _authorizationService.AuthorizeAsync(User, IndexingPermissions.ManageIndexes))
        {
            return Forbid();
        }

        if (!_indexingOptions.Sources.TryGetValue(new IndexEntityKey(source, type), out var service))
        {
            await _notifier.ErrorAsync(H["Unable to find a provider with the name '{0}'.", source]);

            return RedirectToAction(nameof(Index));
        }

        var deployment = await _indexManager.NewAsync(source, type);

        if (deployment == null)
        {
            await _notifier.ErrorAsync(H["Invalid provider or type."]);

            return RedirectToAction(nameof(Index));
        }

        var model = new ModelViewModel
        {
            DisplayName = service.DisplayName,
            Editor = await _displayManager.UpdateEditorAsync(deployment, _updateModelAccessor.ModelUpdater, isNew: true),
        };

        if (ModelState.IsValid)
        {
            await _indexManager.CreateAsync(deployment);

            await _notifier.SuccessAsync(H["An index has been created successfully."]);

            return RedirectToAction(nameof(Index));
        }

        return View(model);
    }

    [Admin("indexing/edit/{id}", "IndexingEdit")]
    public async Task<ActionResult> Edit(string id)
    {
        if (!await _authorizationService.AuthorizeAsync(User, IndexingPermissions.ManageIndexes))
        {
            return Forbid();
        }

        var deployment = await _indexManager.FindByIdAsync(id);

        if (deployment == null)
        {
            return NotFound();
        }

        var model = new ModelViewModel
        {
            DisplayName = deployment.DisplayText,
            Editor = await _displayManager.BuildEditorAsync(deployment, _updateModelAccessor.ModelUpdater, isNew: false),
        };

        return View(model);
    }

    [HttpPost]
    [ActionName(nameof(Edit))]
    [Admin("indexing/edit/{id}", "IndexingEdit")]
    public async Task<ActionResult> EditPost(string id)
    {
        if (!await _authorizationService.AuthorizeAsync(User, IndexingPermissions.ManageIndexes))
        {
            return Forbid();
        }

        var deployment = await _indexManager.FindByIdAsync(id);

        if (deployment == null)
        {
            return NotFound();
        }

        // Clone the deployment to prevent modifying the original instance in the store.
        var mutableProfile = deployment.Clone();

        var model = new ModelViewModel
        {
            DisplayName = mutableProfile.DisplayText,
            Editor = await _displayManager.UpdateEditorAsync(mutableProfile, _updateModelAccessor.ModelUpdater, isNew: false),
        };

        if (ModelState.IsValid)
        {
            await _indexManager.UpdateAsync(mutableProfile);

            await _notifier.SuccessAsync(H["An index has been updated successfully."]);

            return RedirectToAction(nameof(Index));
        }

        return View(model);
    }

    [HttpPost]
    [Admin("indexing/delete/{id}", "IndexingDelete")]
    public async Task<IActionResult> Delete(string id)
    {
        if (!await _authorizationService.AuthorizeAsync(User, IndexingPermissions.ManageIndexes))
        {
            return Forbid();
        }

        var deployment = await _indexManager.FindByIdAsync(id);

        if (deployment == null)
        {
            return NotFound();
        }

        await _indexManager.DeleteAsync(deployment);

        await _notifier.SuccessAsync(H["An index has been deleted successfully."]);

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ActionName(nameof(Index))]
    [FormValueRequired("submit.BulkAction")]
    [Admin("indexing", "IndexingIndex")]
    public async Task<ActionResult> IndexPost(ModelOptions options, IEnumerable<string> itemIds)
    {
        if (!await _authorizationService.AuthorizeAsync(User, IndexingPermissions.ManageIndexes))
        {
            return Forbid();
        }

        if (itemIds?.Count() > 0)
        {
            switch (options.BulkAction)
            {
                case ModelAction.None:
                    break;
                case ModelAction.Remove:
                    var counter = 0;
                    foreach (var id in itemIds)
                    {
                        var dataSource = await _indexManager.FindByIdAsync(id);

                        if (dataSource == null)
                        {
                            continue;
                        }

                        if (await _indexManager.DeleteAsync(dataSource))
                        {
                            counter++;
                        }
                    }
                    if (counter == 0)
                    {
                        await _notifier.WarningAsync(H["No data sources were removed."]);
                    }
                    else
                    {
                        await _notifier.SuccessAsync(H.Plural(counter, "1 data source has been removed successfully.", "{0} data sources have been removed successfully."));
                    }
                    break;
                default:
                    return BadRequest();
            }
        }

        return RedirectToAction(nameof(Index));
    }
}
