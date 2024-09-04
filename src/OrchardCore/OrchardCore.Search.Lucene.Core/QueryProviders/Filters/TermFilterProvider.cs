using System.Text.Json;
using System.Text.Json.Nodes;
using Lucene.Net.Index;
using Lucene.Net.Search;

namespace OrchardCore.Search.Lucene.QueryProviders.Filters;

public class TermFilterProvider : ILuceneBooleanFilterProvider
{
    public FilteredQuery CreateFilteredQuery(ILuceneQueryService builder, LuceneQueryContext context, string type, JsonNode filter, Query toFilter)
    {
        if (type != "term")
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
        TermQuery termQuery;

        switch (first.Value.GetValueKind())
        {
            case JsonValueKind.String:
                termQuery = new TermQuery(new Term(first.Key, first.Value.ToString()));
                break;
            case JsonValueKind.Object:
                var obj = first.Value.AsObject();
                var value = obj["value"].Value<string>();
                termQuery = new TermQuery(new Term(first.Key, value));

                if (obj.TryGetPropertyValue("boost", out var boost))
                {
                    termQuery.Boost = boost.Value<float>();
                }
                break;
            default: throw new ArgumentException("Invalid term query");
        }

        booleanQuery.Add(termQuery, Occur.MUST);
        var queryFilter = new QueryWrapperFilter(termQuery);

        return new FilteredQuery(booleanQuery, queryFilter);
    }
}
