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
using OrchardCore.Mvc.Utilities;
using OrchardCore.Navigation;
using OrchardCore.Routing;
using OrchardCore.Search.Lucene.Model;
using OrchardCore.Search.Lucene.Services;
using OrchardCore.Search.Lucene.ViewModels;
using YesSql;

namespace OrchardCore.Search.Lucene.Controllers
{
    public class AdminController : Controller
    {
        private readonly ISession _session;
        private readonly LuceneIndexManager _luceneIndexManager;
        private readonly LuceneIndexingService _luceneIndexingService;
        private readonly IAuthorizationService _authorizationService;
        private readonly INotifier _notifier;
        private readonly LuceneAnalyzerManager _luceneAnalyzerManager;
        private readonly LuceneIndexSettingsService _luceneIndexSettingsService;
        private readonly ILuceneQueryService _queryService;
        private readonly ILiquidTemplateManager _liquidTemplateManager;
        private readonly IContentDefinitionManager _contentDefinitionManager;
        private readonly PagerOptions _pagerOptions;
        private readonly JavaScriptEncoder _javaScriptEncoder;
        protected readonly dynamic New;
        protected readonly IStringLocalizer S;
        protected readonly IHtmlLocalizer H;
        private readonly ILogger _logger;
        private readonly IOptions<TemplateOptions> _templateOptions;

        public AdminController(
            ISession session,
            IContentDefinitionManager contentDefinitionManager,
            LuceneIndexManager luceneIndexManager,
            LuceneIndexingService luceneIndexingService,
            IAuthorizationService authorizationService,
            LuceneAnalyzerManager luceneAnalyzerManager,
            LuceneIndexSettingsService luceneIndexSettingsService,
            ILuceneQueryService queryService,
            ILiquidTemplateManager liquidTemplateManager,
            INotifier notifier,
            IOptions<PagerOptions> pagerOptions,
            JavaScriptEncoder javaScriptEncoder,
            IShapeFactory shapeFactory,
            IStringLocalizer<AdminController> stringLocalizer,
            IHtmlLocalizer<AdminController> htmlLocalizer,
            ILogger<AdminController> logger,
            IOptions<TemplateOptions> templateOptions)
        {
            _session = session;
            _luceneIndexManager = luceneIndexManager;
            _luceneIndexingService = luceneIndexingService;
            _authorizationService = authorizationService;
            _luceneAnalyzerManager = luceneAnalyzerManager;
            _luceneIndexSettingsService = luceneIndexSettingsService;
            _queryService = queryService;
            _liquidTemplateManager = liquidTemplateManager;
            _contentDefinitionManager = contentDefinitionManager;
            _notifier = notifier;
            _pagerOptions = pagerOptions.Value;
            _javaScriptEncoder = javaScriptEncoder;

            New = shapeFactory;
            S = stringLocalizer;
            H = htmlLocalizer;
            _logger = logger;
            _templateOptions = templateOptions;
        }

