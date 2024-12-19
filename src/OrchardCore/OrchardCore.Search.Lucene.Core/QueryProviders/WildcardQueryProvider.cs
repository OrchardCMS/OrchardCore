using System.Text.Json;
using System.Text.Json.Nodes;
using Lucene.Net.Index;
using Lucene.Net.Search;

namespace OrchardCore.Search.Lucene.QueryProviders;

public class WildcardQueryProvider : ILuceneQueryProvider
{
    public Query CreateQuery(ILuceneQueryService builder, LuceneQueryContext context, string type, JsonObject query)
    {
        if (type != "wildcard")
        {
            return null;
        }

        var first = query.First();

        switch (first.Value.GetValueKind())
        {
            case JsonValueKind.String:
                return new WildcardQuery(new Term(first.Key, first.Value.ToString()));

            case JsonValueKind.Object:
                var obj = first.Value.AsObject();

                if (!obj.TryGetPropertyValue("value", out var value))
                {
                    throw new ArgumentException("Missing value in wildcard query");
                }

                var wildCardQuery = new WildcardQuery(new Term(first.Key, value.Value<string>()));

                if (obj.TryGetPropertyValue("boost", out var boost))
                {
                    wildCardQuery.Boost = boost.Value<float>();
                }

                return wildCardQuery;

            default: throw new ArgumentException("Invalid wildcard query");
        }
    }
}
