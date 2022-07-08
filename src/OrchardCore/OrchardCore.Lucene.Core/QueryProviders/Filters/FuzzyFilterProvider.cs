using System;
using System.Linq;
using Lucene.Net.Index;
using Lucene.Net.Search;
using Lucene.Net.Util.Automaton;
using Newtonsoft.Json.Linq;

namespace OrchardCore.Lucene.QueryProviders.Filters
{
    public class FuzzyFilterProvider : ILuceneBooleanFilterProvider
    {
        public FilteredQuery CreateFilteredQuery(ILuceneQueryService builder, LuceneQueryContext context, string type, JToken filter, Query toFilter)
        {
            if (type != "fuzzy")
            {
                return null;
            }

            if (!(toFilter is BooleanQuery booleanQuery))
            {
                return null;
            }

            var queryObj = filter as JObject;
            var first = queryObj.Properties().First();

            FuzzyQuery fuzzyQuery;

            switch (first.Value.Type)
            {
                case JTokenType.String:
                    fuzzyQuery = new FuzzyQuery(new Term(first.Name, first.Value.ToString()));
                    break;
                case JTokenType.Object:
                    var obj = (JObject)first.Value;

                    if (!obj.TryGetValue("value", out var value))
                    {
                        throw new ArgumentException("Missing value in fuzzy query");
                    }

                    obj.TryGetValue("fuzziness", out var fuzziness);
                    obj.TryGetValue("prefix_length", out var prefixLength);
                    obj.TryGetValue("max_expansions", out var maxExpansions);

                    fuzzyQuery = new FuzzyQuery(
                        new Term(first.Name, value.Value<string>()),
                        fuzziness?.Value<int>() ?? LevenshteinAutomata.MAXIMUM_SUPPORTED_DISTANCE,
                        prefixLength?.Value<int>() ?? 0,
                        maxExpansions?.Value<int>() ?? 50,
                        true);

                    if (obj.TryGetValue("boost", out var boost))
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
}
