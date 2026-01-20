using System.Text.Json;
using System.Text.Json.Nodes;
using Lucene.Net.Index;
using Lucene.Net.Search;

namespace OrchardCore.Search.Lucene.QueryProviders;

public class TermQueryProvider : ILuceneQueryProvider
{
    public Query CreateQuery(ILuceneQueryService builder, LuceneQueryContext context, string type, JsonObject query)
    {
        if (type != "term")
        {
            return null;
        }

        var first = query.First();

        // A term query has only one member, which can either be a string or an object

        switch (first.Value.GetValueKind())
        {
            case JsonValueKind.String:
                return new TermQuery(new Term(first.Key, first.Value.ToString()));

            case JsonValueKind.Object:
                var obj = first.Value.AsObject();
                var value = obj["value"].Value<string>();
                var termQuery = new TermQuery(new Term(first.Key, value));

                if (obj.TryGetPropertyValue("boost", out var boost))
                {
                    termQuery.Boost = boost.Value<float>();
                }

                return termQuery;
            default: throw new ArgumentException("Invalid term query");
        }
    }
}
