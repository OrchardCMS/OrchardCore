using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using Orchard.DisplayManagement.Notify;
using Orchard.Lucene.Services;
using Orchard.Lucene.ViewModels;
using Orchard.Mvc.Utilities;
using Orchard.Tokens.Services;

namespace Orchard.Lucene.Controllers
{
    public class AdminController : Controller
    {
        private readonly LuceneIndexManager _luceneIndexManager;
        private readonly LuceneIndexingService _luceneIndexingService;
        private readonly IAuthorizationService _authorizationService;
        private readonly INotifier _notifier;
        private readonly LuceneAnalyzerManager _luceneAnalyzerManager;
        private readonly ILuceneQueryService _queryService;

        public AdminController(
            LuceneIndexManager luceneIndexManager,
            LuceneIndexingService luceneIndexingService,
            IAuthorizationService authorizationService,
            LuceneAnalyzerManager luceneAnalyzerManager,
            ILuceneQueryService queryService,
            INotifier notifier,
            IStringLocalizer<AdminController> s,
            IHtmlLocalizer<AdminController> h,
            ILogger<AdminController> logger)
        {
            _luceneIndexManager = luceneIndexManager;
            _luceneIndexingService = luceneIndexingService;
            _authorizationService = authorizationService;
            _luceneAnalyzerManager = luceneAnalyzerManager;
            _queryService = queryService;

            _notifier = notifier;
            S = s;
            H = h;
            Logger = logger;
        }

        public ILogger Logger { get; }
        public IStringLocalizer S { get; }
        public IHtmlLocalizer H { get; }

        public ActionResult Index()
        {
            var viewModel = new AdminIndexViewModel();

            viewModel.Indexes = _luceneIndexManager.List().Select(s => new IndexViewModel { Name = s }).ToArray();

            return View(viewModel);
        }

        public async Task<ActionResult> Create()
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageIndexes))
            {
                return Unauthorized();
            }

            var model = new AdminEditViewModel
            {
                IndexName = "",
            };

            return View(model);
        }

        [HttpPost, ActionName("Create")]
        public async Task<ActionResult> CreatePOST(AdminEditViewModel model)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageIndexes))
            {
                return Unauthorized();
            }

            ValidateModel(model);

            if (_luceneIndexManager.Exists(model.IndexName))
            {
                ModelState.AddModelError(nameof(AdminEditViewModel.IndexName), S["An index named {0} already exists."]);
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                // We call Rebuild in order to reset the index state cursor too in case the same index
                // name was also used previously.
                _luceneIndexingService.RebuildIndex(model.IndexName);
                await _luceneIndexingService.ProcessContentItemsAsync();
            }
            catch (Exception e)
            {
                _notifier.Error(H["An error occurred while creating the index"]);
                Logger.LogError("An error occurred while creating an index", e);
                return View(model);
            }

            _notifier.Success(H["Index <em>{0}</em> created successfully", model.IndexName]);

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
            await _luceneIndexingService.ProcessContentItemsAsync();

            _notifier.Success(H["Index <em>{0}</em> resetted successfully", id]);

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
            await _luceneIndexingService.ProcessContentItemsAsync();

            _notifier.Success(H["Index <em>{0}</em> rebuilt successfully", id]);

            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<ActionResult> Delete(AdminEditViewModel model)
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
                _luceneIndexManager.DeleteIndex(model.IndexName);

                _notifier.Success(H["Index <em>{0}</em> deleted successfully", model.IndexName]);
            }
            catch(Exception e)
            {
                _notifier.Error(H["An error occurred while deleting the index"]);
                Logger.LogError("An error occurred while deleting the index " + model.IndexName, e);
            }

            return RedirectToAction("Index");
        }

        public Task<IActionResult> Query(string indexName, string query, [FromServices] ITokenizer tokenizer)
        {
            query = String.IsNullOrWhiteSpace(query) ? "" : System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(query));
            return Query(new AdminQueryViewModel { IndexName = indexName, DecodedQuery = query }, tokenizer);
        }

        [HttpPost]
        public async Task<IActionResult> Query(AdminQueryViewModel model, [FromServices] ITokenizer tokenizer)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageIndexes))
            {
                return Unauthorized();
            }

            model.Indices = _luceneIndexManager.List().ToArray();

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

            var luceneSettings = await _luceneIndexingService.GetLuceneSettingsAsync();

            var stopwatch = new Stopwatch();
            stopwatch.Start();

            await _luceneIndexManager.SearchAsync(model.IndexName, async searcher =>
            {
                var analyzer = _luceneAnalyzerManager.CreateAnalyzer("standardanalyzer");
                var context = new LuceneQueryContext(searcher, LuceneSettings.DefaultVersion, analyzer);

                var tokenizedContent = tokenizer.Tokenize(model.DecodedQuery, JObject.Parse(model.Parameters));

                try
                {
                    var parameterizedQuery = JObject.Parse(tokenizedContent);
                    var docs = await _queryService.SearchAsync(context, parameterizedQuery);
                    model.Documents = docs.ScoreDocs.Select(hit => searcher.Doc(hit.Doc)).ToList();
                }
                catch(Exception e)
                {
                    Logger.LogError("Error while executing query: {0}", e.Message);
                    ModelState.AddModelError(nameof(model.DecodedQuery), "Invalid query");
                }

                stopwatch.Stop();
                model.Elapsed = stopwatch.Elapsed;
            });            

            return View(model);
        }

        private void ValidateModel(AdminEditViewModel model)
        {
            if (String.IsNullOrWhiteSpace(model.IndexName))
            {
                ModelState.AddModelError(nameof(AdminEditViewModel.IndexName), S["The index name is required."]);
            }
            else if (model.IndexName.ToSafeName() != model.IndexName)
            {
                ModelState.AddModelError(nameof(AdminEditViewModel.IndexName), S["The index name contains unallowed chars."]);
            }
        }
    }
}
