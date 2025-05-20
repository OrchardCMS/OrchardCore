using System.Diagnostics;
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
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OrchardCore.Admin;
using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Notify;
using OrchardCore.Liquid;
using OrchardCore.Navigation;
using OrchardCore.Routing;
using OrchardCore.Search.Elasticsearch.Core.Models;
using OrchardCore.Search.Elasticsearch.Core.Services;
using OrchardCore.Search.Elasticsearch.ViewModels;
using OrchardCore.Settings;

namespace OrchardCore.Search.Elasticsearch;

[Admin("elasticsearch/{action}/{id?}", "Elasticsearch.{action}")]
public sealed class AdminController : Controller
{
    private const string _optionsSearch = "Options.Search";

    private readonly ISiteService _siteService;
    private readonly ILiquidTemplateManager _liquidTemplateManager;
    private readonly IAuthorizationService _authorizationService;
    private readonly ElasticsearchIndexManager _indexManager;
    private readonly ElasticsearchIndexSettingsService _indexSettingsService;
    private readonly JavaScriptEncoder _javaScriptEncoder;
    private readonly ElasticsearchOptions _elasticSearchOptions;
    private readonly INotifier _notifier;
    private readonly ILogger _logger;
    private readonly IOptions<TemplateOptions> _templateOptions;
    private readonly ElasticsearchQueryService _elasticQueryService;
    private readonly ElasticsearchConnectionOptions _elasticConnectionOptions;
    private readonly IDisplayManager<ElasticIndexSettings> _displayManager;
    private readonly IUpdateModelAccessor _updateModelAccessor;

    internal readonly IStringLocalizer S;
    internal readonly IHtmlLocalizer H;

    public AdminController(
        ISiteService siteService,
        ILiquidTemplateManager liquidTemplateManager,
        IAuthorizationService authorizationService,
        ElasticsearchIndexManager elasticIndexManager,
        ElasticsearchIndexSettingsService elasticIndexSettingsService,
        JavaScriptEncoder javaScriptEncoder,
        IOptions<ElasticsearchOptions> elasticSearchOptions,
        INotifier notifier,
        ILogger<AdminController> logger,
        IOptions<TemplateOptions> templateOptions,
        IOptions<ElasticsearchConnectionOptions> elasticConnectionOptions,
        ElasticsearchQueryService elasticQueryService,
        IDisplayManager<ElasticIndexSettings> displayManager,
        IUpdateModelAccessor updateModelAccessor,
        IStringLocalizer<AdminController> stringLocalizer,
        IHtmlLocalizer<AdminController> htmlLocalizer)
    {
        _siteService = siteService;
        _liquidTemplateManager = liquidTemplateManager;
        _authorizationService = authorizationService;
        _indexManager = elasticIndexManager;
        _indexSettingsService = elasticIndexSettingsService;
        _javaScriptEncoder = javaScriptEncoder;
        _elasticSearchOptions = elasticSearchOptions.Value;
        _notifier = notifier;
        _logger = logger;
        _templateOptions = templateOptions;
        _elasticQueryService = elasticQueryService;
        _elasticConnectionOptions = elasticConnectionOptions.Value;
        _displayManager = displayManager;
        _updateModelAccessor = updateModelAccessor;
        S = stringLocalizer;
        H = htmlLocalizer;
    }

    public async Task<IActionResult> Index(
        ContentOptions options,
        PagerParameters pagerParameters,
        [FromServices] IShapeFactory shapeFactory)
    {
        if (!await _authorizationService.AuthorizeAsync(User, ElasticsearchPermissions.ManageElasticIndexes))
        {
            return Forbid();
        }

        if (!_elasticConnectionOptions.ConfigurationExists())
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
            Pager = await shapeFactory.PagerAsync(pager, totalIndexes, routeData),
            SourceNames = _elasticSearchOptions.IndexSources.Keys.Order(),
        };

