using System.Text.Json;
using System.Text.Json.Nodes;
using Lucene.Net.Search;

namespace OrchardCore.Search.Lucene.QueryProviders.Filters;

public class RangeFilterProvider : ILuceneBooleanFilterProvider
{
    public FilteredQuery CreateFilteredQuery(ILuceneQueryService builder, LuceneQueryContext context, string type, JsonNode filter, Query toFilter)
    {
        if (type != "range")
        {
            return null;
        }

        if (toFilter is not BooleanQuery booleanQuery)
        {
            return null;
        }

        var queryObj = filter.AsObject();
        var range = queryObj.First();

        Query rangeQuery;

        switch (range.Value.GetValueKind())
        {
            case JsonValueKind.Object:
                var field = range.Key;

                JsonNode gt = null;
                JsonNode lt = null;
                var nodeKind = JsonValueKind.Null;
                float? boost = null;

                bool includeLower = false, includeUpper = false;

                foreach (var element in range.Value.AsObject())
                {
                    switch (element.Key.ToLowerInvariant())
                    {
                        case "gt":
                            gt = element.Value;
                            nodeKind = gt.GetValueKind();
                            break;
                        case "gte":
                            gt = element.Value;
                            nodeKind = gt.GetValueKind();
                            includeLower = true;
                            break;
                        case "lt":
                            lt = element.Value;
                            nodeKind = lt.GetValueKind();
                            break;
                        case "lte":
                            lt = element.Value;
                            nodeKind = lt.GetValueKind();
                            includeUpper = true;
                            break;
                        case "boost":
                            boost = element.Value.Value<float>();
                            break;
                    }
                }

                if (gt is not null && lt is not null && gt.GetValueKind() != lt.GetValueKind())
                {
                    throw new ArgumentException("Lower and upper bound range types don't match");
                }

                switch (nodeKind)
                {
                    case JsonValueKind.Number:
                        if (gt.AsValue().TryGetValue<long>(out var minInt) &&
                            lt.AsValue().TryGetValue<long>(out var maxInt))
                        {
                            rangeQuery = NumericRangeQuery.NewInt64Range(field, minInt, maxInt, includeLower, includeUpper);
                        }
                        else if (gt.AsValue().TryGetValue<double>(out var minFloat) &&
                            lt.AsValue().TryGetValue<double>(out var maxFloat))
                        {
                            rangeQuery = NumericRangeQuery.NewDoubleRange(field, minFloat, maxFloat, includeLower, includeUpper);
                        }
                        else
                        {
                            throw new ArgumentException($"Unsupported range value type: {type}");
                        }

                        break;

                    case JsonValueKind.String:
                        var minString = gt?.Value<string>();
                        var maxString = lt?.Value<string>();
                        rangeQuery = TermRangeQuery.NewStringRange(field, minString, maxString, includeLower, includeUpper);
                        break;

                    default: throw new ArgumentException($"Unsupported range value type: {type}");
                }

                if (boost != null)
                {
                    rangeQuery.Boost = boost.Value;
                }

                break;
            default: throw new ArgumentException("Invalid range query");
        }

        booleanQuery.Add(rangeQuery, Occur.MUST);
        var queryFilter = new QueryWrapperFilter(rangeQuery);

        return new FilteredQuery(booleanQuery, queryFilter);
    }
}
