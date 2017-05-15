using Lucene.Net.QueryParsers.Simple;
using Lucene.Net.Search;
using Newtonsoft.Json.Linq;

namespace Orchard.Lucene.QueryProviders
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
            var fields = query["fields"]?.Value<string>() ?? "";
            var defaultOperator = query["default_operator"]?.Value<string>() ?? "and";
            var queryParser = new SimpleQueryParser(context.DefaultAnalyzer, "");
            return queryParser.Parse(queryString);
        }
    }
}
