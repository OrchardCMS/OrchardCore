using Lucene.Net.QueryParsers.Classic;
using Lucene.Net.Search;
using Newtonsoft.Json.Linq;

namespace OrchardCore.Search.Lucene.QueryProviders
{
    public class QueryStringQueryProvider : ILuceneQueryProvider
    {
        public Query CreateQuery(ILuceneQueryService builder, LuceneQueryContext context, string type, JObject query)
        {
            if (type != "query_string")
            {
                return null;
            }

            var queryString = query["query"]?.Value<string>();
            var defaultField = query["default_field"]?.Value<string>() ?? "";

            var queryParser = new QueryParser(context.DefaultVersion, defaultField, context.DefaultAnalyzer);
            return queryParser.Parse(queryString);
        }
    }
}
