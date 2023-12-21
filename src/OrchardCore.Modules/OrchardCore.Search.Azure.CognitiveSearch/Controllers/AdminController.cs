using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.Notify;
using OrchardCore.Liquid;
using OrchardCore.Navigation;
using OrchardCore.Routing;
using OrchardCore.Search.Azure.CognitiveSearch.Models;
using OrchardCore.Search.Azure.CognitiveSearch.Services;
using OrchardCore.Search.Azure.CognitiveSearch.ViewModels;
using OrchardCore.Settings;
using YesSql;

namespace OrchardCore.Search.Azure.CognitiveSearch.Controllers;

public class AdminController : Controller
{
    private readonly ISession _session;
    private readonly ISiteService _siteService;
    private readonly ILiquidTemplateManager _liquidTemplateManager;
    private readonly IContentDefinitionManager _contentDefinitionManager;
    private readonly IAuthorizationService _authorizationService;
    private readonly AzureCognitiveSearchIndexManager _azureCognitiveSearchIndexManager;
    private readonly CognitiveSearchIndexSettingsService _cognitiveSearchIndexSettingsService;
    private readonly JavaScriptEncoder _javaScriptEncoder;
    private readonly IShapeFactory _shapeFactory;
    private readonly AzureCognitiveSearchOptions _azureCognitiveSearchOptions;
    private readonly INotifier _notifier;
    private readonly ILogger _logger;

    protected readonly IStringLocalizer S;
    protected readonly IHtmlLocalizer H;

    public AdminController(
        ISession session,
        ISiteService siteService,
        ILiquidTemplateManager liquidTemplateManager,
        IContentDefinitionManager contentDefinitionManager,
        IAuthorizationService authorizationService,
        AzureCognitiveSearchIndexManager azureCognitiveSearchIndexManager,
        CognitiveSearchIndexSettingsService cognitiveSearchIndexSettingsService,
        JavaScriptEncoder javaScriptEncoder,
        IShapeFactory shapeFactory,
        IOptions<AzureCognitiveSearchOptions> azureCognitiveSearchOptions,
        INotifier notifier,
        ILogger<AdminController> logger,
        IStringLocalizer<AdminController> stringLocalizer,
        IHtmlLocalizer<AdminController> htmlLocalizer
        )
    {
        _session = session;
        _siteService = siteService;
        _liquidTemplateManager = liquidTemplateManager;
        _contentDefinitionManager = contentDefinitionManager;
        _authorizationService = authorizationService;
        _azureCognitiveSearchIndexManager = azureCognitiveSearchIndexManager;
        _cognitiveSearchIndexSettingsService = cognitiveSearchIndexSettingsService;
        _javaScriptEncoder = javaScriptEncoder;
        _shapeFactory = shapeFactory;
        _azureCognitiveSearchOptions = azureCognitiveSearchOptions.Value;
        _notifier = notifier;
        _logger = logger;

        S = stringLocalizer;
        H = htmlLocalizer;
    }

    public async Task<IActionResult> Index(ContentOptions options, PagerParameters pagerParameters)
    {
        if (!await _authorizationService.AuthorizeAsync(User, AzureCognitiveSearchIndexPermissionHelper.ManageAzureCognitiveSearchIndexes))
        {
            return Forbid();
        }

        var indices = (await _cognitiveSearchIndexSettingsService.GetSettingsAsync()).Select(i => new IndexViewModel { Name = i.IndexName }).ToList();

        var siteSettings = await _siteService.GetSiteSettingsAsync();
        var pager = new Pager(pagerParameters, siteSettings.PageSize);
        var results = indices;

        if (!string.IsNullOrWhiteSpace(options.Search))
        {
            results = results.Where(q => q.Name.Contains(options.Search, StringComparison.OrdinalIgnoreCase)).ToList();
        }

        results = results
            .Skip(pager.GetStartIndex())
            .Take(pager.PageSize).ToList();

        // Maintain previous route data when generating page links
        var routeData = new RouteData();
        var pagerShape = await _shapeFactory.CreateAsync("Pager", Arguments.From(new
        {
            pager.Page,
            pager.PageSize,
            TotalItemCount = indices.Count
        }));

        var model = new AdminIndexViewModel
        {
            Indexes = results,
            Options = options,
            Pager = pagerShape
        };

        model.Options.ContentsBulkAction = [
            new SelectListItem(S["Delete"], nameof(ContentsBulkAction.Remove)),
        ];

        return View(model);
    }

