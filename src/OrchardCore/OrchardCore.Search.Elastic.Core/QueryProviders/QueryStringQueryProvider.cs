using Lucene.Net.QueryParsers.Classic;
using Lucene.Net.Search;
using Newtonsoft.Json.Linq;

namespace OrchardCore.Search.Elastic.QueryProviders
{
    /// <summary>
    /// We may not need this at all as Elastic has full blown DSL with NEST OOTB
    /// </summary>
    public class QueryStringQueryProvider : IElasticQueryProvider
    {
        public Query CreateQuery(IElasticQueryService builder, ElasticQueryContext context, string type, JObject query)
        {
            if (type != "query_string")
            {
                return null;
            }

            var queryString = query["query"]?.Value<string>();
            var defaultField = query["default_field"]?.Value<string>();

            var queryParser = new QueryParser(context.DefaultVersion, "", context.DefaultAnalyzer);
            return queryParser.Parse(queryString);
        }
    }
}
