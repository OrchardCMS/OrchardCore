using System.Text.Json;
using System.Text.Json.Nodes;
using Lucene.Net.Index;
using Lucene.Net.Search;

namespace OrchardCore.Search.Lucene.QueryProviders;

public class TermsQueryProvider : ILuceneQueryProvider
{
    public Query CreateQuery(ILuceneQueryService builder, LuceneQueryContext context, string type, JsonObject query)
    {
        if (type != "terms")
        {
            return null;
        }

        var first = query.First();

        var field = first.Key;
        var boolQuery = new BooleanQuery();

        switch (first.Value.GetValueKind())
        {
            case JsonValueKind.Array:

                foreach (var item in first.Value.AsArray())
                {
                    if (item.GetValueKind() != JsonValueKind.String)
                    {
                        throw new ArgumentException($"Invalid term in terms query");
                    }

                    boolQuery.Add(new TermQuery(new Term(field, item.Value<string>())), Occur.SHOULD);
                }

                break;
            case JsonValueKind.Object:
                throw new ArgumentException("The terms lookup query is not supported");

            default: throw new ArgumentException("Invalid terms query");
        }

        return boolQuery;
    }
}
