using System.Linq;
using Lucene.Net.QueryParsers.Simple;
using Lucene.Net.Search;
using Newtonsoft.Json.Linq;

namespace OrchardCore.Lucene.QueryProviders
{
    public class SimpleQueryStringQueryProvider : ILuceneQueryProvider
    {
        public Query CreateQuery(ILuceneQueryService builder, LuceneQueryContext context, string type, JObject query)
        {
            if (type != "simple_query_string")
            {
                return null;
            }

            var queryString = query["query"]?.Value<string>();
            var fields = query["fields"]?.Values<string>() ?? new string[0];
            var defaultOperator = query["default_operator"]?.Value<string>().ToLowerInvariant() ?? "or";
            var weights = fields.ToDictionary(field => field, field => 1.0f);
            var queryParser = new SimpleQueryParser(context.DefaultAnalyzer, weights);
            switch (defaultOperator)
            {
                case "and":
                    queryParser.DefaultOperator = Occur.MUST;
                    break;
                case "or":
                    queryParser.DefaultOperator = Occur.SHOULD;
                    break;
            }
            return queryParser.Parse(queryString);
        }
    }
}
