using System.Text.Json;
using System.Text.Json.Nodes;
using Lucene.Net.Index;
using Lucene.Net.Search;

namespace OrchardCore.Search.Lucene.QueryProviders.Filters;

public class PrefixFilterProvider : ILuceneBooleanFilterProvider
{
    public FilteredQuery CreateFilteredQuery(ILuceneQueryService builder, LuceneQueryContext context, string type, JsonNode filter, Query toFilter)
    {
        if (type != "prefix")
        {
            return null;
        }

        if (toFilter is not BooleanQuery booleanQuery)
        {
            return null;
        }

        var queryObj = filter.AsObject();
        var first = queryObj.First();

        // A prefix query has only one member, which can either be a string or an object
        PrefixQuery prefixQuery;

        switch (first.Value.GetValueKind())
        {
            case JsonValueKind.String:
                prefixQuery = new PrefixQuery(new Term(first.Key, first.Value.ToString()));
                break;
            case JsonValueKind.Object:
                var obj = first.Value.AsObject();

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

                break;
            default: throw new ArgumentException("Invalid prefix query");
        }

        booleanQuery.Add(prefixQuery, Occur.MUST);
        var queryFilter = new QueryWrapperFilter(prefixQuery);

        return new FilteredQuery(booleanQuery, queryFilter);
    }
}
