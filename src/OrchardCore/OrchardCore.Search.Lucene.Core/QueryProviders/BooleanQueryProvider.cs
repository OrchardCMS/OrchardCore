using System.Text.Json;
using System.Text.Json.Nodes;
using Lucene.Net.Search;
using OrchardCore.Search.Lucene.QueryProviders.Filters;

namespace OrchardCore.Search.Lucene.QueryProviders;

public class BooleanQueryProvider : ILuceneQueryProvider
{
    private readonly IEnumerable<ILuceneBooleanFilterProvider> _filters;

    public BooleanQueryProvider(IEnumerable<ILuceneBooleanFilterProvider> filters)
    {
        _filters = filters;
    }

    public Query CreateQuery(ILuceneQueryService builder, LuceneQueryContext context, string type, JsonObject query)
    {
        if (type != "bool")
        {
            return null;
        }

        var boolQuery = new BooleanQuery();

        foreach (var property in query)
        {
            var occur = Occur.MUST;
            var isProps = false;

            switch (property.Key.ToLowerInvariant())
            {
                case "must":
                    occur = Occur.MUST;
                    break;
                case "mustnot":
                case "must_not":
                    occur = Occur.MUST_NOT;
                    break;
                case "should":
                    occur = Occur.SHOULD;
                    break;
                case "boost":
                    boolQuery.Boost = property.Value.Value<float>();
                    isProps = true;
                    break;
                case "minimum_should_match":
                    boolQuery.MinimumNumberShouldMatch = property.Value.Value<int>();
                    isProps = true;
                    break;
                case "filter":
                    return CreateFilteredQuery(builder, context, boolQuery, property.Value);
                default: throw new ArgumentException($"Invalid property '{property.Key}' in boolean query");
            }

            if (!isProps)
            {
                switch (property.Value.GetValueKind())
                {
                    case JsonValueKind.Object:
                        boolQuery.Add(builder.CreateQueryFragment(context, property.Value.AsObject()), occur);
                        break;
                    case JsonValueKind.Array:
                        foreach (var item in property.Value.AsArray())
                        {
                            if (item.GetValueKind() != JsonValueKind.Object)
                            {
                                throw new ArgumentException($"Invalid value in boolean query");
                            }
                            boolQuery.Add(builder.CreateQueryFragment(context, item.AsObject()), occur);
                        }
                        break;
                    default: throw new ArgumentException($"Invalid value in boolean query");
                }
            }
        }

        return boolQuery;
    }

    private Query CreateFilteredQuery(ILuceneQueryService builder, LuceneQueryContext context, Query query, JsonNode filter)
    {
        Query filteredQuery = null;
        var queryObj = filter.AsObject();

        switch (filter.GetValueKind())
        {
            case JsonValueKind.Object:
                var first = queryObj.First();

                foreach (var queryProvider in _filters)
                {
                    filteredQuery = queryProvider.CreateFilteredQuery(builder, context, first.Key, first.Value, query);

                    if (filteredQuery != null)
                    {
                        break;
                    }
                }
                break;
            case JsonValueKind.Array:
                foreach (var item in filter.AsArray())
                {
                    var firstQuery = item.AsObject().First();

                    foreach (var queryProvider in _filters)
                    {
                        filteredQuery = queryProvider.CreateFilteredQuery(builder, context, firstQuery.Key, firstQuery.Value, query);

                        if (filteredQuery != null)
                        {
                            break;
                        }
                    }
                }
                break;
            default: throw new ArgumentException($"Invalid value in boolean query");
        }

        return filteredQuery;
    }
}
