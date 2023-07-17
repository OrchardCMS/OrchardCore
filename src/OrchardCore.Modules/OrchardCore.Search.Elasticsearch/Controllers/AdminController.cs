using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
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
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
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

namespace OrchardCore.Search.Elasticsearch
{
    public class AdminController : Controller
    {
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
        protected readonly dynamic New;
        protected readonly IStringLocalizer S;
        protected readonly IHtmlLocalizer H;
        private readonly ElasticsearchOptions _elasticSearchOptions;
        private readonly INotifier _notifier;
        private readonly ILogger _logger;
        private readonly IOptions<TemplateOptions> _templateOptions;

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
            IShapeFactory shapeFactory,
            IStringLocalizer<AdminController> stringLocalizer,
            IHtmlLocalizer<AdminController> htmlLocalizer,
            IOptions<ElasticsearchOptions> elasticSearchOptions,
            INotifier notifier,
            ILogger<AdminController> logger,
            IOptions<TemplateOptions> templateOptions
            )
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
            New = shapeFactory;
            S = stringLocalizer;
            H = htmlLocalizer;
            _elasticSearchOptions = elasticSearchOptions.Value;
            _notifier = notifier;
            _logger = logger;
            _templateOptions = templateOptions;
        }

        public async Task<IActionResult> Index(ContentOptions options, PagerParameters pagerParameters)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageElasticIndexes))
            {
                return Forbid();
            }

            var indexes = (await _elasticIndexSettingsService.GetSettingsAsync()).Select(i => new IndexViewModel { Name = i.IndexName });

            var siteSettings = await _siteService.GetSiteSettingsAsync();
            var pager = new Pager(pagerParameters, siteSettings.PageSize);
            var count = indexes.Count();
            var results = indexes;

            if (!String.IsNullOrWhiteSpace(options.Search))
            {
                results = results.Where(q => q.Name.Contains(options.Search, StringComparison.OrdinalIgnoreCase));
            }

            results = results
                .Skip(pager.GetStartIndex())
                .Take(pager.PageSize).ToList();

            // Maintain previous route data when generating page links
            var routeData = new RouteData();
            var pagerShape = (await New.Pager(pager)).TotalItemCount(count).RouteData(routeData);

            var model = new AdminIndexViewModel
            {
                Indexes = results,
                Options = options,
                Pager = pagerShape
            };

            model.Options.ContentsBulkAction = new List<SelectListItem>() {
                new SelectListItem() { Text = S["Reset"], Value = nameof(ContentsBulkAction.Reset) },
                new SelectListItem() { Text = S["Rebuild"], Value = nameof(ContentsBulkAction.Rebuild) },
                new SelectListItem() { Text = S["Delete"], Value = nameof(ContentsBulkAction.Remove) }
            };

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

        public async Task<ActionResult> Edit(string indexName = null)
        {
            var IsCreate = String.IsNullOrWhiteSpace(indexName);
            var settings = new ElasticIndexSettings();

            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageElasticIndexes))
            {
                return Forbid();
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
                IndexName = IsCreate ? "" : settings.IndexName,
                AnalyzerName = IsCreate ? "standardanalyzer" : settings.AnalyzerName,
                IndexLatest = settings.IndexLatest,
                Culture = settings.Culture,
                Cultures = ILocalizationService.GetAllCulturesAndAliases()
                    .Select(x => new SelectListItem { Text = x.Name + " (" + x.DisplayName + ")", Value = x.Name }),
                Analyzers = _elasticSearchOptions.Analyzers
                    .Select(x => new SelectListItem { Text = x.Key, Value = x.Key }),
                IndexedContentTypes = IsCreate ? _contentDefinitionManager.ListTypeDefinitions()
                    .Select(x => x.Name).ToArray() : settings.IndexedContentTypes,
                StoreSourceData = settings.StoreSourceData
            };

            return View(model);
        }

        [HttpPost, ActionName("Edit")]
        public async Task<ActionResult> EditPost(ElasticIndexSettingsViewModel model, string[] indexedContentTypes)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageElasticIndexes))
            {
                return Forbid();
            }

            ValidateModel(model);

            if (model.IsCreate)
            {
                if (await _elasticIndexManager.Exists(model.IndexName))
                {
                    ModelState.AddModelError(nameof(ElasticIndexSettingsViewModel.IndexName), S["An index named {0} already exists.", _elasticIndexManager.GetFullIndexName(model.IndexName)]);
                }
            }
            else
            {
                if (!await _elasticIndexManager.Exists(model.IndexName))
                {
                    ModelState.AddModelError(nameof(ElasticIndexSettingsViewModel.IndexName), S["An index named {0} doesn't exist.", _elasticIndexManager.GetFullIndexName(model.IndexName)]);
                }
            }

            if (!ModelState.IsValid)
            {
                model.Cultures = CultureInfo.GetCultures(CultureTypes.AllCultures)
                    .Select(x => new SelectListItem { Text = x.Name + " (" + x.DisplayName + ")", Value = x.Name });

                model.Analyzers = _elasticSearchOptions.Analyzers
                    .Select(x => new SelectListItem { Text = x.Key, Value = x.Key });

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
                        Culture = model.Culture ?? String.Empty,
                        StoreSourceData = model.StoreSourceData
                    };

                    // We call Rebuild in order to reset the index state cursor too in case the same index
                    // name was also used previously.
                    await _elasticIndexingService.CreateIndexAsync(settings);
                }
                catch (Exception e)
                {
                    await _notifier.ErrorAsync(H["An error occurred while creating the index."]);
                    _logger.LogError(e, "An error occurred while creating an index.");
                    return View(model);
                }

                await _notifier.SuccessAsync(H["Index <em>{0}</em> created successfully.", _elasticIndexManager.GetFullIndexName(model.IndexName)]);
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
                        Culture = model.Culture ?? String.Empty,
                        StoreSourceData = model.StoreSourceData
                    };

                    await _elasticIndexingService.UpdateIndexAsync(settings);
                }
                catch (Exception e)
                {
                    await _notifier.ErrorAsync(H["An error occurred while editing the index."]);
                    _logger.LogError(e, "An error occurred while editing an index.");
                    return View(model);
                }

                await _notifier.SuccessAsync(H["Index <em>{0}</em> modified successfully, <strong>please consider doing a rebuild on the index.</strong>", _elasticIndexManager.GetFullIndexName(model.IndexName)]);
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<ActionResult> Reset(string id)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageElasticIndexes))
            {
                return Forbid();
            }

            if (!await _elasticIndexManager.Exists(id))
            {
                return NotFound();
            }

            await _elasticIndexingService.ResetIndexAsync(id);
            await _elasticIndexingService.ProcessContentItemsAsync(id);

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

            if (!await _elasticIndexManager.Exists(id))
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

            await _elasticIndexingService.ProcessContentItemsAsync(id);

            await _notifier.SuccessAsync(H["Index <em>{0}</em> rebuilt successfully.", id]);

            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<ActionResult> Delete(ElasticIndexSettingsViewModel model)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageElasticIndexes))
            {
                return Forbid();
            }

            if (!await _elasticIndexManager.Exists(model.IndexName))
            {
                await _notifier.SuccessAsync(H["Index not found on Elasticsearch server.", _elasticIndexManager.GetFullIndexName(model.IndexName)]);
                return RedirectToAction("Index");
            }

            try
            {
                await _elasticIndexingService.DeleteIndexAsync(model.IndexName);

                await _notifier.SuccessAsync(H["Index <em>{0}</em> deleted successfully.", _elasticIndexManager.GetFullIndexName(model.IndexName)]);
            }
            catch (Exception e)
            {
                await _notifier.ErrorAsync(H["An error occurred while deleting the index."]);
                _logger.LogError(e, "An error occurred while deleting the index {indexname}", _elasticIndexManager.GetFullIndexName(model.IndexName));
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

            try
            {
                await _elasticIndexingService.DeleteIndexAsync(model.IndexName);

                await _notifier.SuccessAsync(H["Index <em>{0}</em> deleted successfully.", _elasticIndexManager.GetFullIndexName(model.IndexName)]);
            }
            catch (Exception e)
            {
                await _notifier.ErrorAsync(H["An error occurred while deleting the index."]);
                _logger.LogError(e, "An error occurred while deleting the index {indexName}", _elasticIndexManager.GetFullIndexName(model.IndexName));
            }

            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Mappings(string indexName)
        {
            var mappings = await _elasticIndexManager.GetIndexMappings(indexName);
            var formattedJson = JValue.Parse(mappings).ToString(Formatting.Indented);
            return View(new MappingsViewModel
            {
                IndexName = _elasticIndexManager.GetFullIndexName(indexName),
                Mappings = formattedJson
            });
        }

        public async Task<IActionResult> SyncSettings()
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageElasticIndexes))
            {
                return Forbid();
            }

            await _elasticIndexingService.SyncSettings();

            return RedirectToAction("index");
        }

        public Task<IActionResult> Query(string indexName, string query)
        {
            query = String.IsNullOrWhiteSpace(query) ? String.Empty : System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(query));
            return Query(new AdminQueryViewModel
            {
                IndexName = indexName,
                DecodedQuery = query
            });
        }

        [HttpPost]
        public async Task<IActionResult> Query(AdminQueryViewModel model)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageElasticIndexes))
            {
                return Forbid();
            }

            model.Indices = (await _elasticIndexSettingsService.GetSettingsAsync()).Select(x => x.IndexName).ToArray();

            // Can't query if there are no indices
            if (model.Indices.Length == 0)
            {
                return RedirectToAction("Index");
            }

            if (String.IsNullOrEmpty(model.IndexName))
            {
                model.IndexName = model.Indices[0];
            }

            if (!await _elasticIndexManager.Exists(model.IndexName))
            {
                return NotFound();
            }

            if (String.IsNullOrWhiteSpace(model.DecodedQuery))
            {
                return View(model);
            }

            if (String.IsNullOrEmpty(model.Parameters))
            {
                model.Parameters = "{ }";
            }

            var stopwatch = new Stopwatch();
            stopwatch.Start();

            var parameters = JsonConvert.DeserializeObject<Dictionary<string, object>>(model.Parameters);
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

        [HttpPost, ActionName("Index")]
        [FormValueRequired("submit.BulkAction")]
        public async Task<ActionResult> IndexPost(ContentOptions options, IEnumerable<string> itemIds)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageElasticIndexes))
            {
                return Forbid();
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
                            if (!await _elasticIndexManager.Exists(item.IndexName))
                            {
                                return NotFound();
                            }

                            await _elasticIndexingService.ResetIndexAsync(item.IndexName);
                            await _elasticIndexingService.ProcessContentItemsAsync(item.IndexName);

                            await _notifier.SuccessAsync(H["Index <em>{0}</em> reset successfully.", _elasticIndexManager.GetFullIndexName(item.IndexName)]);
                        }
                        break;
                    case ContentsBulkAction.Rebuild:
                        foreach (var item in checkedContentItems)
                        {
                            if (!await _elasticIndexManager.Exists(item.IndexName))
                            {
                                return NotFound();
                            }

                            await _elasticIndexingService.RebuildIndexAsync(await _elasticIndexSettingsService.GetSettingsAsync(item.IndexName));
                            await _elasticIndexingService.ProcessContentItemsAsync(item.IndexName);
                            await _notifier.SuccessAsync(H["Index <em>{0}</em> rebuilt successfully.", _elasticIndexManager.GetFullIndexName(item.IndexName)]);
                        }
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(options.BulkAction), "Unknown bulk action");
                }
            }

            return RedirectToAction("Index");
        }

        private void ValidateModel(ElasticIndexSettingsViewModel model)
        {
            if (model.IndexedContentTypes == null || model.IndexedContentTypes.Length < 1)
            {
                ModelState.AddModelError(nameof(ElasticIndexSettingsViewModel.IndexedContentTypes), S["At least one content type selection is required."]);
            }

            if (String.IsNullOrWhiteSpace(model.IndexName))
            {
                ModelState.AddModelError(nameof(ElasticIndexSettingsViewModel.IndexName), S["The index name is required."]);
            }
            else if (ElasticIndexManager.ToSafeIndexName(model.IndexName) != model.IndexName)
            {
                ModelState.AddModelError(nameof(ElasticIndexSettingsViewModel.IndexName), S["The index name contains unallowed chars."]);
            }
        }
    }
}
