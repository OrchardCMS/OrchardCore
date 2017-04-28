using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using Orchard.Lucene;
using Orchard.Lucene.Services;
using Orchard.Queries.ViewModels;

namespace Orchard.Queries.Controllers
{
    public class AdminController : Controller
    {
        private readonly LuceneIndexProvider _luceneIndexProvider;
        private readonly LuceneIndexingService _luceneIndexingService;
        private readonly LuceneAnalyzerManager _luceneAnalyzerManager;
 
        public AdminController(
            LuceneIndexProvider luceneIndexProvider,
            LuceneIndexingService luceneIndexingService,
            LuceneAnalyzerManager luceneAnalyzerManager)
        {
            _luceneIndexProvider = luceneIndexProvider;
            _luceneIndexingService = luceneIndexingService;
            _luceneAnalyzerManager = luceneAnalyzerManager;
        }

        public IActionResult Index()
        {
            var model = new AdminIndexViewModel
            {
                IndexName = "Search",
                Query = ""
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Index(AdminIndexViewModel model)
        {
            if (!_luceneIndexProvider.Exists(model.IndexName))
            {
                return NotFound();
            }

            if (String.IsNullOrWhiteSpace(model.Query))
            {
                return View(model);
            }

            var luceneSettings = await _luceneIndexingService.GetLuceneSettingsAsync();

            var stopwatch = new Stopwatch();
            stopwatch.Start();

            // TODO: Configure a default analyzer for the query
            var analyzer = _luceneAnalyzerManager.CreateAnalyzer("standardanalyzer");
            var queryDslBuilder = new QueryDslBuilder(analyzer);
            var query = queryDslBuilder.BuildQuery(JObject.Parse(model.Query));

            _luceneIndexProvider.Search(model.IndexName, searcher =>
            {
                var docs = searcher.Search(query, 10);
                model.Documents = docs.ScoreDocs.Select(hit => searcher.Doc(hit.Doc)).ToList();
            });

            model.Duration = stopwatch.Elapsed;

            return View(model);
        }
    }
}
