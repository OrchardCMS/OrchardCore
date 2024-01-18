using System;
using System.Linq;
using Lucene.Net.Search;
using Newtonsoft.Json.Linq;

namespace OrchardCore.Search.Lucene.QueryProviders
{
    public class RangeQueryProvider : ILuceneQueryProvider
    {
        public Query CreateQuery(ILuceneQueryService builder, LuceneQueryContext context, string type, JObject query)
        {
            if (type != "range")
            {
                return null;
            }

            var range = query.Properties().First();
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

                    return rangeQuery;
                default: throw new ArgumentException("Invalid range query");
            }
        }
    }
}
