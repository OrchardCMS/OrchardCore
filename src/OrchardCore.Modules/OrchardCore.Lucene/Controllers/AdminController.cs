using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Fluid;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.Notify;
using OrchardCore.Liquid;
using OrchardCore.Lucene.Model;
using OrchardCore.Lucene.Services;
using OrchardCore.Lucene.ViewModels;
using OrchardCore.Mvc.Utilities;
using OrchardCore.Navigation;
using OrchardCore.Routing;
using OrchardCore.Settings;
using YesSql;

namespace OrchardCore.Lucene.Controllers
{
    public class AdminController : Controller
    {
        private readonly ISession _session;
        private readonly IContentManager _contentManager;
        private readonly LuceneIndexManager _luceneIndexManager;
        private readonly LuceneIndexingService _luceneIndexingService;
        private readonly IAuthorizationService _authorizationService;
        private readonly INotifier _notifier;
        private readonly LuceneAnalyzerManager _luceneAnalyzerManager;
        private readonly LuceneIndexSettingsService _luceneIndexSettingsService;
        private readonly ILuceneQueryService _queryService;
        private readonly ILiquidTemplateManager _liquidTemplateManager;
        private readonly IContentDefinitionManager _contentDefinitionManager;
        private readonly ISiteService _siteService;
        private readonly dynamic New;
        private readonly IStringLocalizer<AdminController> S;
        private readonly IHtmlLocalizer<AdminController> H;

        public AdminController(
            ISession session,
            IContentManager contentManager,
            IContentDefinitionManager contentDefinitionManager,
            LuceneIndexManager luceneIndexManager,
            LuceneIndexingService luceneIndexingService,
            IAuthorizationService authorizationService,
            LuceneAnalyzerManager luceneAnalyzerManager,
            LuceneIndexSettingsService luceneIndexSettingsService,
            ILuceneQueryService queryService,
            ILiquidTemplateManager liquidTemplateManager,
            INotifier notifier,
            ISiteService siteService,
            IShapeFactory shapeFactory,
            IStringLocalizer<AdminController> stringLocalizer,
            IHtmlLocalizer<AdminController> htmlLocalizer,
            ILogger<AdminController> logger)
        {
            _session = session;
            _contentManager = contentManager;
            _luceneIndexManager = luceneIndexManager;
            _luceneIndexingService = luceneIndexingService;
            _authorizationService = authorizationService;
            _luceneAnalyzerManager = luceneAnalyzerManager;
            _luceneIndexSettingsService = luceneIndexSettingsService;
            _queryService = queryService;
            _liquidTemplateManager = liquidTemplateManager;
            _contentDefinitionManager = contentDefinitionManager;
            _notifier = notifier;
            _siteService = siteService;

            New = shapeFactory;
            S = stringLocalizer;
            H = htmlLocalizer;
            Logger = logger;
        }

        public ILogger Logger { get; }

        public async Task<ActionResult> Index(AdminIndexViewModel model, PagerParameters pagerParameters)
        {
            model.Indexes = (await _luceneIndexSettingsService.GetSettingsAsync()).Select(i => new IndexViewModel { Name = i.IndexName });

            var siteSettings = await _siteService.GetSiteSettingsAsync();
            var pager = new Pager(pagerParameters, siteSettings.PageSize);
            var count = model.Indexes.Count();
            var results = model.Indexes
                .Skip(pager.GetStartIndex())
                .Take(pager.PageSize).ToList();

            // Maintain previous route data when generating page links
            var routeData = new RouteData();
            var pagerShape = (await New.Pager(pager)).TotalItemCount(count).RouteData(routeData);

            model.Indexes = results;
            model.Pager = pagerShape;

            model.Options.ContentsBulkAction = new List<SelectListItem>() {
                new SelectListItem() { Text = S["Reset"], Value = nameof(ContentsBulkAction.Reset) },
                new SelectListItem() { Text = S["Rebuild"], Value = nameof(ContentsBulkAction.Rebuild) },
                new SelectListItem() { Text = S["Delete"], Value = nameof(ContentsBulkAction.Remove) }
            };

            return View(model);
        }

