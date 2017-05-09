using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Orchard.Lucene.Services;
using Orchard.Queries;

namespace Orchard.Lucene
{
    public class LuceneQuerySource : IQuerySource
    {
        private readonly LuceneIndexManager _luceneIndexProvider;
        private readonly LuceneIndexingService _luceneIndexingService;
        private readonly LuceneAnalyzerManager _luceneAnalyzerManager;
        private readonly IQueryService _queryService;

        public LuceneQuerySource(
            LuceneIndexManager luceneIndexProvider,
            LuceneIndexingService luceneIndexingService,
            LuceneAnalyzerManager luceneAnalyzerManager,
            IQueryService queryService)
        {
            _luceneIndexProvider = luceneIndexProvider;
            _luceneIndexingService = luceneIndexingService;
            _luceneAnalyzerManager = luceneAnalyzerManager;
            _queryService = queryService;
        }

        public string Name => "Lucene";

        public Query Create()
        {
            return new LuceneQuery();
        }

        public Task<JToken> ExecuteQueryAsync(Query query)
        {
            var luceneQuery = query as LuceneQuery;
            var result = new JArray();

            _luceneIndexProvider.Search(luceneQuery.IndexName, searcher =>
            {
                var analyzer = _luceneAnalyzerManager.CreateAnalyzer("standardanalyzer");
                var context = new LuceneQueryContext(searcher, LuceneSettings.DefaultVersion, analyzer);
                var docs = _queryService.Search(context, luceneQuery.Content);
                foreach (var document in docs.ScoreDocs.Select(hit => searcher.Doc(hit.Doc)))
                {
                    result.Add(new JObject(document.Select(x => new JProperty(x.Name, x.StringValue))));
                }
            });

            return Task.FromResult<JToken>(result);
        }
    }
}
