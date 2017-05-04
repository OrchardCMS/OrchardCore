using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using Orchard.Admin;
using Orchard.Lucene.Services;
using Orchard.Lucene.ViewModels;

namespace Orchard.Lucene.Controllers
{
    [Admin]
    public class QueryController : Controller
    {
        private readonly LuceneIndexManager _luceneIndexProvider;
        private readonly LuceneIndexingService _luceneIndexingService;
        private readonly LuceneAnalyzerManager _luceneAnalyzerManager;
        private readonly IQueryDslBuilder _queryDslBuilder;

        public QueryController(
            LuceneIndexManager luceneIndexProvider,
            LuceneIndexingService luceneIndexingService,
            LuceneAnalyzerManager luceneAnalyzerManager,
            IQueryDslBuilder queryDslBuilder)
        {
            _luceneIndexProvider = luceneIndexProvider;
            _luceneIndexingService = luceneIndexingService;
            _luceneAnalyzerManager = luceneAnalyzerManager;
            _queryDslBuilder = queryDslBuilder;
        }

        public IActionResult Index()
        {
            var model = new QueryIndexViewModel
            {
                IndexName = "Search",
                Query = ""
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Index(QueryIndexViewModel model)
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
            var context = new LuceneQueryContext(LuceneSettings.DefaultVersion, analyzer);
            var query = _queryDslBuilder.CreateQuery(context, JObject.Parse(model.Query));

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
