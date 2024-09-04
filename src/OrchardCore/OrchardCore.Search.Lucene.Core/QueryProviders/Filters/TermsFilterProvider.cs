using System.Text.Json;
using System.Text.Json.Nodes;
using Lucene.Net.Index;
using Lucene.Net.Search;

namespace OrchardCore.Search.Lucene.QueryProviders.Filters;

public class TermsFilterProvider : ILuceneBooleanFilterProvider
{
    public FilteredQuery CreateFilteredQuery(ILuceneQueryService builder, LuceneQueryContext context, string type, JsonNode filter, Query toFilter)
    {
        if (type != "terms")
        {
            return null;
        }

        if (toFilter is not BooleanQuery booleanQuery)
        {
            return null;
        }

        var queryObj = filter.AsObject();
        var first = queryObj.First();

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

        booleanQuery.Add(boolQuery, Occur.MUST);
        var queryFilter = new QueryWrapperFilter(boolQuery);

        return new FilteredQuery(booleanQuery, queryFilter);
    }
}