    [HttpPost, ActionName("Index")]
    [FormValueRequired("submit.Filter")]
    public ActionResult IndexFilterPOST(AdminIndexViewModel model)
    {
        return RedirectToAction("Index", new RouteValueDictionary {
                { "Options.Search", model.Options.Search }
            });
    }

    [HttpPost, ActionName("Index")]
    [FormValueRequired("submit.BulkAction")]
    public async Task<ActionResult> IndexPost(ContentOptions options, IEnumerable<string> itemIds)
    {
        if (!await _authorizationService.AuthorizeAsync(User, AzureCognitiveSearchIndexPermissionHelper.ManageAzureCognitiveSearchIndexes))
        {
            return Forbid();
        }

        if (itemIds?.Count() > 0)
        {
            var indexSettings = await _cognitiveSearchIndexSettingsService.GetSettingsAsync();
            var checkedContentItems = indexSettings.Where(x => itemIds.Contains(x.IndexName));

            switch (options.BulkAction)
            {
                case ContentsBulkAction.None:
                    break;
                case ContentsBulkAction.Remove:
                    foreach (var item in checkedContentItems)
                    {
                        await _azureCognitiveSearchIndexManager.DeleteAsync(item.IndexName);
                    }
                    await _notifier.SuccessAsync(H["Indices successfully removed."]);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(options.BulkAction), "Unknown bulk action");
            }
        }

