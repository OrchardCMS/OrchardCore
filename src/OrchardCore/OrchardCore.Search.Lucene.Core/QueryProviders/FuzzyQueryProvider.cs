using System.Text.Json;
using System.Text.Json.Nodes;
using Lucene.Net.Index;
using Lucene.Net.Search;
using Lucene.Net.Util.Automaton;

namespace OrchardCore.Search.Lucene.QueryProviders;

public class FuzzyQueryProvider : ILuceneQueryProvider
{
    public Query CreateQuery(ILuceneQueryService builder, LuceneQueryContext context, string type, JsonObject query)
    {
        if (type != "fuzzy")
        {
            return null;
        }

        var first = query.First();

        switch (first.Value.GetValueKind())
        {
            case JsonValueKind.String:
                return new FuzzyQuery(new Term(first.Key, first.Value.ToString()));

            case JsonValueKind.Object:
                var obj = first.Value.AsObject();

                if (!obj.TryGetPropertyValue("value", out var value))
                {
                    throw new ArgumentException("Missing value in fuzzy query");
                }

                obj.TryGetPropertyValue("fuzziness", out var fuzziness);
                obj.TryGetPropertyValue("prefix_length", out var prefixLength);
                obj.TryGetPropertyValue("max_expansions", out var maxExpansions);

                var fuzzyQuery = new FuzzyQuery(
                    new Term(first.Key, value.Value<string>()),
                    fuzziness?.Value<int>() ?? LevenshteinAutomata.MAXIMUM_SUPPORTED_DISTANCE,
                    prefixLength?.Value<int>() ?? 0,
                    maxExpansions?.Value<int>() ?? 50,
                    true);

                if (obj.TryGetPropertyValue("boost", out var boost))
                {
                    fuzzyQuery.Boost = boost.Value<float>();
                }

                return fuzzyQuery;
            default: throw new ArgumentException("Invalid fuzzy query");
        }
    }
}
