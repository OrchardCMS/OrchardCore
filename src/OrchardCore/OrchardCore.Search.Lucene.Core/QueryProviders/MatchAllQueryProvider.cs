using System.Text.Json.Nodes;
using Lucene.Net.Search;

namespace OrchardCore.Search.Lucene.QueryProviders;

public class MatchAllQueryProvider : ILuceneQueryProvider
{
    public Query CreateQuery(ILuceneQueryService builder, LuceneQueryContext context, string type, JsonObject query)
    {
        if (type != "match_all")
        {
            return null;
        }

        var matchAllQuery = new MatchAllDocsQuery();

        if (query.TryGetPropertyValue("boost", out var boost))
        {
            matchAllQuery.Boost = boost.Value<float>();
        }

        return matchAllQuery;
    }
}
