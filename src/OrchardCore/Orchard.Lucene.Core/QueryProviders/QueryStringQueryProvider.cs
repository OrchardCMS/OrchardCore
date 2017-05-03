using Lucene.Net.QueryParsers.Classic;
using Lucene.Net.Search;
using Newtonsoft.Json.Linq;

namespace Orchard.Lucene.QueryProviders
{
    public class QueryStringQueryProvider : ILuceneQueryProvider
    {
        public Query CreateQuery(IQueryDslBuilder builder, LuceneQueryContext context, string type, JObject query)
        {
            if (type != "query_string")
            {
                return null;
            }

            var queryString = query["query"]?.Value<string>();
            var queryParser = new QueryParser(context.DefaultVersion, "", context.DefaultAnalyzer);
            return queryParser.Parse(queryString);
        }
    }
}
