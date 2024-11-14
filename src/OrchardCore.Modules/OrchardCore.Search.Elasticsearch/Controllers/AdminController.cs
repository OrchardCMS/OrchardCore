using System.Diagnostics;
using System.Globalization;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Nodes;
using Fluid;
using Fluid.Values;
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
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.Notify;
using OrchardCore.Liquid;
using OrchardCore.Localization;
using OrchardCore.Navigation;
using OrchardCore.Routing;
using OrchardCore.Search.Elasticsearch.Core.Models;
using OrchardCore.Search.Elasticsearch.Core.Services;
using OrchardCore.Search.Elasticsearch.ViewModels;
using OrchardCore.Settings;
using YesSql;

namespace OrchardCore.Search.Elasticsearch;

[Admin("elasticsearch/{action}/{id?}", "Elasticsearch.{action}")]
public sealed class AdminController : Controller
{
    private const string _optionsSearch = "Options.Search";

    private readonly ISession _session;
    private readonly ISiteService _siteService;
    private readonly ILiquidTemplateManager _liquidTemplateManager;
    private readonly IContentDefinitionManager _contentDefinitionManager;
    private readonly IAuthorizationService _authorizationService;
    private readonly IElasticQueryService _queryService;
    private readonly ElasticIndexManager _elasticIndexManager;
    private readonly ElasticIndexingService _elasticIndexingService;
    private readonly ElasticIndexSettingsService _elasticIndexSettingsService;
    private readonly JavaScriptEncoder _javaScriptEncoder;
    private readonly ElasticsearchOptions _elasticSearchOptions;
    private readonly INotifier _notifier;
    private readonly ILogger _logger;
    private readonly IOptions<TemplateOptions> _templateOptions;
    private readonly ElasticConnectionOptions _elasticConnectionOptions;
    private readonly IShapeFactory _shapeFactory;
    private readonly ILocalizationService _localizationService;

    internal readonly IStringLocalizer S;
    internal readonly IHtmlLocalizer H;

    public AdminController(
        ISession session,
        ISiteService siteService,
        ILiquidTemplateManager liquidTemplateManager,
        IContentDefinitionManager contentDefinitionManager,
        IAuthorizationService authorizationService,
        IElasticQueryService queryService,
        ElasticIndexManager elasticIndexManager,
        ElasticIndexingService elasticIndexingService,
        ElasticIndexSettingsService elasticIndexSettingsService,
        JavaScriptEncoder javaScriptEncoder,
        IOptions<ElasticsearchOptions> elasticSearchOptions,
        INotifier notifier,
        ILogger<AdminController> logger,
        IOptions<TemplateOptions> templateOptions,
        IOptions<ElasticConnectionOptions> elasticConnectionOptions,
        IShapeFactory shapeFactory,
        ILocalizationService localizationService,
        IStringLocalizer<AdminController> stringLocalizer,
        IHtmlLocalizer<AdminController> htmlLocalizer)
    {
        _session = session;
        _siteService = siteService;
        _liquidTemplateManager = liquidTemplateManager;
        _contentDefinitionManager = contentDefinitionManager;
        _authorizationService = authorizationService;
        _queryService = queryService;
        _elasticIndexManager = elasticIndexManager;
        _elasticIndexingService = elasticIndexingService;
        _elasticIndexSettingsService = elasticIndexSettingsService;
        _javaScriptEncoder = javaScriptEncoder;
        _elasticSearchOptions = elasticSearchOptions.Value;
        _notifier = notifier;
        _logger = logger;
        _templateOptions = templateOptions;
        _elasticConnectionOptions = elasticConnectionOptions.Value;
        _shapeFactory = shapeFactory;
        _localizationService = localizationService;
        S = stringLocalizer;
        H = htmlLocalizer;
    }

