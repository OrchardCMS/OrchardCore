using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OrchardCore.Admin;
using OrchardCore.ContentManagement;
using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Notify;
using OrchardCore.Indexing;
using OrchardCore.Navigation;
using OrchardCore.Routing;
using OrchardCore.Search.AzureAI.Models;
using OrchardCore.Search.AzureAI.Services;
using OrchardCore.Search.AzureAI.ViewModels;
using OrchardCore.Settings;

namespace OrchardCore.Search.AzureAI.Controllers;

public sealed class AdminController : Controller
{
    private const string _optionsSearch = "Options.Search";

    private readonly ISiteService _siteService;
    private readonly IAuthorizationService _authorizationService;
    private readonly AzureAISearchIndexManager _indexManager;
    private readonly AzureAISearchIndexSettingsService _indexSettingsService;
    private readonly IContentManager _contentManager;
    private readonly IShapeFactory _shapeFactory;
    private readonly AzureAIIndexDocumentManager _azureAIIndexDocumentManager;
    private readonly AzureAISearchDefaultOptions _azureAIOptions;
    private readonly INotifier _notifier;
    private readonly IEnumerable<IContentItemIndexHandler> _contentItemIndexHandlers;
    private readonly ILogger _logger;
    private readonly IDisplayManager<AzureAISearchIndexSettings> _displayManager;
    private readonly AzureAISearchOptions _azureAISearchOptions;
    private readonly IUpdateModelAccessor _updateModelAccessor;

    internal readonly IStringLocalizer S;
    internal readonly IHtmlLocalizer H;

    public AdminController(
        ISiteService siteService,
        IAuthorizationService authorizationService,
        AzureAISearchIndexManager indexManager,
        AzureAISearchIndexSettingsService indexSettingsService,
        IContentManager contentManager,
        IShapeFactory shapeFactory,
        AzureAIIndexDocumentManager azureAIIndexDocumentManager,
        IOptions<AzureAISearchDefaultOptions> azureAIOptions,
        INotifier notifier,
        IEnumerable<IContentItemIndexHandler> contentItemIndexHandlers,
        IDisplayManager<AzureAISearchIndexSettings> displayManager,
        IOptions<AzureAISearchOptions> azureAISearchOptions,
        IUpdateModelAccessor updateModelAccessor,
        ILogger<AdminController> logger,
        IStringLocalizer<AdminController> stringLocalizer,
        IHtmlLocalizer<AdminController> htmlLocalizer
        )
    {
        _siteService = siteService;
        _authorizationService = authorizationService;
        _indexManager = indexManager;
        _indexSettingsService = indexSettingsService;
        _contentManager = contentManager;
        _shapeFactory = shapeFactory;
        _azureAIIndexDocumentManager = azureAIIndexDocumentManager;
        _azureAIOptions = azureAIOptions.Value;
        _notifier = notifier;
        _contentItemIndexHandlers = contentItemIndexHandlers;
        _logger = logger;
        _displayManager = displayManager;
        _azureAISearchOptions = azureAISearchOptions.Value;
        _updateModelAccessor = updateModelAccessor;
        S = stringLocalizer;
        H = htmlLocalizer;
    }

    public async Task<IActionResult> Index(AzureAIIndexOptions options, PagerParameters pagerParameters)
    {
        if (!await _authorizationService.AuthorizeAsync(User, AzureAISearchPermissions.ManageAzureAISearchIndexes))
        {
            return Forbid();
        }

        if (!_azureAIOptions.ConfigurationExists())
        {
            return NotConfigured();
        }

        var indexes = (await _indexSettingsService.GetSettingsAsync()).ToList();

        var totalIndexes = indexes.Count;

        if (!string.IsNullOrWhiteSpace(options.Search))
        {
            indexes = indexes.Where(q => q.IndexName.Contains(options.Search, StringComparison.OrdinalIgnoreCase)).ToList();
        }

        var siteSettings = await _siteService.GetSiteSettingsAsync();
        var pager = new Pager(pagerParameters, siteSettings.PageSize);

        indexes = indexes
            .Skip(pager.GetStartIndex())
            .Take(pager.PageSize)
            .ToList();

        // Maintain previous route data when generating page links.
        var routeData = new RouteData();

        if (!string.IsNullOrEmpty(options.Search))
        {
            routeData.Values.TryAdd(_optionsSearch, options.Search);
        }

        var model = new AdminIndexViewModel
        {
            Indexes = [],
            Options = options,
            Pager = await _shapeFactory.PagerAsync(pager, totalIndexes, routeData),
            SourceNames = _azureAISearchOptions.IndexSources.Keys.Order(),
        };

        foreach (var index in indexes)
        {
            model.Indexes.Add(new AzureAIIndexEntry
            {
                Index = index,
                Shape = await _displayManager.BuildDisplayAsync(index, _updateModelAccessor.ModelUpdater, "SummaryAdmin")
            });
        }

        model.Options.ContentsBulkAction =
        [
            new SelectListItem(S["Delete"], nameof(AzureAISearchIndexBulkAction.Remove)),
        ];

        return View(model);
    }

