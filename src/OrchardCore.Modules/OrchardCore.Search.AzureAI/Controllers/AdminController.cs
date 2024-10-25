using System.Globalization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OrchardCore.Admin;
using OrchardCore.BackgroundJobs;
using OrchardCore.ContentManagement;
using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.Notify;
using OrchardCore.Indexing;
using OrchardCore.Navigation;
using OrchardCore.Routing;
using OrchardCore.Search.AzureAI.Models;
using OrchardCore.Search.AzureAI.Services;
using OrchardCore.Search.AzureAI.ViewModels;
using OrchardCore.Settings;

namespace OrchardCore.Search.AzureAI.Controllers;

[Admin("azure-search/{action}/{indexName?}", "AzureAISearch.{action}")]
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
        S = stringLocalizer;
        H = htmlLocalizer;
    }

    public async Task<IActionResult> Index(AzureAIIndexOptions options, PagerParameters pagerParameters)
    {
        if (!await _authorizationService.AuthorizeAsync(User, AzureAISearchIndexPermissionHelper.ManageAzureAISearchIndexes))
        {
            return Forbid();
        }

        if (!_azureAIOptions.ConfigurationExists())
        {
            return NotConfigured();
        }

        var indexes = (await _indexSettingsService.GetSettingsAsync())
            .Select(i => new IndexViewModel { Name = i.IndexName })
            .ToList();

        var totalIndexes = indexes.Count;

        if (!string.IsNullOrWhiteSpace(options.Search))
        {
            indexes = indexes.Where(q => q.Name.Contains(options.Search, StringComparison.OrdinalIgnoreCase)).ToList();
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
            Indexes = indexes,
            Options = options,
            Pager = await _shapeFactory.PagerAsync(pager, totalIndexes, routeData)
        };

        model.Options.ContentsBulkAction =
        [
            new SelectListItem(S["Delete"], nameof(AzureAISearchIndexBulkAction.Remove)),
        ];

        return View(model);
    }

    [HttpPost, ActionName(nameof(Index))]
    [FormValueRequired("submit.Filter")]
    public ActionResult IndexFilterPOST(AdminIndexViewModel model)
        => RedirectToAction(nameof(Index),
            new RouteValueDictionary
            {
                { _optionsSearch, model.Options.Search }
            });


    [HttpPost, ActionName(nameof(Index))]
    [FormValueRequired("submit.BulkAction")]
    public async Task<IActionResult> IndexPost(AzureAIIndexOptions options, IEnumerable<string> itemIds)
    {
        if (!await _authorizationService.AuthorizeAsync(User, AzureAISearchIndexPermissionHelper.ManageAzureAISearchIndexes))
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

    public async Task<IActionResult> Create()
    {
        if (!await _authorizationService.AuthorizeAsync(User, AzureAISearchIndexPermissionHelper.ManageAzureAISearchIndexes))
        {
            return Forbid();
        }

        if (!_azureAIOptions.ConfigurationExists())
        {
            return NotConfigured();
        }

        var model = new AzureAISettingsViewModel
        {
            AnalyzerName = AzureAISearchDefaultOptions.DefaultAnalyzer
        };

        PopulateMenuOptions(model);

        return View(model);
    }

    [HttpPost, ActionName(nameof(Create))]
    public async Task<IActionResult> CreatePost(AzureAISettingsViewModel model)
    {
        if (!await _authorizationService.AuthorizeAsync(User, AzureAISearchIndexPermissionHelper.ManageAzureAISearchIndexes))
        {
            return Forbid();
        }

        if (!_azureAIOptions.ConfigurationExists())
        {
            return BadRequest();
        }

        if (ModelState.IsValid && await _indexManager.ExistsAsync(model.IndexName))
        {
            ModelState.AddModelError(nameof(AzureAISettingsViewModel.IndexName), S["An index named <em>{0}</em> already exist in Azure AI Search server.", model.IndexName]);
        }

        if (ModelState.IsValid)
        {
            try
            {
                var settings = new AzureAISearchIndexSettings
                {
                    IndexName = model.IndexName,
                    IndexFullName = _indexManager.GetFullIndexName(model.IndexName),
                    AnalyzerName = model.AnalyzerName,
                    QueryAnalyzerName = model.AnalyzerName,
                    IndexLatest = model.IndexLatest,
                    IndexedContentTypes = model.IndexedContentTypes,
                    Culture = model.Culture ?? string.Empty,
                };

                if (string.IsNullOrEmpty(settings.AnalyzerName))
                {
                    settings.AnalyzerName = AzureAISearchDefaultOptions.DefaultAnalyzer;
                }

                if (string.IsNullOrEmpty(settings.QueryAnalyzerName))
                {
                    settings.QueryAnalyzerName = settings.AnalyzerName;
                }

                settings.IndexMappings = await _azureAIIndexDocumentManager.GetMappingsAsync(settings);

                if (await _indexManager.CreateAsync(settings))
                {
                    await _indexSettingsService.UpdateAsync(settings);
                    await AsyncContentItemsAsync(settings.IndexName);
                    await _notifier.SuccessAsync(H["Index <em>{0}</em> created successfully.", model.IndexName]);

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
                _logger.LogError(e, "An error occurred while creating an index {IndexName}.", _indexManager.GetFullIndexName(model.IndexName));
            }
        }

        PopulateMenuOptions(model);

        return View(model);
    }

    public async Task<IActionResult> Edit(string indexName)
    {
        if (!await _authorizationService.AuthorizeAsync(User, AzureAISearchIndexPermissionHelper.ManageAzureAISearchIndexes))
        {
            return Forbid();
        }

        if (!_azureAIOptions.ConfigurationExists())
        {
            return NotConfigured();
        }

        var settings = await _indexSettingsService.GetAsync(indexName);

        if (settings == null)
        {
            return NotFound();
        }

        var model = new AzureAISettingsViewModel
        {
            IndexName = settings.IndexName,
            AnalyzerName = settings.AnalyzerName,
            IndexLatest = settings.IndexLatest,
            Culture = settings.Culture,
            IndexedContentTypes = settings.IndexedContentTypes,
        };

        if (string.IsNullOrEmpty(model.AnalyzerName))
        {
            model.AnalyzerName = AzureAISearchDefaultOptions.DefaultAnalyzer;
        }

        if (string.IsNullOrEmpty(settings.QueryAnalyzerName))
        {
            settings.QueryAnalyzerName = model.AnalyzerName;
        }

        PopulateMenuOptions(model);

        return View(model);
    }

    [HttpPost, ActionName(nameof(Edit))]
    public async Task<IActionResult> EditPost(AzureAISettingsViewModel model)
    {
        if (!await _authorizationService.AuthorizeAsync(User, AzureAISearchIndexPermissionHelper.ManageAzureAISearchIndexes))
        {
            return Forbid();
        }

        if (!_azureAIOptions.ConfigurationExists())
        {
            return BadRequest();
        }

        if (ModelState.IsValid && !await _indexManager.ExistsAsync(model.IndexName))
        {
            ModelState.AddModelError(nameof(AzureAISettingsViewModel.IndexName), S["The index named <em>{0}</em> doesn't exist in Azure AI Search server.", model.IndexName]);
        }

        if (ModelState.IsValid)
        {
            var settings = await _indexSettingsService.GetAsync(model.IndexName);

            if (settings == null)
            {
                return NotFound();
            }

            try
            {
                settings.AnalyzerName = model.AnalyzerName;
                settings.QueryAnalyzerName = model.AnalyzerName;
                settings.IndexLatest = model.IndexLatest;
                settings.IndexedContentTypes = model.IndexedContentTypes;
                settings.Culture = model.Culture ?? string.Empty;

                if (string.IsNullOrEmpty(settings.IndexFullName))
                {
                    settings.IndexFullName = _indexManager.GetFullIndexName(settings.IndexName);
                }

                if (string.IsNullOrEmpty(settings.AnalyzerName))
                {
                    settings.AnalyzerName = AzureAISearchDefaultOptions.DefaultAnalyzer;
                }

                if (string.IsNullOrEmpty(settings.QueryAnalyzerName))
                {
                    settings.QueryAnalyzerName = settings.AnalyzerName;
                }

                settings.IndexMappings = await _azureAIIndexDocumentManager.GetMappingsAsync(settings);

                if (!await _indexManager.CreateAsync(settings))
                {
                    await _notifier.ErrorAsync(H["An error occurred while creating the index."]);
                }
                else
                {
                    await _indexSettingsService.UpdateAsync(settings);

                    await _notifier.SuccessAsync(H["Index <em>{0}</em> created successfully.", model.IndexName]);

                    await AsyncContentItemsAsync(settings.IndexName);

                    return RedirectToAction(nameof(Index));
                }
            }
            catch (Exception e)
            {
                await _notifier.ErrorAsync(H["An error occurred while updating the index."]);

                _logger.LogError(e, "An error occurred while updating an index {IndexName}.", _indexManager.GetFullIndexName(model.IndexName));
            }
        }

        PopulateMenuOptions(model);

        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> Delete(string indexName)
    {
        if (!await _authorizationService.AuthorizeAsync(User, AzureAISearchIndexPermissionHelper.ManageAzureAISearchIndexes))
        {
            return Forbid();
        }

        if (!_azureAIOptions.ConfigurationExists())
        {
            return BadRequest();
        }

        var exists = await _indexManager.ExistsAsync(indexName);

        if (!exists)
        {
            // At this point we know that the index does not exists on remote server. Let's delete it locally.
            await _indexSettingsService.DeleteAsync(indexName);

            await _notifier.SuccessAsync(H["Index <em>{0}</em> deleted successfully.", indexName]);
        }
        else if (await _indexManager.DeleteAsync(indexName))
        {
            await _indexSettingsService.DeleteAsync(indexName);

            await _notifier.SuccessAsync(H["Index <em>{0}</em> deleted successfully.", indexName]);
        }
        else
        {
            await _notifier.ErrorAsync(H["An error occurred while deleting the <em>{0}</em> index.", indexName]);
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> Rebuild(string indexName)
    {
        if (!await _authorizationService.AuthorizeAsync(User, AzureAISearchIndexPermissionHelper.ManageAzureAISearchIndexes))
        {
            return Forbid();
        }

        if (!_azureAIOptions.ConfigurationExists())
        {
            return BadRequest();
        }

        var settings = await _indexSettingsService.GetAsync(indexName);

        if (settings == null)
        {
            return NotFound();
        }

        settings.SetLastTaskId(0);
        settings.IndexMappings = await _azureAIIndexDocumentManager.GetMappingsAsync(settings);
        await _indexSettingsService.UpdateAsync(settings);
        await _indexManager.RebuildAsync(settings);
        await AsyncContentItemsAsync(settings.IndexName);
        await _notifier.SuccessAsync(H["Index <em>{0}</em> rebuilt successfully.", indexName]);

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> Reset(string indexName)
    {
        if (!await _authorizationService.AuthorizeAsync(User, AzureAISearchIndexPermissionHelper.ManageAzureAISearchIndexes))
        {
            return Forbid();
        }

        if (!_azureAIOptions.ConfigurationExists())
        {
            return BadRequest();
        }

        var settings = await _indexSettingsService.GetAsync(indexName);

        if (settings == null)
        {
            return NotFound();
        }

        if (!await _indexManager.ExistsAsync(indexName))
        {
            await _notifier.ErrorAsync(H["Unable to reset the <em>{0}</em> index. Try rebuilding it instead.", indexName]);

            return RedirectToAction(nameof(Index));
        }

        settings.SetLastTaskId(0);
        settings.IndexMappings = await _azureAIIndexDocumentManager.GetMappingsAsync(settings);
        await _indexSettingsService.UpdateAsync(settings);
        await AsyncContentItemsAsync(settings.IndexName);
        await _notifier.SuccessAsync(H["Index <em>{0}</em> reset successfully.", indexName]);

        return RedirectToAction(nameof(Index));
    }

    private ViewResult NotConfigured()
        => View("NotConfigured");

    private static Task AsyncContentItemsAsync(string indexName)
        => HttpBackgroundJob.ExecuteAfterEndOfRequestAsync("sync-content-items-azure-ai-" + indexName, async (scope) =>
        {
            var indexingService = scope.ServiceProvider.GetRequiredService<AzureAISearchIndexingService>();
            await indexingService.ProcessContentItemsAsync(indexName);
        });

    private void PopulateMenuOptions(AzureAISettingsViewModel model)
    {
        model.Cultures = CultureInfo.GetCultures(CultureTypes.AllCultures)
            .Select(x => new SelectListItem { Text = $"{x.Name} ({x.DisplayName})", Value = x.Name });

        model.Analyzers = _azureAIOptions.Analyzers
            .Select(x => new SelectListItem { Text = x, Value = x });
    }
}
