using System.Text.Json;
using System.Text.Json.Nodes;
using Lucene.Net.Index;
using Lucene.Net.Search;

namespace OrchardCore.Search.Lucene.QueryProviders;

public class RegexpQueryProvider : ILuceneQueryProvider
{
    public Query CreateQuery(ILuceneQueryService builder, LuceneQueryContext context, string type, JsonObject query)
    {
        if (type != "regexp")
        {
            return null;
        }

        var first = query.First();

        switch (first.Value.GetValueKind())
        {
            case JsonValueKind.String:
                return new RegexpQuery(new Term(first.Key, first.Value.ToString()));

            case JsonValueKind.Object:
                var obj = first.Value.AsObject();

                if (!obj.TryGetPropertyValue("value", out var value))
                {
                    throw new ArgumentException("Missing value in regexp query");
                }

                // TODO: Support flags

                var regexpQuery = new RegexpQuery(new Term(first.Key, value.Value<string>()));

                if (obj.TryGetPropertyValue("boost", out var boost))
                {
                    regexpQuery.Boost = boost.Value<float>();
                }

                return regexpQuery;
            default: throw new ArgumentException("Invalid regexp query");
        }
    }
}