        public async Task<ActionResult> Edit(string indexName = null)
        {
            var IsCreate = String.IsNullOrWhiteSpace(indexName);
            var settings = new LuceneIndexSettings();

            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageIndexes))
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
                Cultures = CultureInfo.GetCultures(CultureTypes.AllCultures)
                    .Select(x => new SelectListItem { Text = x.Name + " (" + x.DisplayName + ")", Value = x.Name }).Prepend(new SelectListItem { Text = S["Any culture"], Value = "any" }),
                Analyzers = _luceneAnalyzerManager.GetAnalyzers()
                    .Select(x => new SelectListItem { Text = x.Name, Value = x.Name }),
                IndexedContentTypes = IsCreate ? _contentDefinitionManager.ListTypeDefinitions()
                    .Select(x => x.Name).ToArray() : settings.IndexedContentTypes
            };

            return View(model);
        }

        [HttpPost, ActionName("Edit")]
        public async Task<ActionResult> EditPost(LuceneIndexSettingsViewModel model, string[] indexedContentTypes)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageIndexes))
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
                    var settings = new LuceneIndexSettings { IndexName = model.IndexName, AnalyzerName = model.AnalyzerName, IndexLatest = model.IndexLatest, IndexedContentTypes = indexedContentTypes, Culture = model.Culture ?? "" };

                    // We call Rebuild in order to reset the index state cursor too in case the same index
                    // name was also used previously.
                    await _luceneIndexingService.CreateIndexAsync(settings);
                }
                catch (Exception e)
                {
                    _notifier.Error(H["An error occurred while creating the index"]);
                    Logger.LogError(e, "An error occurred while creating an index");
                    return View(model);
                }

                _notifier.Success(H["Index <em>{0}</em> created successfully.", model.IndexName]);
            }
            else
            {
                try
                {
                    var settings = new LuceneIndexSettings { IndexName = model.IndexName, AnalyzerName = model.AnalyzerName, IndexLatest = model.IndexLatest, IndexedContentTypes = indexedContentTypes, Culture = model.Culture ?? "" };

                    await _luceneIndexingService.UpdateIndexAsync(settings);
                }
                catch (Exception e)
                {
                    _notifier.Error(H["An error occurred while editing the index"]);
                    Logger.LogError(e, "An error occurred while editing an index");
                    return View(model);
                }

                _notifier.Success(H["Index <em>{0}</em> modified successfully, <strong>please consider doing a rebuild on the index.</strong>", model.IndexName]);
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<ActionResult> Reset(string id)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageIndexes))
            {
                return Forbid();
            }

            if (!_luceneIndexManager.Exists(id))
            {
                return NotFound();
            }

            _luceneIndexingService.ResetIndex(id);
            await _luceneIndexingService.ProcessContentItemsAsync(id);

            _notifier.Success(H["Index <em>{0}</em> reset successfully.", id]);

            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<ActionResult> Rebuild(string id)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageIndexes))
            {
                return Forbid();
            }

            if (!_luceneIndexManager.Exists(id))
            {
                return NotFound();
            }

            await _luceneIndexingService.RebuildIndexAsync(id);
            await _luceneIndexingService.ProcessContentItemsAsync(id);

            _notifier.Success(H["Index <em>{0}</em> rebuilt successfully.", id]);

            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<ActionResult> Delete(LuceneIndexSettingsViewModel model)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageIndexes))
            {
                return Forbid();
            }

            if (!_luceneIndexManager.Exists(model.IndexName))
            {
                return NotFound();
            }

            try
            {
                await _luceneIndexingService.DeleteIndexAsync(model.IndexName);

                _notifier.Success(H["Index <em>{0}</em> deleted successfully.", model.IndexName]);
            }
            catch (Exception e)
            {
                _notifier.Error(H["An error occurred while deleting the index."]);
                Logger.LogError("An error occurred while deleting the index " + model.IndexName, e);
            }

            return RedirectToAction("Index");
        }

        public Task<IActionResult> Query(string indexName, string query)
        {
            query = String.IsNullOrWhiteSpace(query) ? "" : System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(query));
            return Query(new AdminQueryViewModel { IndexName = indexName, DecodedQuery = query });
        }

        [HttpPost]
        public async Task<IActionResult> Query(AdminQueryViewModel model)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageIndexes))
            {
                return Forbid();
            }

            model.Indices = (await _luceneIndexSettingsService.GetSettingsAsync()).Select(x => x.IndexName).ToArray();

            // Can't query if there are no indices
            if (model.Indices.Length == 0)
            {
                return RedirectToAction("Index");
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

                var templateContext = _liquidTemplateManager.Context;
                var parameters = JsonConvert.DeserializeObject<Dictionary<string, object>>(model.Parameters);

                foreach (var parameter in parameters)
                {
                    templateContext.SetValue(parameter.Key, parameter.Value);
                }

                var tokenizedContent = await _liquidTemplateManager.RenderAsync(model.DecodedQuery, System.Text.Encodings.Web.JavaScriptEncoder.Default);

                try
                {
                    var parameterizedQuery = JObject.Parse(tokenizedContent);
                    var docs = await _queryService.SearchAsync(context, parameterizedQuery);
                    model.Documents = docs.TopDocs.ScoreDocs.Select(hit => searcher.Doc(hit.Doc)).ToList();
                    model.Count = docs.Count;
                }
                catch (Exception e)
                {
                    Logger.LogError(e, "Error while executing query");
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
                            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageIndexes, item))
                            {
                                _notifier.Warning(H["Couldn't remove selected index."]);
                                _session.Cancel();
                                return Forbid();
                            }

                            await _luceneIndexingService.DeleteIndexAsync(item.IndexName);
                        }
                        _notifier.Success(H["Index successfully removed."]);
                        break;
                    case ContentsBulkAction.Reset:
                        foreach (var item in checkedContentItems)
                        {
                            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageIndexes))
                            {
                                return Forbid();
                            }

                            if (!_luceneIndexManager.Exists(item.IndexName))
                            {
                                return NotFound();
                            }

                            _luceneIndexingService.ResetIndex(item.IndexName);
                            await _luceneIndexingService.ProcessContentItemsAsync(item.IndexName);

                            _notifier.Success(H["Index <em>{0}</em> reset successfully.", item.IndexName]);
                        }
                        break;
                    case ContentsBulkAction.Rebuild:
                        foreach (var item in checkedContentItems)
                        {
                            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageIndexes))
                            {
                                return Forbid();
                            }

                            if (!_luceneIndexManager.Exists(item.IndexName))
                            {
                                return NotFound();
                            }

                            await _luceneIndexingService.RebuildIndexAsync(item.IndexName);
                            await _luceneIndexingService.ProcessContentItemsAsync(item.IndexName);

                            _notifier.Success(H["Index <em>{0}</em> rebuilt successfully.", item.IndexName]);
                        }
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            return RedirectToAction("Index");
        }

        private void ValidateModel(LuceneIndexSettingsViewModel model)
        {
            if (model.IndexedContentTypes == null || model.IndexedContentTypes.Count() < 1)
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
