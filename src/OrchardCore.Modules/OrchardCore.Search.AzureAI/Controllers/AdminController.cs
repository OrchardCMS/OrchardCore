using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OrchardCore.ContentManagement;
using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.Notify;
using OrchardCore.Indexing;
using OrchardCore.Modules;
using OrchardCore.Navigation;
using OrchardCore.Routing;
using OrchardCore.Search.AzureAI.Models;
using OrchardCore.Search.AzureAI.Services;
using OrchardCore.Search.AzureAI.ViewModels;
using OrchardCore.Settings;

namespace OrchardCore.Search.AzureAI.Controllers;

public class AdminController : Controller
{
    private readonly ISiteService _siteService;
    private readonly IAuthorizationService _authorizationService;
    private readonly AzureAIIndexManager _indexManager;
    private readonly AzureAIIndexSettingsService _indexSettingsService;
    private readonly IContentManager _contentManager;
    private readonly IShapeFactory _shapeFactory;
    private readonly AzureAIOptions _azureAIOptions;
    private readonly INotifier _notifier;
    private readonly IEnumerable<IContentItemIndexHandler> _contentItemIndexHandlers;
    private readonly ILogger _logger;

    protected readonly IStringLocalizer S;
    protected readonly IHtmlLocalizer H;

    public AdminController(
        ISiteService siteService,
        IAuthorizationService authorizationService,
        AzureAIIndexManager indeManager,
        AzureAIIndexSettingsService indexSettingsService,
        IContentManager contentManager,
        IShapeFactory shapeFactory,
        IOptions<AzureAIOptions> azureAIOptions,
        INotifier notifier,
        IEnumerable<IContentItemIndexHandler> contentItemIndexHandlers,
        ILogger<AdminController> logger,
        IStringLocalizer<AdminController> stringLocalizer,
        IHtmlLocalizer<AdminController> htmlLocalizer
        )
    {
        _siteService = siteService;
        _authorizationService = authorizationService;
        _indexManager = indeManager;
        _indexSettingsService = indexSettingsService;
        _contentManager = contentManager;
        _shapeFactory = shapeFactory;
        _azureAIOptions = azureAIOptions.Value;
        _notifier = notifier;
        _contentItemIndexHandlers = contentItemIndexHandlers;
        _logger = logger;

        S = stringLocalizer;
        H = htmlLocalizer;
    }

    public async Task<IActionResult> Index(AzureAIIndexOptions options, PagerParameters pagerParameters)
    {
        if (!await _authorizationService.AuthorizeAsync(User, AzureAIIndexPermissionHelper.ManageAzureAIIndexes))
        {
            return Forbid();
        }

        var indexes = (await _indexSettingsService.GetSettingsAsync()).Select(i => new IndexViewModel { Name = i.IndexName }).ToList();

        var totalIndexes = indexes.Count;

        if (!string.IsNullOrWhiteSpace(options.Search))
        {
            indexes = indexes.Where(q => q.Name.Contains(options.Search, StringComparison.OrdinalIgnoreCase)).ToList();
        }

        var siteSettings = await _siteService.GetSiteSettingsAsync();
        var pager = new Pager(pagerParameters, siteSettings.PageSize);

        indexes = indexes
            .Skip(pager.GetStartIndex())
            .Take(pager.PageSize).ToList();

        // Maintain previous route data when generating page links
        var routeData = new RouteData();
        var pagerShape = await _shapeFactory.CreateAsync("Pager", Arguments.From(new
        {
            pager.Page,
            pager.PageSize,
            TotalItemCount = totalIndexes
        }));

        var model = new AdminIndexViewModel
        {
            Indexes = indexes,
            Options = options,
            Pager = pagerShape
        };

        model.Options.ContentsBulkAction = [
            new SelectListItem(S["Delete"], nameof(AzureAIIndexBulkAction.Remove)),
        ];

        return View(model);
    }

    [HttpPost, ActionName(nameof(Index))]
    [FormValueRequired("submit.Filter")]
    public ActionResult IndexFilterPOST(AdminIndexViewModel model)
    {
        return RedirectToAction(nameof(Index),
            new RouteValueDictionary
            {
                { "Options.Search", model.Options.Search }
            });
    }