        foreach (var index in indexes)
        {
            model.Indexes.Add(new IndexViewModel
            {
                Index = index,
                Shape = await _displayManager.BuildDisplayAsync(index, _updateModelAccessor.ModelUpdater, "SummaryAdmin"),
            });
        }

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
            { _optionsSearch, model.Options.Search },
        });

    [Admin("elasticsearch/create/{source}", "ElasticsearchCreate")]
    public async Task<IActionResult> Create(string source)
    {
        if (!await _authorizationService.AuthorizeAsync(User, ElasticsearchPermissions.ManageElasticIndexes))
        {
            return Forbid();
        }

        if (!_elasticConnectionOptions.ConfigurationExists())
        {
            return NotConfigured();
        }

        if (!_elasticSearchOptions.IndexSources.TryGetValue(source, out var indexSource))
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
    [Admin("elasticsearch/create/{source}", "ElasticsearchCreate")]

    public async Task<IActionResult> CreatePost(string source)
    {
        if (!await _authorizationService.AuthorizeAsync(User, ElasticsearchPermissions.ManageElasticIndexes))
        {
            return Forbid();
        }

        if (!_elasticConnectionOptions.ConfigurationExists())
        {
            return BadRequest();
        }

        if (!_elasticSearchOptions.IndexSources.TryGetValue(source, out var indexSource))
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
                await _indexSettingsService.CreateAsync(settings);

                if (await _indexManager.CreateIndexAsync(settings))
                {
                    await _indexSettingsService.SynchronizeAsync(settings);
                    await _notifier.SuccessAsync(H["Index <em>{0}</em> created successfully.", settings.IndexName]);

                    return RedirectToAction(nameof(Index));
                }

                await _notifier.ErrorAsync(H["An error occurred while creating the index."]);
            }
            catch (Exception e)
            {
                await _notifier.ErrorAsync(H["An error occurred while creating the index."]);
                _logger.LogError(e, "An error occurred while creating an index {IndexName}.", settings.IndexFullName);
            }
        }

        return View(model);
    }

    [Admin("elasticsearch/Edit/{id}", "ElasticsearchEdit")]
    public async Task<IActionResult> Edit(string id)
    {
        if (!await _authorizationService.AuthorizeAsync(User, ElasticsearchPermissions.ManageElasticIndexes))
        {
            return Forbid();
        }

        if (!_elasticConnectionOptions.ConfigurationExists())
        {
            return NotConfigured();
        }

        var settings = await _indexSettingsService.FindByIdAsync(id);

        if (settings == null)
        {
            return NotFound();
        }

        var model = await _displayManager.BuildEditorAsync(settings, _updateModelAccessor.ModelUpdater, false);

        return View(model);
    }

    [HttpPost]
    [ActionName(nameof(Edit))]
    [Admin("elasticsearch/Edit/{id}", "ElasticsearchEdit")]
    public async Task<ActionResult> EditPost(string id)
    {
        if (!await _authorizationService.AuthorizeAsync(User, ElasticsearchPermissions.ManageElasticIndexes))
        {
            return Forbid();
        }

        if (!_elasticConnectionOptions.ConfigurationExists())
        {
            return BadRequest();
        }

        var settings = await _indexSettingsService.FindByIdAsync(id);

        if (settings == null)
        {
            return NotFound();
        }

        var model = await _displayManager.UpdateEditorAsync(settings, _updateModelAccessor.ModelUpdater, false);

        if (ModelState.IsValid)
        {
            try
            {
                if (!await _indexManager.CreateIndexAsync(settings))
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

                _logger.LogError(e, "An error occurred while updating an index {IndexName}.", settings.IndexFullName);
            }
        }

        return View(model);
    }

    [HttpPost]
    [Admin("elasticsearch/Reset/{id}", "ElasticsearchReset")]
    public async Task<ActionResult> Reset(string id)
    {
        if (!await _authorizationService.AuthorizeAsync(User, ElasticsearchPermissions.ManageElasticIndexes))
        {
            return Forbid();
        }

        if (!_elasticConnectionOptions.ConfigurationExists())
        {
            return BadRequest();
        }

        var settings = await _indexSettingsService.FindByIdAsync(id);

        if (settings == null)
        {
            return NotFound();
        }

        if (!await _indexManager.ExistsAsync(settings.IndexName))
        {
            await _notifier.ErrorAsync(H["Unable to reset the <em>{0}</em> index. Try rebuilding it instead.", settings.IndexName]);

            return RedirectToAction(nameof(Index));
        }

        await _indexSettingsService.ResetAsync(settings);
        await _indexSettingsService.UpdateAsync(settings);
        await _indexSettingsService.SynchronizeAsync(settings);
        await _notifier.SuccessAsync(H["Index <em>{0}</em> reset successfully.", settings.IndexName]);

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [Admin("elasticsearch/Rebuild/{id}", "ElasticsearchRebuild")]
    public async Task<ActionResult> Rebuild(string id)
    {
        if (!await _authorizationService.AuthorizeAsync(User, ElasticsearchPermissions.ManageElasticIndexes))
        {
            return Forbid();
        }

        if (!_elasticConnectionOptions.ConfigurationExists())
        {
            return BadRequest();
        }

        if (!await _authorizationService.AuthorizeAsync(User, ElasticsearchPermissions.ManageElasticIndexes))
        {
            return Forbid();
        }

        if (!_elasticConnectionOptions.ConfigurationExists())
        {
            return BadRequest();
        }

        var settings = await _indexSettingsService.FindByIdAsync(id);

        if (settings == null)
        {
            return NotFound();
        }

        await _indexSettingsService.ResetAsync(settings);
        await _indexSettingsService.UpdateAsync(settings);
        await _indexManager.RebuildIndexAsync(settings);
        await _indexSettingsService.SynchronizeAsync(settings);
        await _notifier.SuccessAsync(H["Index <em>{0}</em> rebuilt successfully.", settings.IndexName]);

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [Admin("elasticsearch/Delete/{id}", "ElasticsearchDelete")]
    public async Task<ActionResult> Delete(string id)
    {
        if (!await _authorizationService.AuthorizeAsync(User, ElasticsearchPermissions.ManageElasticIndexes))
        {
            return Forbid();
        }

        if (!_elasticConnectionOptions.ConfigurationExists())
        {
            return BadRequest();
        }

        var settings = await _indexSettingsService.FindByIdAsync(id);

        if (settings == null)
        {
            return NotFound();
        }

        var exists = await _indexManager.ExistsAsync(settings.IndexName);

        if (!exists)
        {
            // At this point we know that the index does not exists on remote server. Let's delete it locally.
            await _indexSettingsService.DeleteByIdAsync(id);

            await _notifier.SuccessAsync(H["Index <em>{0}</em> deleted successfully.", settings.IndexName]);
        }
        else if (await _indexManager.DeleteIndexAsync(settings.IndexName))
        {
            await _indexSettingsService.DeleteByIdAsync(id);

            await _notifier.SuccessAsync(H["Index <em>{0}</em> deleted successfully.", settings.IndexName]);
        }
        else
        {
            await _notifier.ErrorAsync(H["An error occurred while deleting the <em>{0}</em> index.", settings.IndexName]);
        }

        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> IndexInfo(string id)
    {
        var settings = await _indexSettingsService.FindByIdAsync(id);

        if (settings == null)
        {
            return NotFound();
        }

        var exists = await _indexManager.ExistsAsync(settings.IndexName);
        var info = await _indexManager.GetIndexInfo(settings.IndexName);

        var formattedJson = JNode.Parse(info).ToJsonString(JOptions.Indented);
        return View(new IndexInfoViewModel
        {
            IndexName = settings.IndexFullName,
            IndexInfo = formattedJson,
        });
    }

    public async Task<IActionResult> SyncSettings()
    {
        if (!await _authorizationService.AuthorizeAsync(User, ElasticsearchPermissions.ManageElasticIndexes))
        {
            return Forbid();
        }

        await _indexSettingsService.SynchronizeSettingsAsync();

        return RedirectToAction(nameof(Index));
    }

    public Task<IActionResult> Query(string id, string query)
    {
        if (!_elasticConnectionOptions.ConfigurationExists())
        {
            return Task.FromResult<IActionResult>(NotConfigured());
        }

        return Query(new AdminQueryViewModel
        {
            Id = id,
            DecodedQuery = string.IsNullOrWhiteSpace(query)
            ? string.Empty
            : Base64.FromUTF8Base64String(query),
        });
    }

    [HttpPost]
    public async Task<IActionResult> Query(AdminQueryViewModel model)
    {
        if (!await _authorizationService.AuthorizeAsync(User, ElasticsearchPermissions.ManageElasticIndexes))
        {
            return Forbid();
        }

        if (!_elasticConnectionOptions.ConfigurationExists())
        {
            return BadRequest();
        }

        model.Indices = (await _indexSettingsService.GetSettingsAsync()).Select(x => x.IndexName).ToArray();

        var settings = await _indexSettingsService.FindByIdAsync(model.Id);

        if (settings == null)
        {
            return NotFound();
        }

        // Can't query if there are no indices.
        if (model.Indices.Length == 0)
        {
            return RedirectToAction(nameof(Index));
        }

        if (string.IsNullOrEmpty(model.Id))
        {
            model.Id = model.Indices[0];
        }

        if (!await _indexManager.ExistsAsync(settings.IndexName))
        {
            return NotFound();
        }

        if (string.IsNullOrWhiteSpace(model.DecodedQuery))
        {
            return View(model);
        }

        if (string.IsNullOrEmpty(model.Parameters))
        {
            model.Parameters = "{}";
        }

        var stopwatch = new Stopwatch();
        stopwatch.Start();

        var parameters = JConvert.DeserializeObject<Dictionary<string, object>>(model.Parameters);
        var tokenizedContent = await _liquidTemplateManager.RenderStringAsync(model.DecodedQuery, _javaScriptEncoder, parameters.Select(x => new KeyValuePair<string, FluidValue>(x.Key, FluidValue.Create(x.Value, _templateOptions.Value))));

        try
        {
            var results = await _elasticQueryService.SearchAsync(model.Id, tokenizedContent);

            if (results != null)
            {
                model.Documents = results.TopDocs;
                model.Fields = results.Fields;
                model.Count = results.Count;
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
        if (!await _authorizationService.AuthorizeAsync(User, ElasticsearchPermissions.ManageElasticIndexes))
        {
            return Forbid();
        }

        if (!_elasticConnectionOptions.ConfigurationExists())
        {
            return BadRequest();
        }

        if (itemIds?.Count() > 0)
        {
            switch (options.BulkAction)
            {
                case ContentsBulkAction.None:
                    break;
                case ContentsBulkAction.Remove:
                    foreach (var id in itemIds)
                    {
                        var settings = await _indexSettingsService.FindByIdAsync(id);
                        if (settings is null)
                        {
                            continue;
                        }
                        await _indexSettingsService.DeleteByIdAsync(id);
                        await _indexManager.DeleteIndexAsync(settings.IndexName);
                    }
                    await _notifier.SuccessAsync(H["Indices successfully removed."]);
                    break;
                case ContentsBulkAction.Reset:
                    foreach (var itemId in itemIds)
                    {
                        var settings = await _indexSettingsService.FindByIdAsync(itemId);
                        if (settings is null)
                        {
                            continue;
                        }

                        if (!await _indexManager.ExistsAsync(settings.IndexName))
                        {
                            continue;
                        }

                        await _indexSettingsService.ResetAsync(settings);
                        await _indexSettingsService.UpdateAsync(settings);
                        await _indexSettingsService.SynchronizeAsync(settings);

                        await _notifier.SuccessAsync(H["Index <em>{0}</em> reset successfully.", settings.IndexName]);
                    }
                    break;
                case ContentsBulkAction.Rebuild:
                    foreach (var itemId in itemIds)
                    {
                        var settings = await _indexSettingsService.FindByIdAsync(itemId);
                        if (settings is null)
                        {
                            continue;
                        }

                        if (!await _indexManager.ExistsAsync(settings.IndexName))
                        {
                            continue;
                        }

                        await _indexSettingsService.ResetAsync(settings);
                        await _indexSettingsService.UpdateAsync(settings);
                        await _indexManager.RebuildIndexAsync(settings);
                        await _indexSettingsService.SynchronizeAsync(settings);

                        await _notifier.SuccessAsync(H["Index <em>{0}</em> rebuilt successfully.", settings.IndexName]);
                    }
                    break;
                default:
                    return BadRequest();
            }
        }

        return RedirectToAction(nameof(Index));
    }

    private ViewResult NotConfigured()
        => View("NotConfigured");
}
