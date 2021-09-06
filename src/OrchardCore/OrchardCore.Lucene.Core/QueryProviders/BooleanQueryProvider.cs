using System;
using System.Collections.Generic;
using System.Linq;
using Lucene.Net.Search;
using Newtonsoft.Json.Linq;
using OrchardCore.Lucene.QueryProviders.Filters;

namespace OrchardCore.Lucene.QueryProviders
{
    public class BooleanQueryProvider : ILuceneQueryProvider
    {
        private readonly IEnumerable<ILuceneBooleanFilterProvider> _filters;

        public BooleanQueryProvider(IEnumerable<ILuceneBooleanFilterProvider> filters)
        {
            _filters = filters;
        }

        public Query CreateQuery(ILuceneQueryService builder, LuceneQueryContext context, string type, JObject query)
        {
            if (type != "bool")
            {
                return null;
            }

            var boolQuery = new BooleanQuery();

            foreach (var property in query.Properties())
            {
                var occur = Occur.MUST;
                bool isProps = false;

                switch (property.Name.ToLowerInvariant())
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
                    default: throw new ArgumentException($"Invalid property '{property.Name}' in boolean query");
                }

                if (!isProps)
                {
                    switch (property.Value.Type)
                    {
                        case JTokenType.Object:
                            boolQuery.Add(builder.CreateQueryFragment(context, (JObject)property.Value), occur);
                            break;
                        case JTokenType.Array:
                            foreach (var item in ((JArray)property.Value))
                            {
                                if (item.Type != JTokenType.Object)
                                {
                                    throw new ArgumentException($"Invalid value in boolean query");
                                }
                                boolQuery.Add(builder.CreateQueryFragment(context, (JObject)item), occur);
                            }
                            break;
                        default: throw new ArgumentException($"Invalid value in boolean query");
                    }
                }
            }

            return boolQuery;
        }

        private Query CreateFilteredQuery(ILuceneQueryService builder, LuceneQueryContext context, Query query, JToken filter)
        {
            Query filteredQuery = null;
            var queryObj = filter as JObject;

            switch (filter.Type)
            {
                case JTokenType.Object:
                    var first = queryObj.Properties().First();

                    foreach (var queryProvider in _filters)
                    {
                        filteredQuery = queryProvider.CreateFilteredQuery(builder, context, first.Name, first.Value, query);

                        if (filteredQuery != null)
                        {
                            break;
                        }
                    }
                    break;
                case JTokenType.Array:
                    foreach (var item in ((JArray)filter))
                    {
                        var firstQuery = item.First() as JProperty;

                        foreach (var queryProvider in _filters)
                        {
                            filteredQuery = queryProvider.CreateFilteredQuery(builder, context, firstQuery.Name, firstQuery.Value, query);

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
}
