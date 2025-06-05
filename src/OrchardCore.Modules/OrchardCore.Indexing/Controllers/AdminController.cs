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
using OrchardCore.Indexing.Core;
using OrchardCore.Indexing.Models;
using OrchardCore.Infrastructure.Entities;
using OrchardCore.Navigation;
using OrchardCore.Routing;

namespace OrchardCore.Indexing.Controllers;

public sealed class AdminController : Controller
{
    private const string _optionsSearch = "Options.Search";

    private readonly IAuthorizationService _authorizationService;
    private readonly IUpdateModelAccessor _updateModelAccessor;
    private readonly IIndexProfileManager _indexProfileManager;
    private readonly IndexingOptions _indexingOptions;
    private readonly IDisplayManager<IndexProfile> _displayManager;
    private readonly IServiceProvider _serviceProvider;
    private readonly INotifier _notifier;

    internal readonly IHtmlLocalizer H;
    internal readonly IStringLocalizer S;

    public AdminController(
        IAuthorizationService authorizationService,
        IUpdateModelAccessor updateModelAccessor,
        IIndexProfileManager indexProfileManager,
        IDisplayManager<IndexProfile> displayManager,
        IOptions<IndexingOptions> indexingOptions,
        IServiceProvider serviceProvider,
        INotifier notifier,
        IHtmlLocalizer<AdminController> htmlLocalizer,
        IStringLocalizer<AdminController> stringLocalizer)
    {
        _authorizationService = authorizationService;
        _updateModelAccessor = updateModelAccessor;
        _indexProfileManager = indexProfileManager;
        _displayManager = displayManager;
        _serviceProvider = serviceProvider;
        _indexingOptions = indexingOptions.Value;
        _notifier = notifier;
        H = htmlLocalizer;
        S = stringLocalizer;
    }

    [Admin("indexing", "IndexingIndex")]
    public async Task<IActionResult> Index(
        IndexingEntityOptions options,
        PagerParameters pagerParameters,
        [FromServices] IOptions<PagerOptions> pagerOptions,
        [FromServices] IShapeFactory shapeFactory)
    {
        if (!await _authorizationService.AuthorizeAsync(User, IndexingPermissions.ManageIndexes))
        {
            return Forbid();
        }

        var pager = new Pager(pagerParameters, pagerOptions.Value.GetPageSize());

        var result = await _indexProfileManager.PageAsync(pager.Page, pager.PageSize, new QueryContext
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

        var viewModel = new ListSourcedEntitiesViewModel<IndexProfileKey, ModelEntry<IndexProfile>, IndexingEntityOptions>
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
            viewModel.Models.Add(new ModelEntry<IndexProfile>
            {
                Model = record,
                Shape = await _displayManager.BuildDisplayAsync(record, _updateModelAccessor.ModelUpdater, "SummaryAdmin"),
            });
        }

        viewModel.Options.BulkActions =
        [
            new SelectListItem(S["Delete"], nameof(IndexingEntityAction.Remove)),
            new SelectListItem(S["Reset"], nameof(IndexingEntityAction.Reset)),
            new SelectListItem(S["Rebuild"], nameof(IndexingEntityAction.Rebuild)),
        ];

        return View(viewModel);
    }

    [HttpPost]
    [ActionName(nameof(Index))]
    [FormValueRequired("submit.Filter")]
    [Admin("indexing", "IndexingIndex")]
    public ActionResult IndexFilterPost(ListEntitiesViewModel model)
    {
        return RedirectToAction(nameof(Index), new RouteValueDictionary
        {
            { _optionsSearch, model.Options?.Search },
        });
    }