        return RedirectToAction("Index");
    }

    public async Task<ActionResult> Edit(string indexName = null)
    {
        if (!await _authorizationService.AuthorizeAsync(User, AzureCognitiveSearchIndexPermissionHelper.ManageAzureCognitiveSearchIndexes))
        {
            return Forbid();
        }

        var IsCreate = string.IsNullOrWhiteSpace(indexName);
        var settings = new CognitiveSearchIndexSettings();

        if (!IsCreate)
        {
            settings = await _cognitiveSearchIndexSettingsService.GetSettingsAsync(indexName);

            if (settings == null)
            {
                return NotFound();
            }
        }

        var model = new CognitiveSearchSettingsViewModel
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
            model.AnalyzerName = AzureCognitiveSearchOptions.DefaultAnalyzer;
        }

        PopulateMenuOptions(model);

        return View(model);
    }

    [HttpPost, ActionName("Edit")]
    public async Task<ActionResult> EditPost(CognitiveSearchSettingsViewModel model, string[] indexedContentTypes)
    {
        if (!await _authorizationService.AuthorizeAsync(User, AzureCognitiveSearchIndexPermissionHelper.ManageAzureCognitiveSearchIndexes))
        {
            return Forbid();
        }

        ValidateModel(model);

        if (model.IsCreate)
        {
            if (await _azureCognitiveSearchIndexManager.ExistsAsync(model.IndexName))
            {
                ModelState.AddModelError(nameof(CognitiveSearchSettingsViewModel.IndexName), S["An index named '{0}' already exists.", model.IndexName]);
            }
        }
        else
        {
            if (!await _azureCognitiveSearchIndexManager.ExistsAsync(model.IndexName))
            {
                ModelState.AddModelError(nameof(CognitiveSearchSettingsViewModel.IndexName), S["An index named '{0}' doesn't exist.", model.IndexName]);
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
                var settings = new CognitiveSearchIndexSettings
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
                    settings.AnalyzerName = AzureCognitiveSearchOptions.DefaultAnalyzer;
                }

                if (string.IsNullOrEmpty(settings.QueryAnalyzerName))
                {
                    settings.QueryAnalyzerName = AzureCognitiveSearchOptions.DefaultAnalyzer;
                }

                // We call Rebuild in order to reset the index state cursor too in case the same index
                // name was also used previously.
                var result = await _azureCognitiveSearchIndexManager.CreateAsync(settings);

                if (!result)
                {
                    await _notifier.ErrorAsync(H["An error occurred while creating the index."]);
                }
                else
                {
                    await _cognitiveSearchIndexSettingsService.UpdateIndexAsync(settings);

                    await _notifier.SuccessAsync(H["Index <em>{0}</em> created successfully.", model.IndexName]);

                    return RedirectToAction("Index");
                }
            }
            catch (Exception e)
            {
                await _notifier.ErrorAsync(H["An error occurred while creating the index."]);

                _logger.LogError(e, "An error occurred while creating an index.");
            }
        }
        else
        {
            try
            {
                var settings = new CognitiveSearchIndexSettings
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
                    settings.AnalyzerName = AzureCognitiveSearchOptions.DefaultAnalyzer;
                }

                if (string.IsNullOrEmpty(settings.QueryAnalyzerName))
                {
                    settings.QueryAnalyzerName = AzureCognitiveSearchOptions.DefaultAnalyzer;
                }

                await _cognitiveSearchIndexSettingsService.UpdateIndexAsync(settings);

                await _notifier.SuccessAsync(H["Index <em>{0}</em> modified successfully, <strong>please consider doing a rebuild on the index.</strong>", model.IndexName]);

                return RedirectToAction("Index");
            }
            catch (Exception e)
            {
                await _notifier.ErrorAsync(H["An error occurred while editing the index."]);
                _logger.LogError(e, "An error occurred while editing an index.");
            }
        }

        PopulateMenuOptions(model);

        return View(model);
    }

    [HttpPost]
    public async Task<ActionResult> Delete(CognitiveSearchSettingsViewModel model)
    {
        if (!await _authorizationService.AuthorizeAsync(User, AzureCognitiveSearchIndexPermissionHelper.ManageAzureCognitiveSearchIndexes))
        {
            return Forbid();
        }
        await _cognitiveSearchIndexSettingsService.DeleteIndexAsync(model.IndexName);

        if (await _azureCognitiveSearchIndexManager.DeleteAsync(model.IndexName))
        {
            await _cognitiveSearchIndexSettingsService.DeleteIndexAsync(model.IndexName);

            await _notifier.SuccessAsync(H["Index <em>{0}</em> deleted successfully.", model.IndexName]);
        }
        else
        {
            await _notifier.ErrorAsync(H["An error occurred while deleting the index.", model.IndexName]);
        }

        return RedirectToAction("Index");
    }

    private void PopulateMenuOptions(CognitiveSearchSettingsViewModel model)
    {
        model.Cultures = CultureInfo.GetCultures(CultureTypes.AllCultures)
            .Select(x => new SelectListItem { Text = $"{x.Name} ({x.DisplayName})", Value = x.Name });

        model.Analyzers = _azureCognitiveSearchOptions.Analyzers
            .Select(x => new SelectListItem { Text = x, Value = x });
    }

    private void ValidateModel(CognitiveSearchSettingsViewModel model)
    {
        if (model.IndexedContentTypes == null || model.IndexedContentTypes.Length < 1)
        {
            ModelState.AddModelError(nameof(CognitiveSearchSettingsViewModel.IndexedContentTypes), S["At least one content type is required."]);
        }

        if (string.IsNullOrWhiteSpace(model.IndexName))
        {
            ModelState.AddModelError(nameof(CognitiveSearchSettingsViewModel.IndexName), S["The index name is required."]);
        }
        else if (!_azureCognitiveSearchIndexManager.TryGetSafeName(model.IndexName, out var indexName) || indexName != model.IndexName)
        {
            ModelState.AddModelError(nameof(CognitiveSearchSettingsViewModel.IndexName), S["The index name contains not allowed chars."]);
        }
    }
}
