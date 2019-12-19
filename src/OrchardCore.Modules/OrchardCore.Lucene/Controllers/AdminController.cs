using System;
using System.Collections.Generic;
using System.Diagnostics;
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
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.Notify;
using OrchardCore.Liquid;
using OrchardCore.Lucene.Model;
using OrchardCore.Lucene.Services;
using OrchardCore.Lucene.ViewModels;
using OrchardCore.Mvc.Utilities;
using OrchardCore.Navigation;
using OrchardCore.Settings;

namespace OrchardCore.Lucene.Controllers
{
    public class AdminController : Controller
    {
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

        public AdminController(
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
            IStringLocalizer<AdminController> s,
            IHtmlLocalizer<AdminController> h,
            ILogger<AdminController> logger)
        {
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
            S = s;
            H = h;
            Logger = logger;
        }

        public ILogger Logger { get; }
        public IStringLocalizer S { get; }
        public IHtmlLocalizer H { get; }

        public async Task<ActionResult> Index(PagerParameters pagerParameters)
        {
            var viewModel = new AdminIndexViewModel
            {
                Indexes = _luceneIndexSettingsService.GetIndices().Select(i => new IndexViewModel { Name = i })
            };

            var siteSettings = await _siteService.GetSiteSettingsAsync();
            var pager = new Pager(pagerParameters, siteSettings.PageSize);
            var count = viewModel.Indexes.Count();
            var results = viewModel.Indexes
                .Skip(pager.GetStartIndex())
                .Take(pager.PageSize).ToList();

            // Maintain previous route data when generating page links
            var routeData = new RouteData();
            var pagerShape = (await New.Pager(pager)).TotalItemCount(count).RouteData(routeData);
            var model = new AdminIndexViewModel
            {
                Indexes = results,
                Pager = pagerShape
            };

            return View(model);
        }

        public async Task<ActionResult> Create()
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageIndexes))
            {
                return Unauthorized();
            }

            var model = new LuceneIndexSettingsViewModel
            {
                IndexName = "",
                AnalyzerName = "standardanalyzer",
                Analyzers = _luceneAnalyzerManager.GetAnalyzers()
                    .Select(x => new SelectListItem { Text = x.Name, Value = x.Name }),
                IndexedContentTypes = _contentDefinitionManager.ListTypeDefinitions()
                    .Select(x => x.Name).ToArray()
            };

            return View(model);
        }

        public async Task<ActionResult> Edit(string indexName)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageIndexes))
            {
                return Unauthorized();
            }

            var settings = _luceneIndexSettingsService.GetSettings(indexName);

            if (settings == null)
            {
                return NotFound();
            }

            var model = new LuceneIndexSettingsViewModel
            {
                IndexName = settings.IndexName,
                AnalyzerName = settings.AnalyzerName,
                IndexLatest = settings.IndexLatest,
                IndexInBackgroundTask = settings.IndexInBackgroundTask,
                Analyzers = _luceneAnalyzerManager.GetAnalyzers()
                    .Select(x => new SelectListItem { Text = x.Name, Value = x.Name }),
                IndexedContentTypes = settings.IndexedContentTypes
            };

            return View(model);
        }

        [HttpPost, ActionName("Edit")]
        public async Task<ActionResult> EditPost(LuceneIndexSettingsViewModel model, string[] indexedContentTypes)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageIndexes))
            {
                return Unauthorized();
            }

            ValidateModel(model);

            if (!_luceneIndexManager.Exists(model.IndexName))
            {
                ModelState.AddModelError(nameof(LuceneIndexSettingsViewModel.IndexName), S["An index named {0} doesn't exists."]);
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var settings = new LuceneIndexSettings { IndexName = model.IndexName, AnalyzerName = model.AnalyzerName, IndexLatest = model.IndexLatest, IndexedContentTypes = indexedContentTypes };

                _luceneIndexingService.UpdateIndex(settings);
            }
            catch (Exception e)
            {
                _notifier.Error(H["An error occurred while creating the index"]);
                Logger.LogError(e, "An error occurred while creating an index");
                return View(model);
            }

            _notifier.Success(H["Index <em>{0}</em> modified successfully, <strong>please consider doing a rebuild on the index.</strong>", model.IndexName]);

            return RedirectToAction("Index");
        }

        [HttpPost, ActionName("Create")]
        public async Task<ActionResult> CreatePOST(LuceneIndexSettingsViewModel model, string[] indexedContentTypes)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageIndexes))
            {
                return Unauthorized();
            }

            ValidateModel(model);

            if (_luceneIndexManager.Exists(model.IndexName))
            {
                ModelState.AddModelError(nameof(LuceneIndexSettingsViewModel.IndexName), S["An index named {0} already exists."]);
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var settings = new LuceneIndexSettings { IndexName = model.IndexName, AnalyzerName = model.AnalyzerName, IndexLatest = model.IndexLatest, IndexedContentTypes = indexedContentTypes };

                // We call Rebuild in order to reset the index state cursor too in case the same index
                // name was also used previously.
                _luceneIndexingService.CreateIndex(settings);
            }
            catch (Exception e)
            {
                _notifier.Error(H["An error occurred while creating the index"]);
                Logger.LogError(e, "An error occurred while creating an index");
                return View(model);
            }

            _notifier.Success(H["Index <em>{0}</em> created successfully.", model.IndexName]);

            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<ActionResult> Reset(string id)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageIndexes))
            {
                return Unauthorized();
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
                return Unauthorized();
            }

            if (!_luceneIndexManager.Exists(id))
            {
                return NotFound();
            }

            _luceneIndexingService.RebuildIndex(id);
            await _luceneIndexingService.ProcessContentItemsAsync(id);

            _notifier.Success(H["Index <em>{0}</em> rebuilt successfully.", id]);

            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<ActionResult> Delete(LuceneIndexSettingsViewModel model)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageIndexes))
            {
                return Unauthorized();
            }

            if (!_luceneIndexManager.Exists(model.IndexName))
            {
                return NotFound();
            }

            try
            {
                _luceneIndexingService.DeleteIndex(model.IndexName);

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
                return Unauthorized();
            }

            model.Indices = _luceneIndexSettingsService.GetIndices().ToArray();

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
                var analyzer = _luceneAnalyzerManager.CreateAnalyzer(_luceneIndexSettingsService.GetIndexAnalyzer(model.IndexName));
                var context = new LuceneQueryContext(searcher, LuceneSettings.DefaultVersion, analyzer);

                var templateContext = new TemplateContext();
                var parameters = JsonConvert.DeserializeObject<Dictionary<string, object>>(model.Parameters);

                foreach (var parameter in parameters)
                {
                    templateContext.SetValue(parameter.Key, parameter.Value);
                }

                var tokenizedContent = await _liquidTemplateManager.RenderAsync(model.DecodedQuery, System.Text.Encodings.Web.JavaScriptEncoder.Default, templateContext);

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
                    ModelState.AddModelError(nameof(model.DecodedQuery), String.Format("Invalid query : {0}", e.Message));
                }

                stopwatch.Stop();
                model.Elapsed = stopwatch.Elapsed;
            });

            return View(model);
        }

        private void ValidateModel(LuceneIndexSettingsViewModel model)
        {
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