    [Admin("indexing/create/{providerName}/{type}", "IndexingCreate")]
    public async Task<ActionResult> Create(string providerName, string type)
    {
        if (!await _authorizationService.AuthorizeAsync(User, IndexingPermissions.ManageIndexes))
        {
            return Forbid();
        }

        if (!_indexingOptions.Sources.TryGetValue(new IndexProfileKey(providerName, type), out var service))
        {
            await _notifier.ErrorAsync(H["Unable to find a provider named '{0}' with the type '{1}'.", providerName, type]);

            return RedirectToAction(nameof(Index));
        }

        var index = await _indexProfileManager.NewAsync(providerName, type);

        if (index == null)
        {
            await _notifier.ErrorAsync(H["Invalid provider or type."]);

            return RedirectToAction(nameof(Index));
        }

        var model = new ModelViewModel
        {
            DisplayName = service.DisplayName,
            Editor = await _displayManager.BuildEditorAsync(index, _updateModelAccessor.ModelUpdater, isNew: true),
        };

        return View(model);
    }

    [HttpPost]
    [ActionName(nameof(Create))]
    [Admin("indexing/create/{providerName}/{type}", "IndexingCreate")]
    public async Task<ActionResult> CreatePost(string providerName, string type)
    {
        if (!await _authorizationService.AuthorizeAsync(User, IndexingPermissions.ManageIndexes))
        {
            return Forbid();
        }

        if (!_indexingOptions.Sources.TryGetValue(new IndexProfileKey(providerName, type), out var service))
        {
            await _notifier.ErrorAsync(H["Unable to find a provider with the name '{0}'.", providerName]);

            return RedirectToAction(nameof(Index));
        }

        var index = await _indexProfileManager.NewAsync(providerName, type);

        if (index == null)
        {
            await _notifier.ErrorAsync(H["Invalid provider or type."]);

            return RedirectToAction(nameof(Index));
        }

        var model = new ModelViewModel
        {
            DisplayName = service.DisplayName,
            Editor = await _displayManager.UpdateEditorAsync(index, _updateModelAccessor.ModelUpdater, isNew: true),
        };

        var validate = await _indexProfileManager.ValidateAsync(index);

        if (!validate.Succeeded && ModelState.IsValid)
        {
            foreach (var error in validate.Errors)
            {
                foreach (var memberName in error.MemberNames)
                {
                    ModelState.TryAddModelError(memberName, error.ErrorMessage);
                }
            }
        }

        if (ModelState.IsValid)
        {
            var indexManager = _serviceProvider.GetKeyedService<IIndexManager>(index.ProviderName);

            if (indexManager is null)
            {
                await _notifier.ErrorAsync(H["No index manager found to rebuild index for provider '{0}'.", index.ProviderName]);

                return RedirectToAction(nameof(Index));
            }

            // Before creating the index in the provider, we need to create it locally to ensure all the properties are set.
            await _indexProfileManager.CreateAsync(index);

            if (!await indexManager.CreateAsync(index))
            {
                // Delete the index locally if we failed to create it in the provider.
                await _indexProfileManager.DeleteAsync(index);

                await _notifier.ErrorAsync(H["Unable to create the index for the provider '{0}'.", index.ProviderName]);

                return View(model);
            }

            await _indexProfileManager.SynchronizeAsync(index);

            await _notifier.SuccessAsync(H["An index has been created successfully. The synchronizing process was triggered in the background."]);

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

        var index = await _indexProfileManager.FindByIdAsync(id);

        if (index == null)
        {
            return NotFound();
        }

        var model = new ModelViewModel
        {
            DisplayName = index.Name,
            Editor = await _displayManager.BuildEditorAsync(index, _updateModelAccessor.ModelUpdater, isNew: false),
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

        var index = await _indexProfileManager.FindByIdAsync(id);

        if (index == null)
        {
            return NotFound();
        }

        // Clone the index to prevent modifying the original instance in the store.
        var mutableProfile = index.Clone();

        var model = new ModelViewModel
        {
            DisplayName = mutableProfile.Name,
            Editor = await _displayManager.UpdateEditorAsync(mutableProfile, _updateModelAccessor.ModelUpdater, isNew: false),
        };

        var validate = await _indexProfileManager.ValidateAsync(index);

        if (!validate.Succeeded && ModelState.IsValid)
        {
            foreach (var error in validate.Errors)
            {
                foreach (var memberName in error.MemberNames)
                {
                    ModelState.TryAddModelError(memberName, error.ErrorMessage);
                }
            }
        }

        if (ModelState.IsValid)
        {
            await _indexProfileManager.UpdateAsync(mutableProfile);

            await _notifier.SuccessAsync(H["An index has been updated successfully."]);

            return RedirectToAction(nameof(Index));
        }

        return View(model);
    }

    [HttpPost]
    [Admin("indexing/delete/{id}", "IndexingDelete")]
    public async Task<IActionResult> Delete(string id, bool force = false)
    {
        if (!await _authorizationService.AuthorizeAsync(User, IndexingPermissions.ManageIndexes))
        {
            return Forbid();
        }

        var index = await _indexProfileManager.FindByIdAsync(id);

        if (index == null)
        {
            return NotFound();
        }

        var indexManager = _serviceProvider.GetKeyedService<IIndexManager>(index.ProviderName);

        if (force)
        {
            await indexManager?.DeleteAsync(index);
            await _indexProfileManager.DeleteAsync(index);

            await _notifier.SuccessAsync(H["The index was removed successfully."]);

            return RedirectToAction(nameof(Index));
        }

        if (indexManager is null)
        {
            await _notifier.ErrorAsync(H["No index manager found to rebuild index for provider '{0}'.", index.ProviderName]);

            return RedirectToAction(nameof(Index));
        }

        var exists = await indexManager.ExistsAsync(index.IndexFullName);

        if (exists && !await indexManager.DeleteAsync(index))
        {
            await _notifier.ErrorAsync(H["Unable to delete the index for the provider {0}.", index.ProviderName]);

            return RedirectToAction(nameof(Index));
        }

        if (await _indexProfileManager.DeleteAsync(index))
        {
            await _notifier.SuccessAsync(H["The index was removed successfully."]);
        }
        else
        {
            await _notifier.ErrorAsync(H["Unable to delete the index locally. Try force-deleting the index."]);
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [Admin("indexing/reset/{id}", "IndexingReset")]
    public async Task<IActionResult> Reset(string id)
    {
        if (!await _authorizationService.AuthorizeAsync(User, IndexingPermissions.ManageIndexes))
        {
            return Forbid();
        }

        var index = await _indexProfileManager.FindByIdAsync(id);

        if (index == null)
        {
            return NotFound();
        }

        await _indexProfileManager.ResetAsync(index);
        await _indexProfileManager.UpdateAsync(index);
        await _indexProfileManager.SynchronizeAsync(index);

        await _notifier.SuccessAsync(H["An index has been reset successfully. The synchronizing process was triggered in the background."]);

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [Admin("indexing/rebuild/{id}", "IndexingRebuild")]
    public async Task<IActionResult> Rebuild(string id)
    {
        if (!await _authorizationService.AuthorizeAsync(User, IndexingPermissions.ManageIndexes))
        {
            return Forbid();
        }

        var index = await _indexProfileManager.FindByIdAsync(id);

        if (index == null)
        {
            return NotFound();
        }

        var indexManager = _serviceProvider.GetKeyedService<IIndexManager>(index.ProviderName);

        if (indexManager is null)
        {
            await _notifier.ErrorAsync(H["No index manager found to rebuild index for provider '{0}'.", index.ProviderName]);

            return RedirectToAction(nameof(Index));
        }

        if (await indexManager.RebuildAsync(index))
        {
            await _indexProfileManager.ResetAsync(index);
            await _indexProfileManager.UpdateAsync(index);
            await _indexProfileManager.SynchronizeAsync(index);

            await _notifier.SuccessAsync(H["An index has been rebuilt successfully. The synchronizing process was triggered in the background."]);
        }
        else
        {
            await _notifier.ErrorAsync(H["An error occurred while rebuilding the index."]);
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ActionName(nameof(Index))]
    [FormValueRequired("submit.BulkAction")]
    [Admin("indexing", "IndexingIndex")]
    public async Task<ActionResult> IndexPost(IndexingEntityOptions options, IEnumerable<string> itemIds)
    {
        if (!await _authorizationService.AuthorizeAsync(User, IndexingPermissions.ManageIndexes))
        {
            return Forbid();
        }

        if (itemIds?.Count() > 0)
        {
            var indexManagers = new Dictionary<string, IIndexManager>();

            switch (options.BulkAction)
            {
                case IndexingEntityAction.None:
                    break;
                case IndexingEntityAction.Remove:
                    var removeCounter = 0;
                    foreach (var id in itemIds)
                    {
                        var index = await _indexProfileManager.FindByIdAsync(id);

                        if (index == null)
                        {
                            continue;
                        }

                        if (!indexManagers.TryGetValue(index.ProviderName, out var indexManager))
                        {
                            indexManager = _serviceProvider.GetKeyedService<IIndexManager>(index.ProviderName);
                            indexManagers.Add(index.ProviderName, indexManager);
                        }

                        if (indexManager is null)
                        {
                            continue;
                        }

                        var exists = await indexManager.ExistsAsync(index.IndexFullName);

                        if (exists && !await indexManager.DeleteAsync(index))
                        {
                            continue;
                        }

                        if (await _indexProfileManager.DeleteAsync(index))
                        {
                            removeCounter++;
                        }
                    }

                    if (removeCounter == 0)
                    {
                        await _notifier.WarningAsync(H["No index were removed."]);
                    }
                    else
                    {
                        await _notifier.SuccessAsync(H.Plural(removeCounter, "1 index has been removed successfully.", "{0} indexes have been removed successfully."));
                    }
                    break;

                case IndexingEntityAction.Reset:
                    var resetCounter = 0;

                    foreach (var id in itemIds)
                    {
                        var index = await _indexProfileManager.FindByIdAsync(id);

                        if (index == null)
                        {
                            continue;
                        }

                        await _indexProfileManager.ResetAsync(index);
                        await _indexProfileManager.UpdateAsync(index);
                        await _indexProfileManager.SynchronizeAsync(index);

                        resetCounter++;
                    }

                    if (resetCounter == 0)
                    {
                        await _notifier.WarningAsync(H["No index were reset."]);
                    }
                    else
                    {
                        await _notifier.SuccessAsync(H.Plural(resetCounter, "1 index has been reset successfully.", "{0} indexes have been reset successfully."));
                    }
                    break;

                case IndexingEntityAction.Rebuild:
                    var rebuildCounter = 0;
                    foreach (var id in itemIds)
                    {
                        var index = await _indexProfileManager.FindByIdAsync(id);

                        if (index == null)
                        {
                            continue;
                        }

                        if (!indexManagers.TryGetValue(index.ProviderName, out var indexManager))
                        {
                            indexManager = _serviceProvider.GetKeyedService<IIndexManager>(index.ProviderName);
                            indexManagers.Add(index.ProviderName, indexManager);
                        }

                        if (indexManager is null)
                        {
                            continue;
                        }

                        if (!await indexManager.RebuildAsync(index))
                        {
                            continue;
                        }

                        rebuildCounter++;

                        await _indexProfileManager.ResetAsync(index);
                        await _indexProfileManager.UpdateAsync(index);
                        await _indexProfileManager.SynchronizeAsync(index);
                    }

                    if (rebuildCounter == 0)
                    {
                        await _notifier.WarningAsync(H["No index were rebuilt."]);
                    }
                    else
                    {
                        await _notifier.SuccessAsync(H.Plural(rebuildCounter, "1 index has been rebuilt successfully.", "{0} indexes have been rebuilt successfully."));
                    }
                    break;
                default:
                    return BadRequest();
            }
        }

        return RedirectToAction(nameof(Index));
    }
}