    [HttpPost]
    [ActionName(nameof(Index))]
    [FormValueRequired("submit.Filter")]
    public ActionResult IndexFilterPOST(AdminIndexViewModel model)
        => RedirectToAction(nameof(Index),
            new RouteValueDictionary
            {
                { _optionsSearch, model.Options.Search }
            });


    [HttpPost]
    [ActionName(nameof(Index))]
    [FormValueRequired("submit.BulkAction")]
    public async Task<IActionResult> IndexPost(AzureAIIndexOptions options, IEnumerable<string> itemIds)
    {
        if (!await _authorizationService.AuthorizeAsync(User, AzureAISearchPermissions.ManageAzureAISearchIndexes))
        {
            return Forbid();
        }

        if (!_azureAIOptions.ConfigurationExists())
        {
            return BadRequest();
        }

        if (itemIds?.Count() > 0)
        {
            var indexSettings = await _indexSettingsService.GetSettingsAsync();
            var checkedContentItems = indexSettings.Where(x => itemIds.Contains(x.IndexName));

            switch (options.BulkAction)
            {
                case AzureAISearchIndexBulkAction.None:
                    break;
                case AzureAISearchIndexBulkAction.Remove:
                    foreach (var item in checkedContentItems)
                    {
                        await _indexManager.DeleteAsync(item.IndexName);
                    }

                    await _notifier.SuccessAsync(H["Indices successfully removed."]);

                    break;
                default:
                    throw new ArgumentOutOfRangeException(options.BulkAction.ToString(), "Unknown bulk action");
            }
        }

        return RedirectToAction(nameof(Index));
    }

    [Admin("azure-search/create/{source}", "AzureAISearch.Create")]
    public async Task<IActionResult> Create(string source)
    {
        if (!await _authorizationService.AuthorizeAsync(User, AzureAISearchPermissions.ManageAzureAISearchIndexes))
        {
            return Forbid();
        }

        if (!_azureAIOptions.ConfigurationExists())
        {
            return NotConfigured();
        }

        if (!_azureAISearchOptions.IndexSources.TryGetValue(source, out var indexSource))
        {
            await _notifier.ErrorAsync(H["Unable to find a provider with the name '{0}'.", source]);

            return RedirectToAction(nameof(Index));
        }

        var settings = await _indexSettingsService.NewAsync(indexSource.Source);

        var model = await _displayManager.BuildEditorAsync(settings, _updateModelAccessor.ModelUpdater, true);

        return View(model);
    }

    [HttpPost]
    [ActionName(nameof(Create))]
    [Admin("azure-search/create/{source}", "AzureAISearch.Create")]
    public async Task<IActionResult> CreatePost(string source)
    {
        if (!await _authorizationService.AuthorizeAsync(User, AzureAISearchPermissions.ManageAzureAISearchIndexes))
        {
            return Forbid();
        }

        if (!_azureAIOptions.ConfigurationExists())
        {
            return BadRequest();
        }

        if (!_azureAISearchOptions.IndexSources.TryGetValue(source, out var indexSource))
        {
            await _notifier.ErrorAsync(H["Unable to find a provider with the name '{0}'.", source]);

            return RedirectToAction(nameof(Index));
        }

        var settings = await _indexSettingsService.NewAsync(indexSource.Source);

        var model = await _displayManager.UpdateEditorAsync(settings, _updateModelAccessor.ModelUpdater, true);

        if (ModelState.IsValid)
        {
            try
            {
                // TO Investigate: GetMappingsAsync should be moved to _indexSettingsService.

                settings.IndexMappings = await _azureAIIndexDocumentManager.GetMappingsAsync(settings);

                if (await _indexManager.CreateAsync(settings))
                {
                    await _indexSettingsService.UpdateAsync(settings);
                    await _indexSettingsService.SynchronizeAsync(settings);
                    await _notifier.SuccessAsync(H["Index <em>{0}</em> created successfully.", settings.IndexName]);

                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    await _notifier.ErrorAsync(H["An error occurred while creating the index."]);
                }
            }
            catch (Exception e)
            {
                await _notifier.ErrorAsync(H["An error occurred while creating the index."]);
                _logger.LogError(e, "An error occurred while creating an index {IndexName}.", _indexManager.GetFullIndexName(settings.IndexName));
            }
        }

        return View(model);
    }

    [Admin("azure-search/Edit/{id}", "AzureAISearch.Edit")]
    public async Task<IActionResult> Edit(string id)
    {
        if (!await _authorizationService.AuthorizeAsync(User, AzureAISearchPermissions.ManageAzureAISearchIndexes))
        {
            return Forbid();
        }

        if (!_azureAIOptions.ConfigurationExists())
        {
            return NotConfigured();
        }

        var settings = await _indexSettingsService.GetAsync(id);

        if (settings == null)
        {
            return NotFound();
        }

        var model = await _displayManager.BuildEditorAsync(settings, _updateModelAccessor.ModelUpdater, false);

        return View(model);
    }