        public async Task<IActionResult> Index(ContentOptions options, PagerParameters pagerParameters)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageLuceneIndexes))
            {
                return Forbid();
            }

            var indexes = (await _luceneIndexSettingsService.GetSettingsAsync()).Select(i => new IndexViewModel { Name = i.IndexName });

            var pager = new Pager(pagerParameters, _pagerOptions.GetPageSize());
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
            return RedirectToAction(nameof(Index), new RouteValueDictionary {
                { "Options.Search", model.Options.Search }
            });
        }

        public async Task<ActionResult> Edit(string indexName = null)
        {
            var IsCreate = String.IsNullOrWhiteSpace(indexName);
            var settings = new LuceneIndexSettings();

            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageLuceneIndexes))
            {
                return Forbid();
            }

            if (!IsCreate)
            {
                settings = await _luceneIndexSettingsService.GetSettingsAsync(indexName);

                if (settings == null)
                {
                    return NotFound();
                }
            }

            var model = new LuceneIndexSettingsViewModel
            {
                IsCreate = IsCreate,
                IndexName = IsCreate ? "" : settings.IndexName,
                AnalyzerName = IsCreate ? "standardanalyzer" : settings.AnalyzerName,
                IndexLatest = settings.IndexLatest,
                Culture = settings.Culture,
                Cultures = ILocalizationService.GetAllCulturesAndAliases()
                    .Select(x => new SelectListItem { Text = x.Name + " (" + x.DisplayName + ")", Value = x.Name }).Prepend(new SelectListItem { Text = S["Any culture"], Value = "any" }),
                Analyzers = _luceneAnalyzerManager.GetAnalyzers()
                    .Select(x => new SelectListItem { Text = x.Name, Value = x.Name }),
                IndexedContentTypes = IsCreate ? _contentDefinitionManager.ListTypeDefinitions()
                    .Select(x => x.Name).ToArray() : settings.IndexedContentTypes,
                StoreSourceData = !IsCreate && settings.StoreSourceData
            };

            return View(model);
        }

        [HttpPost, ActionName("Edit")]
        public async Task<ActionResult> EditPost(LuceneIndexSettingsViewModel model, string[] indexedContentTypes)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageLuceneIndexes))
            {
                return Forbid();
            }

            ValidateModel(model);

            if (model.IsCreate)
            {
                if (_luceneIndexManager.Exists(model.IndexName))
                {
                    ModelState.AddModelError(nameof(LuceneIndexSettingsViewModel.IndexName), S["An index named {0} already exists.", model.IndexName]);
                }
            }
            else
            {
                if (!_luceneIndexManager.Exists(model.IndexName))
                {
                    ModelState.AddModelError(nameof(LuceneIndexSettingsViewModel.IndexName), S["An index named {0} doesn't exist.", model.IndexName]);
                }
            }

            if (!ModelState.IsValid)
            {
                model.Cultures = CultureInfo.GetCultures(CultureTypes.AllCultures)
                    .Select(x => new SelectListItem { Text = x.Name + " (" + x.DisplayName + ")", Value = x.Name }).Prepend(new SelectListItem { Text = S["Any culture"], Value = "any" });
                model.Analyzers = _luceneAnalyzerManager.GetAnalyzers()
                    .Select(x => new SelectListItem { Text = x.Name, Value = x.Name });
                return View(model);
            }

            if (model.IsCreate)
            {
                try
                {
                    var settings = new LuceneIndexSettings { IndexName = model.IndexName, AnalyzerName = model.AnalyzerName, IndexLatest = model.IndexLatest, IndexedContentTypes = indexedContentTypes, Culture = model.Culture ?? "", StoreSourceData = model.StoreSourceData };

                    // We call Rebuild in order to reset the index state cursor too in case the same index
                    // name was also used previously.
                    await _luceneIndexingService.CreateIndexAsync(settings);
                }
                catch (Exception e)
                {
                    await _notifier.ErrorAsync(H["An error occurred while creating the index."]);
                    _logger.LogError(e, "An error occurred while creating an index.");
                    return View(model);
                }

                await _notifier.SuccessAsync(H["Index <em>{0}</em> created successfully.", model.IndexName]);
            }
            else
            {
                try
                {
                    var settings = new LuceneIndexSettings { IndexName = model.IndexName, AnalyzerName = model.AnalyzerName, IndexLatest = model.IndexLatest, IndexedContentTypes = indexedContentTypes, Culture = model.Culture ?? "", StoreSourceData = model.StoreSourceData };

                    await _luceneIndexingService.UpdateIndexAsync(settings);
                }
                catch (Exception e)
                {
                    await _notifier.ErrorAsync(H["An error occurred while editing the index."]);
                    _logger.LogError(e, "An error occurred while editing an index.");
                    return View(model);
                }

                await _notifier.SuccessAsync(H["Index <em>{0}</em> modified successfully, <strong>please consider doing a rebuild on the index.</strong>", model.IndexName]);
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<ActionResult> Reset(string id)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageLuceneIndexes))
            {
                return Forbid();
            }

            var luceneIndexSettings = await _luceneIndexSettingsService.GetSettingsAsync(id);

            if (luceneIndexSettings != null)
            {
                if (!_luceneIndexManager.Exists(id))
                {
                    await _luceneIndexingService.CreateIndexAsync(luceneIndexSettings);
                    await _luceneIndexingService.ProcessContentItemsAsync(id);
                }
                else
                {
                    _luceneIndexingService.ResetIndexAsync(id);
                    await _luceneIndexingService.ProcessContentItemsAsync(id);
                }

                await _notifier.SuccessAsync(H["Index <em>{0}</em> reset successfully.", id]);
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<ActionResult> Rebuild(string id)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageLuceneIndexes))
            {
                return Forbid();
            }

            var luceneIndexSettings = await _luceneIndexSettingsService.GetSettingsAsync(id);

            if (luceneIndexSettings != null)
            {
                await _luceneIndexingService.RebuildIndexAsync(id);
                await _luceneIndexingService.ProcessContentItemsAsync(id);

                await _notifier.SuccessAsync(H["Index <em>{0}</em> rebuilt successfully.", id]);
            }
            else
            {
                return NotFound();
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<ActionResult> Delete(LuceneIndexSettingsViewModel model)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageLuceneIndexes))
            {
                return Forbid();
            }

            var luceneIndexSettings = await _luceneIndexSettingsService.GetSettingsAsync(model.IndexName);

            if (luceneIndexSettings != null)
            {
                try
                {
                    await _luceneIndexingService.DeleteIndexAsync(model.IndexName);

                    await _notifier.SuccessAsync(H["Index <em>{0}</em> deleted successfully.", model.IndexName]);
                }
                catch (Exception e)
                {
                    await _notifier.ErrorAsync(H["An error occurred while deleting the index."]);
                    _logger.LogError(e, "An error occurred while deleting the index '{IndexName}'.", model.IndexName);
                }
            }
            else
            {
                return NotFound();
            }

            return RedirectToAction(nameof(Index));
        }

        public Task<IActionResult> Query(string indexName, string query)
        {
            query = String.IsNullOrWhiteSpace(query) ? "" : System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(query));
            return Query(new AdminQueryViewModel { IndexName = indexName, DecodedQuery = query });
        }

        [HttpPost]
        public async Task<IActionResult> Query(AdminQueryViewModel model)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageLuceneIndexes))
            {
                return Forbid();
            }

            model.Indices = (await _luceneIndexSettingsService.GetSettingsAsync()).Select(x => x.IndexName).ToArray();

            // Can't query if there are no indices
            if (model.Indices.Length == 0)
            {
                return RedirectToAction(nameof(Index));
            }

            if (String.IsNullOrEmpty(model.IndexName))
            {
                model.IndexName = model.Indices[0];
            }

            if (!_luceneIndexManager.Exists(model.IndexName))
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

            await _luceneIndexManager.SearchAsync(model.IndexName, async searcher =>
            {
                var analyzer = _luceneAnalyzerManager.CreateAnalyzer(await _luceneIndexSettingsService.GetIndexAnalyzerAsync(model.IndexName));
                var context = new LuceneQueryContext(searcher, LuceneSettings.DefaultVersion, analyzer);

                var parameters = JsonConvert.DeserializeObject<Dictionary<string, object>>(model.Parameters);

                var tokenizedContent = await _liquidTemplateManager.RenderStringAsync(model.DecodedQuery, _javaScriptEncoder, parameters.Select(x => new KeyValuePair<string, FluidValue>(x.Key, FluidValue.Create(x.Value, _templateOptions.Value))));

                try
                {
                    var parameterizedQuery = JObject.Parse(tokenizedContent);
                    var luceneTopDocs = await _queryService.SearchAsync(context, parameterizedQuery);

                    if (luceneTopDocs != null)
                    {
                        model.Documents = luceneTopDocs.TopDocs.ScoreDocs.Select(hit => searcher.Doc(hit.Doc)).ToList();
                        model.Count = luceneTopDocs.Count;
                    }
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "Error while executing query");
                    ModelState.AddModelError(nameof(model.DecodedQuery), S["Invalid query : {0}", e.Message]);
                }

                stopwatch.Stop();
                model.Elapsed = stopwatch.Elapsed;
            });

            return View(model);
        }

        [HttpPost, ActionName("Index")]
        [FormValueRequired("submit.BulkAction")]
        public async Task<ActionResult> IndexPost(ViewModels.ContentOptions options, IEnumerable<string> itemIds)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageLuceneIndexes))
            {
                return Forbid();
            }

            if (itemIds?.Count() > 0)
            {
                var luceneIndexSettings = await _luceneIndexSettingsService.GetSettingsAsync();
                var checkedContentItems = luceneIndexSettings.Where(x => itemIds.Contains(x.IndexName));
                switch (options.BulkAction)
                {
                    case ContentsBulkAction.None:
                        break;
                    case ContentsBulkAction.Remove:
                        foreach (var item in checkedContentItems)
                        {
                            await _luceneIndexingService.DeleteIndexAsync(item.IndexName);
                        }
                        await _notifier.SuccessAsync(H["Indices successfully removed."]);
                        break;
                    case ContentsBulkAction.Reset:
                        foreach (var item in checkedContentItems)
                        {
                            if (!_luceneIndexManager.Exists(item.IndexName))
                            {
                                return NotFound();
                            }

                            _luceneIndexingService.ResetIndexAsync(item.IndexName);
                            await _luceneIndexingService.ProcessContentItemsAsync(item.IndexName);

                            await _notifier.SuccessAsync(H["Index <em>{0}</em> reset successfully.", item.IndexName]);
                        }
                        break;
                    case ContentsBulkAction.Rebuild:
                        foreach (var item in checkedContentItems)
                        {
                            if (!_luceneIndexManager.Exists(item.IndexName))
                            {
                                return NotFound();
                            }

                            await _luceneIndexingService.RebuildIndexAsync(item.IndexName);
                            await _luceneIndexingService.ProcessContentItemsAsync(item.IndexName);

                            await _notifier.SuccessAsync(H["Index <em>{0}</em> rebuilt successfully.", item.IndexName]);
                        }
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(options.BulkAction), "Invalid bulk action.");
                }
            }

            return RedirectToAction(nameof(Index));
        }

        private void ValidateModel(LuceneIndexSettingsViewModel model)
        {
            if (model.IndexedContentTypes == null || model.IndexedContentTypes.Length < 1)
            {
                ModelState.AddModelError(nameof(LuceneIndexSettingsViewModel.IndexedContentTypes), S["At least one content type selection is required."]);
            }

            if (String.IsNullOrWhiteSpace(model.IndexName))
            {
                ModelState.AddModelError(nameof(LuceneIndexSettingsViewModel.IndexName), S["The index name is required."]);
            }
            else if (model.IndexName.ToSafeName() != model.IndexName)
            {
                ModelState.AddModelError(nameof(LuceneIndexSettingsViewModel.IndexName), S["The index name contains unallowed chars."]);
            }
        }
    }
}