    [HttpPost, ActionName(nameof(Index))]
    [FormValueRequired("submit.BulkAction")]
    public async Task<ActionResult> IndexPost(AzureAIIndexOptions options, IEnumerable<string> itemIds)
    {
        if (!await _authorizationService.AuthorizeAsync(User, AzureAIIndexPermissionHelper.ManageAzureAIIndexes))
        {
            return Forbid();
        }

        if (itemIds?.Count() > 0)
        {
            var indexSettings = await _indexSettingsService.GetSettingsAsync();
            var checkedContentItems = indexSettings.Where(x => itemIds.Contains(x.IndexName));

            switch (options.BulkAction)
            {
                case AzureAIIndexBulkAction.None:
                    break;
                case AzureAIIndexBulkAction.Remove:
                    foreach (var item in checkedContentItems)
                    {
                        await _indexManager.DeleteAsync(item.IndexName);
                    }

                    await _notifier.SuccessAsync(H["Indices successfully removed."]);

                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(options.BulkAction), "Unknown bulk action");
            }
        }

        return RedirectToAction(nameof(Index));
    }

    public async Task<ActionResult> Edit(string indexName = null)
    {
        if (!await _authorizationService.AuthorizeAsync(User, AzureAIIndexPermissionHelper.ManageAzureAIIndexes))
        {
            return Forbid();
        }

        var IsCreate = string.IsNullOrWhiteSpace(indexName);
        var settings = new AzureAIIndexSettings();

        if (!IsCreate)
        {
            settings = await _indexSettingsService.GetSettingsAsync(indexName);

            if (settings == null)
            {
                return NotFound();
            }
        }

        var model = new AzureAISettingsViewModel
        {
            IsCreate = IsCreate,
            IndexName = IsCreate ? string.Empty : settings.IndexName,
            AnalyzerName = settings.AnalyzerName,
            IndexLatest = settings.IndexLatest,
            Culture = settings.Culture,
            IndexedContentTypes = settings.IndexedContentTypes,
        };

        if (string.IsNullOrEmpty(model.AnalyzerName))
        {
            model.AnalyzerName = AzureAIOptions.DefaultAnalyzer;
        }

        PopulateMenuOptions(model);

        return View(model);
    }

    [HttpPost, ActionName(nameof(Edit))]
    public async Task<ActionResult> EditPost(AzureAISettingsViewModel model, string[] indexedContentTypes)
    {
        if (!await _authorizationService.AuthorizeAsync(User, AzureAIIndexPermissionHelper.ManageAzureAIIndexes))
        {
            return Forbid();
        }

        ValidateModel(model);

        if (model.IsCreate)
        {
            if (await _indexManager.ExistsAsync(model.IndexName))
            {
                ModelState.AddModelError(nameof(AzureAISettingsViewModel.IndexName), S["An index named <em>{0}</em> already exists.", model.IndexName]);
            }
        }
        else
        {
            if (!await _indexManager.ExistsAsync(model.IndexName))
            {
                ModelState.AddModelError(nameof(AzureAISettingsViewModel.IndexName), S["An index named <em>{0}</em> doesn't exist.", model.IndexName]);
            }
        }

        if (!ModelState.IsValid)
        {
            PopulateMenuOptions(model);

            return View(model);
        }

        if (model.IsCreate)
        {
            try
            {
                var settings = new AzureAIIndexSettings
                {
                    IndexName = model.IndexName,
                    AnalyzerName = model.AnalyzerName,
                    QueryAnalyzerName = model.AnalyzerName,
                    IndexLatest = model.IndexLatest,
                    IndexedContentTypes = indexedContentTypes,
                    Culture = model.Culture ?? string.Empty,
                };

                if (string.IsNullOrEmpty(settings.AnalyzerName))
                {
                    settings.AnalyzerName = AzureAIOptions.DefaultAnalyzer;
                }

                if (string.IsNullOrEmpty(settings.QueryAnalyzerName))
                {
                    settings.QueryAnalyzerName = AzureAIOptions.DefaultAnalyzer;
                }

                await SetMappingsAsync(settings);

                var result = await _indexManager.CreateAsync(settings);

                if (!result)
                {
                    await _notifier.ErrorAsync(H["An error occurred while creating the index."]);
                }
                else
                {
                    await _indexSettingsService.UpdateIndexAsync(settings);

                    await _notifier.SuccessAsync(H["Index <em>{0}</em> created successfully.", model.IndexName]);

                    return RedirectToAction(nameof(Index));
                }
            }
            catch (Exception e)
            {
                await _notifier.ErrorAsync(H["An error occurred while creating the index."]);

                _logger.LogError(e, "An error occurred while creating an index {indexName}.", _indexManager.GetFullIndexName(model.IndexName));
            }
        }
        else
        {
            try
            {
                var settings = new AzureAIIndexSettings
                {
                    IndexName = model.IndexName,
                    AnalyzerName = model.AnalyzerName,
                    QueryAnalyzerName = model.AnalyzerName,
                    IndexLatest = model.IndexLatest,
                    IndexedContentTypes = indexedContentTypes,
                    Culture = model.Culture ?? string.Empty,
                };

                if (string.IsNullOrEmpty(settings.AnalyzerName))
                {
                    settings.AnalyzerName = AzureAIOptions.DefaultAnalyzer;
                }

                if (string.IsNullOrEmpty(settings.QueryAnalyzerName))
                {
                    settings.QueryAnalyzerName = AzureAIOptions.DefaultAnalyzer;
                }

                await _indexSettingsService.UpdateIndexAsync(settings);

                await _notifier.SuccessAsync(H["Index <em>{0}</em> modified successfully.", model.IndexName]);

                return RedirectToAction("Index");
            }
            catch (Exception e)
            {
                await _notifier.ErrorAsync(H["An error occurred while editing the index."]);
                _logger.LogError(e, "An error occurred while editing an index {indexName}.", _indexManager.GetFullIndexName(model.IndexName));
            }
        }

        PopulateMenuOptions(model);

        return View(model);
    }

    [HttpPost]
    public async Task<ActionResult> Delete(AzureAISettingsViewModel model)
    {
        if (!await _authorizationService.AuthorizeAsync(User, AzureAIIndexPermissionHelper.ManageAzureAIIndexes))
        {
            return Forbid();
        }

        await _indexSettingsService.DeleteIndexAsync(model.IndexName);

        if (await _indexManager.DeleteAsync(model.IndexName))
        {
            await _indexSettingsService.DeleteIndexAsync(model.IndexName);

            await _notifier.SuccessAsync(H["Index <em>{0}</em> deleted successfully.", model.IndexName]);
        }
        else
        {
            await _notifier.ErrorAsync(H["An error occurred while deleting the <em>{0}</em> index.", model.IndexName]);
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<ActionResult> Rebuild(string id)
    {
        if (!await _authorizationService.AuthorizeAsync(User, AzureAIIndexPermissionHelper.ManageAzureAIIndexes))
        {
            return Forbid();
        }

        if (!await _indexManager.ExistsAsync(id))
        {
            return NotFound();
        }

        var settings = await _indexSettingsService.GetSettingsAsync(id);

        if (settings == null)
        {
            return NotFound();
        }

        await SetMappingsAsync(settings);

        await _indexManager.RebuildIndexAsync(settings);

        if (settings.QueryAnalyzerName != settings.AnalyzerName)
        {
            // Query Analyzer may be different until the index in rebuilt.
            // Since the index is rebuilt, lets make sure we query using the same analyzer.
            settings.QueryAnalyzerName = settings.AnalyzerName;

            await _indexSettingsService.UpdateIndexAsync(settings);
        }

        await _notifier.SuccessAsync(H["Index <em>{0}</em> rebuilt successfully.", id]);

        return RedirectToAction("Index");
    }

    private void PopulateMenuOptions(AzureAISettingsViewModel model)
    {
        model.Cultures = CultureInfo.GetCultures(CultureTypes.AllCultures)
            .Select(x => new SelectListItem { Text = $"{x.Name} ({x.DisplayName})", Value = x.Name });

        model.Analyzers = _azureAIOptions.Analyzers
            .Select(x => new SelectListItem { Text = x, Value = x });
    }

    private void ValidateModel(AzureAISettingsViewModel model)
    {
        if (model.IndexedContentTypes == null || model.IndexedContentTypes.Length < 1)
        {
            ModelState.AddModelError(nameof(AzureAISettingsViewModel.IndexedContentTypes), S["At least one content type is required."]);
        }

        if (string.IsNullOrWhiteSpace(model.IndexName))
        {
            ModelState.AddModelError(nameof(AzureAISettingsViewModel.IndexName), S["The index name is required."]);
        }
        else if (!_indexManager.TryGetSafeName(model.IndexName, out var indexName) || indexName != model.IndexName)
        {
            ModelState.AddModelError(nameof(AzureAISettingsViewModel.IndexName), S["The index name contains forbidden characters."]);
        }
    }

    private async Task SetMappingsAsync(AzureAIIndexSettings settings)
    {
        settings.IndexMappings = [];
        foreach (var contentType in settings.IndexedContentTypes)
        {
            var contentItem = await _contentManager.NewAsync(contentType);
            var index = new DocumentIndex(contentItem.ContentItemId, contentItem.ContentItemVersionId);
            var buildIndexContext = new BuildIndexContext(index, contentItem, [contentType], new AzureAIContentIndexSettings());
            await _contentItemIndexHandlers.InvokeAsync(x => x.BuildIndexAsync(buildIndexContext), _logger);

            foreach (var entry in index.Entries)
            {
                settings.IndexMappings.Add(new AzureAIIndexMap(entry.Name, entry.Type, entry.Options));
            }
        }
    }
}
