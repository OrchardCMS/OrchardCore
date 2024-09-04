using System.Text.Json.Nodes;
using Lucene.Net.QueryParsers.Classic;
using Lucene.Net.Search;

namespace OrchardCore.Search.Lucene.QueryProviders;

public class QueryStringQueryProvider : ILuceneQueryProvider
{
    public Query CreateQuery(ILuceneQueryService builder, LuceneQueryContext context, string type, JsonObject query)
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
