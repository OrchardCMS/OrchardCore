using System.Text.Json;
using System.Text.Json.Nodes;
using Lucene.Net.Index;
using Lucene.Net.Search;

namespace OrchardCore.Search.Lucene.QueryProviders;

public class PrefixQueryProvider : ILuceneQueryProvider
{
    public Query CreateQuery(ILuceneQueryService builder, LuceneQueryContext context, string type, JsonObject query)
    {
        if (type != "prefix")
        {
            return null;
        }

        var first = query.First();

        // A prefix query has only one member, which can either be a string or an object.

        switch (first.Value.GetValueKind())
        {
            case JsonValueKind.String:
                return new PrefixQuery(new Term(first.Key, first.Value.ToString()));

            case JsonValueKind.Object:
                var obj = first.Value.AsObject();
                PrefixQuery prefixQuery;

                if (obj.TryGetPropertyValue("value", out var value))
                {
                    prefixQuery = new PrefixQuery(new Term(first.Key, value.Value<string>()));
                }
                else if (obj.TryGetPropertyValue("prefix", out var prefix))
                {
                    prefixQuery = new PrefixQuery(new Term(first.Key, prefix.Value<string>()));
                }
                else
                {
                    throw new ArgumentException("Prefix query misses prefix value");
                }

                if (obj.TryGetPropertyValue("boost", out var boost))
                {
                    prefixQuery.Boost = boost.Value<float>();
                }

                return prefixQuery;

            default: throw new ArgumentException("Invalid prefix query");
        }
    }
}
