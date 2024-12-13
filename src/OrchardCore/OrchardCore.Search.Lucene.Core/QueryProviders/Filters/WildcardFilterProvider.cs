using System.Text.Json;
using System.Text.Json.Nodes;
using Lucene.Net.Index;
using Lucene.Net.Search;

namespace OrchardCore.Search.Lucene.QueryProviders.Filters;

public class WildcardFilterProvider : ILuceneBooleanFilterProvider
{
    public FilteredQuery CreateFilteredQuery(ILuceneQueryService builder, LuceneQueryContext context, string type, JsonNode filter, Query toFilter)
    {
        if (type != "wildcard")
        {
            return null;
        }

        if (toFilter is not BooleanQuery booleanQuery)
        {
            return null;
        }

        var queryObj = filter.AsObject();
        var first = queryObj.First();

        // A term query has only one member, which can either be a string or an object
        WildcardQuery wildcardQuery;

        switch (first.Value.GetValueKind())
        {
            case JsonValueKind.String:
                wildcardQuery = new WildcardQuery(new Term(first.Key, first.Value.ToString()));
                break;
            case JsonValueKind.Object:
                var obj = first.Value.AsObject();

                if (!obj.TryGetPropertyValue("value", out var value))
                {
                    throw new ArgumentException("Missing value in wildcard query");
                }

                wildcardQuery = new WildcardQuery(new Term(first.Key, value.Value<string>()));

                if (obj.TryGetPropertyValue("boost", out var boost))
                {
                    wildcardQuery.Boost = boost.Value<float>();
                }

                break;
            default: throw new ArgumentException("Invalid wildcard query");
        }

        booleanQuery.Add(wildcardQuery, Occur.MUST);
        var queryFilter = new QueryWrapperFilter(wildcardQuery);

        return new FilteredQuery(booleanQuery, queryFilter);
    }
}
