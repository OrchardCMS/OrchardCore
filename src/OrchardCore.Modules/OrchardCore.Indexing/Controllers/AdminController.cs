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
using OrchardCore.Indexing.Core.Operations;
using OrchardCore.Indexing.Models;
using OrchardCore.Indexing.ViewModels;
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
    private readonly IIndexProfileManagementService _management;
    private readonly IndexOperationRunner _operations;
    private readonly IndexingOptions _indexingOptions;
    private readonly IDisplayManager<IndexProfile> _displayManager;
    private readonly INotifier _notifier;

    internal readonly IHtmlLocalizer H;
    internal readonly IStringLocalizer S;

    public AdminController(
        IAuthorizationService authorizationService,
        IUpdateModelAccessor updateModelAccessor,
        IIndexProfileManager indexProfileManager,
        IIndexProfileManagementService management,
        IndexOperationRunner operations,
        IDisplayManager<IndexProfile> displayManager,
        IOptions<IndexingOptions> indexingOptions,
        INotifier notifier,
        IHtmlLocalizer<AdminController> htmlLocalizer,
        IStringLocalizer<AdminController> stringLocalizer)
    {
        _authorizationService = authorizationService;
        _updateModelAccessor = updateModelAccessor;
        _indexProfileManager = indexProfileManager;
        _management = management;
        _operations = operations;
        _displayManager = displayManager;
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

        var pager = new Pager(pagerParameters, pagerOptions.Value);

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

        var sourceGroups = _indexingOptions.Sources
            .Select(x => new
            {
                Source = x.Key,
                ProviderName = x.Value.ProviderName,
            })
            .OrderBy(x => x.ProviderName, StringComparer.OrdinalIgnoreCase)
            .ThenBy(x => x.Source.Type)
            .GroupBy(x => x.ProviderName, StringComparer.OrdinalIgnoreCase)
            .Select(group => new IndexSourceGroupViewModel
            {
                ProviderName = group.Key,
                ProviderDisplayName = _indexingOptions.Providers.TryGetValue(group.Key, out var provider)
                    ? provider.DisplayName?.Value ?? group.Key
                    : group.Key,
                Sources = group.Select(x => x.Source).ToArray(),
            })
            .ToArray();

        var viewModel = new AdminIndexViewModel
        {
            Models = [],
            Options = options,
            Pager = await shapeFactory.PagerAsync(pager, result.Count, routeData),
            Sources = sourceGroups.SelectMany(x => x.Sources).ToArray(),
            SourceGroups = sourceGroups,
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
            new SelectListItem(S["Synchronize"], nameof(IndexingEntityAction.Synchronize)),
            new SelectListItem(S["Reset"], nameof(IndexingEntityAction.Reset)),
            new SelectListItem(S["Rebuild"], nameof(IndexingEntityAction.Rebuild)),
            new SelectListItem(S["Delete"], nameof(IndexingEntityAction.Remove)),
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

        var indexProfile = await _indexProfileManager.NewAsync(providerName, type);

        if (indexProfile == null)
        {
            await _notifier.ErrorAsync(H["Invalid provider or type."]);

            return RedirectToAction(nameof(Index));
        }

        var model = new ModelViewModel
        {
            DisplayName = service.DisplayName,
            Editor = await _displayManager.BuildEditorAsync(indexProfile, _updateModelAccessor.ModelUpdater, isNew: true),
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

        var indexProfile = await _indexProfileManager.NewAsync(providerName, type);

        if (indexProfile == null)
        {
            await _notifier.ErrorAsync(H["Invalid provider or type."]);

            return RedirectToAction(nameof(Index));
        }

        var model = new ModelViewModel
        {
            DisplayName = service.DisplayName,
            Editor = await _displayManager.UpdateEditorAsync(indexProfile, _updateModelAccessor.ModelUpdater, isNew: true),
        };

        var validate = await _indexProfileManager.ValidateAsync(indexProfile);

        if (!validate.Succeeded && ModelState.IsValid)
        {
            foreach (var error in validate.Errors)
            {
                if (error.MemberNames.Any())
                {
                    foreach (var memberName in error.MemberNames)
                    {
                        ModelState.TryAddModelError(memberName, error.ErrorMessage);
                    }
                }
                else
                {
                    ModelState.TryAddModelError(string.Empty, error.ErrorMessage);
                }
            }
        }

        if (ModelState.IsValid)
        {
            var result = await _management.CreateAsync(indexProfile);
            if (result == IndexProfileManagementResult.ProviderUnavailable)
            {
                await _notifier.ErrorAsync(H["No index manager found to create index for provider '{0}'.", indexProfile.ProviderName]);

                return RedirectToAction(nameof(Index));
            }

            if (result != IndexProfileManagementResult.Success)
            {
                await _notifier.ErrorAsync(H["Unable to create the index for the provider '{0}'.", indexProfile.ProviderName]);

                return View(model);
            }

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

        var indexProfile = await _indexProfileManager.FindByIdAsync(id);

        if (indexProfile == null)
        {
            return NotFound();
        }

        var model = new ModelViewModel
        {
            DisplayName = indexProfile.Name,
            Editor = await _displayManager.BuildEditorAsync(indexProfile, _updateModelAccessor.ModelUpdater, isNew: false),
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

        var indexProfile = await _indexProfileManager.FindByIdAsync(id);

        if (indexProfile == null)
        {
            return NotFound();
        }

        var model = new ModelViewModel
        {
            DisplayName = indexProfile.Name,
            Editor = await _displayManager.UpdateEditorAsync(indexProfile, _updateModelAccessor.ModelUpdater, isNew: false),
        };

        var validate = await _indexProfileManager.ValidateAsync(indexProfile);

        if (!validate.Succeeded && ModelState.IsValid)
        {
            foreach (var error in validate.Errors)
            {
                if (error.MemberNames.Any())
                {
                    foreach (var memberName in error.MemberNames)
                    {
                        ModelState.TryAddModelError(memberName, error.ErrorMessage);
                    }
                }
                else
                {
                    ModelState.TryAddModelError(string.Empty, error.ErrorMessage);
                }
            }
        }

        if (ModelState.IsValid)
        {
            try
            {
                await _indexProfileManager.UpdateAsync(indexProfile);
            }
            catch (IndexProfileValidationException exception)
            {
                foreach (var error in exception.Errors)
                {
                    foreach (var member in error.MemberNames.DefaultIfEmpty(string.Empty))
                    {
                        ModelState.TryAddModelError(member, error.ErrorMessage);
                    }
                }
                return View(model);
            }

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

        var indexProfile = await _indexProfileManager.FindByIdAsync(id);

        if (indexProfile == null)
        {
            return NotFound();
        }

        var result = await _management.DeleteAsync(indexProfile, force);
        switch (result)
        {
            case IndexProfileManagementResult.Success:
                await _notifier.SuccessAsync(H["The index was removed successfully."]);
                break;
            case IndexProfileManagementResult.ProviderUnavailable:
                await _notifier.ErrorAsync(H["No index manager found to delete index for provider '{0}'.", indexProfile.ProviderName]);
                break;
            case IndexProfileManagementResult.ProviderRejected:
                await _notifier.ErrorAsync(H["Unable to delete the index for the provider {0}.", indexProfile.ProviderName]);
                break;
            case IndexProfileManagementResult.LocalDeleteFailed:
                await _notifier.ErrorAsync(H["Unable to delete the index locally. Try force-deleting the index."]);
                break;
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [Admin("indexing/reset/{id}", "IndexingReset")]
    public Task<IActionResult> Reset(string id) => QueueLifecycleAsync(id, IndexLifecycleAction.Reset);

    [HttpPost]
    [Admin("indexing/synchronize/{id}", "IndexingSynchronize")]
    public Task<IActionResult> Synchronize(string id) => QueueLifecycleAsync(id, IndexLifecycleAction.Synchronize);

    [HttpPost]
    [Admin("indexing/rebuild/{id}", "IndexingRebuild")]
    public Task<IActionResult> Rebuild(string id) => QueueLifecycleAsync(id, IndexLifecycleAction.Rebuild);

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

        if (itemIds?.Any() == true)
        {
            switch (options.BulkAction)
            {
                case IndexingEntityAction.None:
                    break;
                case IndexingEntityAction.Remove:
                    var removeCounter = 0;
                    foreach (var id in itemIds)
                    {
                        var indexProfile = await _indexProfileManager.FindByIdAsync(id);

                        if (indexProfile == null)
                        {
                            continue;
                        }

                        if (await _management.DeleteAsync(indexProfile) == IndexProfileManagementResult.Success)
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
                case IndexingEntityAction.Synchronize:
                case IndexingEntityAction.Rebuild:
                    var action = options.BulkAction switch
                    {
                        IndexingEntityAction.Reset => IndexLifecycleAction.Reset,
                        IndexingEntityAction.Rebuild => IndexLifecycleAction.Rebuild,
                        _ => IndexLifecycleAction.Synchronize,
                    };
                    var queued = 0;
                    foreach (var id in itemIds.Distinct(StringComparer.Ordinal))
                    {
                        if (await _indexProfileManager.FindByIdAsync(id) is null) { continue; }
                        await _operations.QueueAsync(id, action);
                        queued++;
                    }
                    await _notifier.SuccessAsync(H.Plural(queued, "1 index operation was queued.", "{0} index operations were queued."));
                    break;
                default:
                    return BadRequest();
            }
        }

        return RedirectToAction(nameof(Index));
    }
    private async Task<IActionResult> QueueLifecycleAsync(string id, IndexLifecycleAction action)
    {
        if (!await _authorizationService.AuthorizeAsync(User, IndexingPermissions.ManageIndexes)) { return Forbid(); }
        if (await _indexProfileManager.FindByIdAsync(id) is null) { return NotFound(); }
        await _operations.QueueAsync(id, action);
        await _notifier.SuccessAsync(H["The index operation was queued."]);
        return RedirectToAction(nameof(Index));
    }
}
