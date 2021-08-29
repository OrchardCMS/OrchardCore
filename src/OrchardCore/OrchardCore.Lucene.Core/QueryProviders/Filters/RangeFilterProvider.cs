using System;
using System.Linq;
using Lucene.Net.Index;
using Lucene.Net.Search;
using Newtonsoft.Json.Linq;

namespace OrchardCore.Lucene.QueryProviders.Filters
{
    public class RangeFilterProvider : ILuceneBooleanFilterProvider
    {
        public FilteredQuery CreateFilteredQuery(ILuceneQueryService builder, LuceneQueryContext context, string type, JToken filter, Query toFilter)
        {
            if (type != "range")
            {
                return null;
            }

            if (!(toFilter is BooleanQuery booleanQuery))
            {
                return null;
            }

            var queryObj = filter as JObject;
            var range = queryObj.Properties().First();

            Query rangeQuery;

            switch (range.Value.Type)
            {
                case JTokenType.Object:
                    var field = range.Name;

                    JToken gt = null;
                    JToken lt = null;
                    var tokenType = JTokenType.None;
                    float? boost = null;

                    bool includeLower = false, includeUpper = false;

                    foreach (var element in ((JObject)range.Value).Properties())
                    {
                        switch (element.Name.ToLowerInvariant())
                        {
                            case "gt":
                                gt = element.Value;
                                tokenType = gt.Type;
                                break;
                            case "gte":
                                gt = element.Value;
                                tokenType = gt.Type;
                                includeLower = true;
                                break;
                            case "lt":
                                lt = element.Value;
                                tokenType = lt.Type;
                                break;
                            case "lte":
                                lt = element.Value;
                                tokenType = lt.Type;
                                includeUpper = true;
                                break;
                            case "boost":
                                boost = element.Value.Value<float>();
                                break;
                        }
                    }

                    if (gt != null && lt != null && gt.Type != lt.Type)
                    {
                        throw new ArgumentException("Lower and upper bound range types don't match");
                    }

                    switch (tokenType)
                    {
                        case JTokenType.Integer:
                            var minInt = gt?.Value<long>();
                            var maxInt = lt?.Value<long>();
                            rangeQuery = NumericRangeQuery.NewInt64Range(field, minInt, maxInt, includeLower, includeUpper);
                            break;
                        case JTokenType.Float:
                            var minFloat = gt?.Value<double>();
                            var maxFloat = lt?.Value<double>();
                            rangeQuery = NumericRangeQuery.NewDoubleRange(field, minFloat, maxFloat, includeLower, includeUpper);
                            break;
                        case JTokenType.String:
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
}
