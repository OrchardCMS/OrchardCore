using System.Text.Json;
using System.Text.Json.Nodes;
using Lucene.Net.Index;
using Lucene.Net.Search;
using Lucene.Net.Util.Automaton;

namespace OrchardCore.Search.Lucene.QueryProviders.Filters;

public class FuzzyFilterProvider : ILuceneBooleanFilterProvider
{
    public FilteredQuery CreateFilteredQuery(ILuceneQueryService builder, LuceneQueryContext context, string type, JsonNode filter, Query toFilter)
    {
        if (type != "fuzzy")
        {
            return null;
        }

        if (toFilter is not BooleanQuery booleanQuery)
        {
            return null;
        }

        var queryObj = filter.AsObject();
        var first = queryObj.First();

        FuzzyQuery fuzzyQuery;

        switch (first.Value.GetValueKind())
        {
            case JsonValueKind.String:
                fuzzyQuery = new FuzzyQuery(new Term(first.Key, first.Value.ToString()));
                break;
            case JsonValueKind.Object:
                var obj = first.Value.AsObject();

                if (!obj.TryGetPropertyValue("value", out var value))
                {
                    throw new ArgumentException("Missing value in fuzzy query");
                }

                obj.TryGetPropertyValue("fuzziness", out var fuzziness);
                obj.TryGetPropertyValue("prefix_length", out var prefixLength);
                obj.TryGetPropertyValue("max_expansions", out var maxExpansions);

                fuzzyQuery = new FuzzyQuery(
                    new Term(first.Key, value.Value<string>()),
                    fuzziness?.Value<int>() ?? LevenshteinAutomata.MAXIMUM_SUPPORTED_DISTANCE,
                    prefixLength?.Value<int>() ?? 0,
                    maxExpansions?.Value<int>() ?? 50,
                    true);

                if (obj.TryGetPropertyValue("boost", out var boost))
                {
                    fuzzyQuery.Boost = boost.Value<float>();
                }

                break;
            default: throw new ArgumentException("Invalid fuzzy query");
        }

        booleanQuery.Add(fuzzyQuery, Occur.MUST);
        var queryFilter = new QueryWrapperFilter(fuzzyQuery);

        return new FilteredQuery(booleanQuery, queryFilter);
    }
}