    public async Task<IActionResult> Index(ContentOptions options, PagerParameters pagerParameters)
    {
        if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageElasticIndexes))
        {
            return Forbid();
        }

        if (!_elasticConnectionOptions.FileConfigurationExists())
        {
            return NotConfigured();
        }

        var indexes = (await _elasticIndexSettingsService.GetSettingsAsync())
            .Select(i => new IndexViewModel { Name = i.IndexName })
            .ToList();

        var totalIndexes = indexes.Count;
        var siteSettings = await _siteService.GetSiteSettingsAsync();
        var pager = new Pager(pagerParameters, siteSettings.PageSize);

        if (!string.IsNullOrWhiteSpace(options.Search))
        {
            indexes = indexes.Where(q => q.Name.Contains(options.Search, StringComparison.OrdinalIgnoreCase)).ToList();
        }

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

        var pagerShape = await _shapeFactory.PagerAsync(pager, totalIndexes, routeData);

        var model = new AdminIndexViewModel
        {
            Indexes = indexes,
            Options = options,
            Pager = pagerShape
        };

        model.Options.ContentsBulkAction =
        [
            new SelectListItem(S["Reset"], nameof(ContentsBulkAction.Reset)),
            new SelectListItem(S["Rebuild"], nameof(ContentsBulkAction.Rebuild)),
            new SelectListItem(S["Delete"], nameof(ContentsBulkAction.Remove)),
        ];


        return View(model);
    }

    [HttpPost, ActionName(nameof(Index))]
    [FormValueRequired("submit.Filter")]
    public IActionResult IndexFilterPOST(AdminIndexViewModel model)
        => RedirectToAction(nameof(Index), new RouteValueDictionary
        {
            { _optionsSearch, model.Options.Search }
        });

    public async Task<IActionResult> Edit(string indexName = null)
    {
        var IsCreate = string.IsNullOrWhiteSpace(indexName);
        var settings = new ElasticIndexSettings();

        if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageElasticIndexes))
        {
            return Forbid();
        }

        if (!_elasticConnectionOptions.FileConfigurationExists())
        {
            return NotConfigured();
        }

        if (!IsCreate)
        {
            settings = await _elasticIndexSettingsService.GetSettingsAsync(indexName);

            if (settings == null)
            {
                return NotFound();
            }
        }

        var model = new ElasticIndexSettingsViewModel
        {
            IsCreate = IsCreate,
            IndexName = IsCreate ? string.Empty : settings.IndexName,
            AnalyzerName = IsCreate ? "standardanalyzer" : settings.AnalyzerName,
            IndexLatest = settings.IndexLatest,
            Culture = settings.Culture,
            IndexedContentTypes = settings.IndexedContentTypes,
            StoreSourceData = settings.StoreSourceData
        };

        await PopulateMenuOptionsAsync(model);

        return View(model);
    }

    [HttpPost, ActionName(nameof(Edit))]
    public async Task<ActionResult> EditPost(ElasticIndexSettingsViewModel model, string[] indexedContentTypes)
    {
        if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageElasticIndexes))
        {
            return Forbid();
        }

        if (!_elasticConnectionOptions.FileConfigurationExists())
        {
            return BadRequest();
        }

        ValidateModel(model);

        if (model.IsCreate)
        {
            if (await _elasticIndexManager.ExistsAsync(model.IndexName))
            {
                ModelState.AddModelError(nameof(ElasticIndexSettingsViewModel.IndexName), S["An index named {0} already exists.", model.IndexName]);
            }
        }
        else
        {
            if (!await _elasticIndexManager.ExistsAsync(model.IndexName))
            {
                ModelState.AddModelError(nameof(ElasticIndexSettingsViewModel.IndexName), S["An index named {0} doesn't exist.", model.IndexName]);
            }
        }

        if (!ModelState.IsValid)
        {
            await PopulateMenuOptionsAsync(model);

            return View(model);
        }

        if (model.IsCreate)
        {
            try
            {
                var settings = new ElasticIndexSettings
                {
                    IndexName = model.IndexName,
                    AnalyzerName = model.AnalyzerName,
                    QueryAnalyzerName = model.AnalyzerName,
                    IndexLatest = model.IndexLatest,
                    IndexedContentTypes = indexedContentTypes,
                    Culture = model.Culture ?? string.Empty,
                    StoreSourceData = model.StoreSourceData
                };

                // We call Rebuild in order to reset the index state cursor too in case the same index
                // name was also used previously.
                await _elasticIndexingService.CreateIndexAsync(settings);
            }
            catch (Exception e)
            {
                await _notifier.ErrorAsync(H["An error occurred while creating the index."]);
                _logger.LogError(e, "An error occurred while creating index: {IndexName}.", _elasticIndexManager.GetFullIndexName(model.IndexName));

                await PopulateMenuOptionsAsync(model);

                return View(model);
            }

            await _notifier.SuccessAsync(H["Index <em>{0}</em> created successfully.", model.IndexName]);
        }
        else
        {
            try
            {
                var settings = new ElasticIndexSettings
                {
                    IndexName = model.IndexName,
                    AnalyzerName = model.AnalyzerName,
                    IndexLatest = model.IndexLatest,
                    IndexedContentTypes = indexedContentTypes,
                    Culture = model.Culture ?? string.Empty,
                    StoreSourceData = model.StoreSourceData
                };

                await _elasticIndexingService.UpdateIndexAsync(settings);
            }
            catch (Exception e)
            {
                await _notifier.ErrorAsync(H["An error occurred while editing the index."]);
                _logger.LogError(e, "An error occurred while editing index: {IndexName}.", _elasticIndexManager.GetFullIndexName(model.IndexName));

                await PopulateMenuOptionsAsync(model);

                return View(model);
            }

            await _notifier.SuccessAsync(H["Index <em>{0}</em> modified successfully, <strong>please consider rebuilding the index.</strong>", model.IndexName]);
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<ActionResult> Reset(string id)
    {
        if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageElasticIndexes))
        {
            return Forbid();
        }

        if (!_elasticConnectionOptions.FileConfigurationExists())
        {
            return BadRequest();
        }

        if (!await _elasticIndexManager.ExistsAsync(id))
        {
            return NotFound();
        }

        await _elasticIndexingService.ResetIndexAsync(id);
        await ProcessContentItemsAsync(id);

        await _notifier.SuccessAsync(H["Index <em>{0}</em> reset successfully.", id]);

        return RedirectToAction("Index");
    }

    [HttpPost]
    public async Task<ActionResult> Rebuild(string id)
    {
        if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageElasticIndexes))
        {
            return Forbid();
        }

        if (!_elasticConnectionOptions.FileConfigurationExists())
        {
            return BadRequest();
        }

        if (!await _elasticIndexManager.ExistsAsync(id))
        {
            return NotFound();
        }

        var settings = await _elasticIndexSettingsService.GetSettingsAsync(id);

        await _elasticIndexingService.RebuildIndexAsync(settings);

        if (settings.QueryAnalyzerName != settings.AnalyzerName)
        {
            // Query Analyzer may be different until the index in rebuilt.
            // Since the index is rebuilt, lets make sure we query using the same analyzer.
            settings.QueryAnalyzerName = settings.AnalyzerName;

            await _elasticIndexSettingsService.UpdateIndexAsync(settings);
        }

        await ProcessContentItemsAsync(id);

        await _notifier.SuccessAsync(H["Index <em>{0}</em> rebuilt successfully.", id]);

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<ActionResult> Delete(ElasticIndexSettingsViewModel model)
    {
        if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageElasticIndexes))
        {
            return Forbid();
        }

        if (!_elasticConnectionOptions.FileConfigurationExists())
        {
            return BadRequest();
        }

        if (!await _elasticIndexManager.ExistsAsync(model.IndexName))
        {
            await _notifier.SuccessAsync(H["Index not found on Elasticsearch server.", model.IndexName]);
            return RedirectToAction("Index");
        }

        try
        {
            await _elasticIndexingService.DeleteIndexAsync(model.IndexName);

            await _notifier.SuccessAsync(H["Index <em>{0}</em> deleted successfully.", model.IndexName]);
        }
        catch (Exception e)
        {
            await _notifier.ErrorAsync(H["An error occurred while deleting the index."]);
            _logger.LogError(e, "An error occurred while deleting the index {IndexName}", _elasticIndexManager.GetFullIndexName(model.IndexName));
        }

        return RedirectToAction("Index");
    }

    [HttpPost]
    public async Task<ActionResult> ForceDelete(ElasticIndexSettingsViewModel model)
    {
        if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageElasticIndexes))
        {
            return Forbid();
        }

        if (!_elasticConnectionOptions.FileConfigurationExists())
        {
            return BadRequest();
        }

        try
        {
            await _elasticIndexingService.DeleteIndexAsync(model.IndexName);

            await _notifier.SuccessAsync(H["Index <em>{0}</em> deleted successfully.", model.IndexName]);
        }
        catch (Exception e)
        {
            await _notifier.ErrorAsync(H["An error occurred while deleting the index."]);
            _logger.LogError(e, "An error occurred while deleting the index {IndexName}", _elasticIndexManager.GetFullIndexName(model.IndexName));
        }

        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> IndexInfo(string indexName)
    {
        var info = await _elasticIndexManager.GetIndexInfo(indexName);

        var formattedJson = JNode.Parse(info).ToJsonString(JOptions.Indented);
        return View(new IndexInfoViewModel
        {
            IndexName = _elasticIndexManager.GetFullIndexName(indexName),
            IndexInfo = formattedJson
        });
    }

    public async Task<IActionResult> SyncSettings()
    {
        if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageElasticIndexes))
        {
            return Forbid();
        }

        await _elasticIndexingService.SyncSettings();

        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Query(string indexName, string query)
    {
        if (!_elasticConnectionOptions.FileConfigurationExists())
        {
            return NotConfigured();
        }

        return await Query(new AdminQueryViewModel
        {
            IndexName = indexName,
            DecodedQuery = string.IsNullOrWhiteSpace(query)
            ? string.Empty
            : Base64.FromUTF8Base64String(query)
        });
    }

    [HttpPost]
    public async Task<IActionResult> Query(AdminQueryViewModel model)
    {
        if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageElasticIndexes))
        {
            return Forbid();
        }

        if (!_elasticConnectionOptions.FileConfigurationExists())
        {
            return BadRequest();
        }

        model.Indices = (await _elasticIndexSettingsService.GetSettingsAsync()).Select(x => x.IndexName).ToArray();

        // Can't query if there are no indices.
        if (model.Indices.Length == 0)
        {
            return RedirectToAction(nameof(Index));
        }

        if (string.IsNullOrEmpty(model.IndexName))
        {
            model.IndexName = model.Indices[0];
        }

        if (!await _elasticIndexManager.ExistsAsync(model.IndexName))
        {
            return NotFound();
        }

        if (string.IsNullOrWhiteSpace(model.DecodedQuery))
        {
            return View(model);
        }

        if (string.IsNullOrEmpty(model.Parameters))
        {
            model.Parameters = "{ }";
        }

        var stopwatch = new Stopwatch();
        stopwatch.Start();

        var parameters = JConvert.DeserializeObject<Dictionary<string, object>>(model.Parameters);
        var tokenizedContent = await _liquidTemplateManager.RenderStringAsync(model.DecodedQuery, _javaScriptEncoder, parameters.Select(x => new KeyValuePair<string, FluidValue>(x.Key, FluidValue.Create(x.Value, _templateOptions.Value))));

        try
        {
            var elasticTopDocs = await _queryService.SearchAsync(model.IndexName, tokenizedContent);

            if (elasticTopDocs != null)
            {
                model.Documents = elasticTopDocs.TopDocs.Where(x => x != null);
                model.Fields = elasticTopDocs.Fields;
                model.Count = elasticTopDocs.Count;
            }
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error while executing query");
            ModelState.AddModelError(nameof(model.DecodedQuery), S["Invalid query : {0}", e.Message]);
        }

        stopwatch.Stop();
        model.Elapsed = stopwatch.Elapsed;
        return View(model);
    }

    [HttpPost, ActionName(nameof(Index))]
    [FormValueRequired("submit.BulkAction")]
    public async Task<ActionResult> IndexPost(ContentOptions options, IEnumerable<string> itemIds)
    {
        if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageElasticIndexes))
        {
            return Forbid();
        }

        if (!_elasticConnectionOptions.FileConfigurationExists())
        {
            return BadRequest();
        }

        if (itemIds?.Count() > 0)
        {
            var elasticIndexSettings = await _elasticIndexSettingsService.GetSettingsAsync();
            var checkedContentItems = elasticIndexSettings.Where(x => itemIds.Contains(x.IndexName));

            switch (options.BulkAction)
            {
                case ContentsBulkAction.None:
                    break;
                case ContentsBulkAction.Remove:
                    foreach (var item in checkedContentItems)
                    {
                        await _elasticIndexingService.DeleteIndexAsync(item.IndexName);
                    }
                    await _notifier.SuccessAsync(H["Indices successfully removed."]);
                    break;
                case ContentsBulkAction.Reset:
                    foreach (var item in checkedContentItems)
                    {
                        if (!await _elasticIndexManager.ExistsAsync(item.IndexName))
                        {
                            return NotFound();
                        }

                        await _elasticIndexingService.ResetIndexAsync(item.IndexName);
                        await ProcessContentItemsAsync(item.IndexName);

                        await _notifier.SuccessAsync(H["Index <em>{0}</em> reset successfully.", item.IndexName]);
                    }
                    break;
                case ContentsBulkAction.Rebuild:
                    foreach (var item in checkedContentItems)
                    {
                        if (!await _elasticIndexManager.ExistsAsync(item.IndexName))
                        {
                            return NotFound();
                        }

                        await _elasticIndexingService.RebuildIndexAsync(await _elasticIndexSettingsService.GetSettingsAsync(item.IndexName));

                        await ProcessContentItemsAsync(item.IndexName);
                        await _notifier.SuccessAsync(H["Index <em>{0}</em> rebuilt successfully.", item.IndexName]);
                    }
                    break;
                default:
                    return BadRequest();
            }
        }

        return RedirectToAction(nameof(Index));
    }

    private void ValidateModel(ElasticIndexSettingsViewModel model)
    {
        if (model.IndexedContentTypes == null || model.IndexedContentTypes.Length < 1)
        {
            ModelState.AddModelError(nameof(ElasticIndexSettingsViewModel.IndexedContentTypes), S["At least one content type is required."]);
        }

        if (string.IsNullOrWhiteSpace(model.IndexName))
        {
            ModelState.AddModelError(nameof(ElasticIndexSettingsViewModel.IndexName), S["The index name is required."]);
        }
        else if (ElasticIndexManager.ToSafeIndexName(model.IndexName) != model.IndexName)
        {
            ModelState.AddModelError(nameof(ElasticIndexSettingsViewModel.IndexName), S["The index name contains forbidden characters."]);
        }
    }

    private async Task PopulateMenuOptionsAsync(ElasticIndexSettingsViewModel model)
    {
        var supportedCultures = await _localizationService.GetSupportedCulturesAsync();

        model.Cultures = supportedCultures.Select(c => new SelectListItem
        {
            Text = $"{c} ({CultureInfo.GetCultureInfo(c).DisplayName})",
            Value = c
        });

        model.Analyzers = _elasticSearchOptions.Analyzers
            .Select(x => new SelectListItem { Text = x.Key, Value = x.Key });
    }

    private ViewResult NotConfigured()
        => View("NotConfigured");

    private static Task ProcessContentItemsAsync(string indexName)
        => HttpBackgroundJob.ExecuteAfterEndOfRequestAsync("sync-content-items-elasticsearch-" + indexName, async (scope) =>
        {
            var indexingService = scope.ServiceProvider.GetRequiredService<ElasticIndexingService>();
            await indexingService.ProcessContentItemsAsync(indexName);
        });
}
