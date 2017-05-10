using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Orchard.Lucene.Services;
using Orchard.Queries;
using Orchard.Tokens.Services;

namespace Orchard.Lucene
{
    public class LuceneQuerySource : IQuerySource
    {
        private readonly LuceneIndexManager _luceneIndexProvider;
        private readonly LuceneIndexingService _luceneIndexingService;
        private readonly LuceneAnalyzerManager _luceneAnalyzerManager;
        private readonly ILuceneQueryService _queryService;
        private readonly ITokenizer _tokenizer;

        public LuceneQuerySource(
            LuceneIndexManager luceneIndexProvider,
            LuceneIndexingService luceneIndexingService,
            LuceneAnalyzerManager luceneAnalyzerManager,
            ILuceneQueryService queryService,
            ITokenizer tokenizer)
        {
            _luceneIndexProvider = luceneIndexProvider;
            _luceneIndexingService = luceneIndexingService;
            _luceneAnalyzerManager = luceneAnalyzerManager;
            _queryService = queryService;
            _tokenizer = tokenizer;
        }

        public string Name => "Lucene";

        public Query Create()
        {
            return new LuceneQuery();
        }

        public Task<JToken> ExecuteQueryAsync(Query query, IDictionary<string, object> parameters)
        {
            var luceneQuery = query as LuceneQuery;
            var result = new JArray();

            _luceneIndexProvider.Search(luceneQuery.Index, searcher =>
            {
                var tokenizedContent = _tokenizer.Tokenize(luceneQuery.Template, parameters);
                var parameterizedQuery = JObject.Parse(tokenizedContent);

                var analyzer = _luceneAnalyzerManager.CreateAnalyzer("standardanalyzer");
                var context = new LuceneQueryContext(searcher, LuceneSettings.DefaultVersion, analyzer);
                var docs = _queryService.Search(context, parameterizedQuery);
                foreach (var document in docs.ScoreDocs.Select(hit => searcher.Doc(hit.Doc)))
                {
                    result.Add(new JObject(document.Select(x => new JProperty(x.Name, x.StringValue))));
                }
            });

            return Task.FromResult<JToken>(result);
        }
    }
}