    [HttpPost]
    [ActionName(nameof(Edit))]
    [Admin("azure-search/Edit/{id}", "AzureAISearch.Edit")]
    public async Task<IActionResult> EditPost(string id)
    {
        if (!await _authorizationService.AuthorizeAsync(User, AzureAISearchPermissions.ManageAzureAISearchIndexes))
        {
            return Forbid();
        }

        if (!_azureAIOptions.ConfigurationExists())
        {
            return BadRequest();
        }

        var settings = await _indexSettingsService.GetAsync(id);

        if (settings == null)
        {
            return NotFound();
        }

        var model = await _displayManager.UpdateEditorAsync(settings, _updateModelAccessor.ModelUpdater, false);

        if (ModelState.IsValid)
        {
            try
            {
                settings.IndexMappings = await _azureAIIndexDocumentManager.GetMappingsAsync(settings);

                if (!await _indexManager.CreateAsync(settings))
                {
                    await _notifier.ErrorAsync(H["An error occurred while updating the index."]);
                }
                else
                {
                    await _indexSettingsService.UpdateAsync(settings);
                    await _indexSettingsService.SynchronizeAsync(settings);

                    await _notifier.SuccessAsync(H["Index <em>{0}</em> updated successfully.", settings.IndexName]);

                    return RedirectToAction(nameof(Index));
                }
            }
            catch (Exception e)
            {
                await _notifier.ErrorAsync(H["An error occurred while updating the index."]);

                _logger.LogError(e, "An error occurred while updating an index {IndexName}.", _indexManager.GetFullIndexName(settings.IndexName));
            }
        }

        return View(model);
    }

    [HttpPost]
    [Admin("azure-search/Delete/{id}", "AzureAISearch.Delete")]
    public async Task<IActionResult> Delete(string id)
    {
        if (!await _authorizationService.AuthorizeAsync(User, AzureAISearchPermissions.ManageAzureAISearchIndexes))
        {
            return Forbid();
        }

        if (!_azureAIOptions.ConfigurationExists())
        {
            return BadRequest();
        }

        var settings = await _indexSettingsService.GetAsync(id);

        if (settings == null)
        {
            return NotFound();
        }

        var exists = await _indexManager.ExistsAsync(settings.IndexName);

        if (!exists)
        {
            // At this point we know that the index does not exists on remote server. Let's delete it locally.
            await _indexSettingsService.DeleteAsync(id);

            await _notifier.SuccessAsync(H["Index <em>{0}</em> deleted successfully.", id]);
        }
        else if (await _indexManager.DeleteAsync(settings.IndexName))
        {
            await _indexSettingsService.DeleteAsync(id);

            await _notifier.SuccessAsync(H["Index <em>{0}</em> deleted successfully.", id]);
        }
        else
        {
            await _notifier.ErrorAsync(H["An error occurred while deleting the <em>{0}</em> index.", id]);
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [Admin("azure-search/Rebuild/{id}", "AzureAISearch.Rebuild")]
    public async Task<IActionResult> Rebuild(string id)
    {
        if (!await _authorizationService.AuthorizeAsync(User, AzureAISearchPermissions.ManageAzureAISearchIndexes))
        {
            return Forbid();
        }

        if (!_azureAIOptions.ConfigurationExists())
        {
            return BadRequest();
        }

        var settings = await _indexSettingsService.GetAsync(id);

        if (settings == null)
        {
            return NotFound();
        }

        settings.IndexMappings = await _azureAIIndexDocumentManager.GetMappingsAsync(settings);
        await _indexSettingsService.ResetAsync(settings);
        await _indexSettingsService.UpdateAsync(settings);
        await _indexManager.RebuildAsync(settings);
        await _indexSettingsService.SynchronizeAsync(settings);
        await _notifier.SuccessAsync(H["Index <em>{0}</em> rebuilt successfully.", settings.IndexName]);

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [Admin("azure-search/Reset/{id}", "AzureAISearch.Reset")]
    public async Task<IActionResult> Reset(string id)
    {
        if (!await _authorizationService.AuthorizeAsync(User, AzureAISearchPermissions.ManageAzureAISearchIndexes))
        {
            return Forbid();
        }

        if (!_azureAIOptions.ConfigurationExists())
        {
            return BadRequest();
        }

        var settings = await _indexSettingsService.GetAsync(id);

        if (settings == null)
        {
            return NotFound();
        }

        if (!await _indexManager.ExistsAsync(settings.IndexName))
        {
            await _notifier.ErrorAsync(H["Unable to reset the <em>{0}</em> index. Try rebuilding it instead.", id]);

            return RedirectToAction(nameof(Index));
        }

        // settings.SetLastTaskId(0);
        settings.IndexMappings = await _azureAIIndexDocumentManager.GetMappingsAsync(settings);
        await _indexSettingsService.UpdateAsync(settings);
        await _indexSettingsService.SynchronizeAsync(settings);
        await _notifier.SuccessAsync(H["Index <em>{0}</em> reset successfully.", id]);

        return RedirectToAction(nameof(Index));
    }

    private ViewResult NotConfigured()
        => View("NotConfigured");
}
