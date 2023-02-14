using System;
using System.Linq;
using Lucene.Net.Index;
using Lucene.Net.Search;
using Newtonsoft.Json.Linq;

namespace OrchardCore.Search.Lucene.QueryProviders.Filters
{
    public class WildcardFilterProvider : ILuceneBooleanFilterProvider
    {
        public FilteredQuery CreateFilteredQuery(ILuceneQueryService builder, LuceneQueryContext context, string type, JToken filter, Query toFilter)
        {
            if (type != "wildcard")
            {
                return null;
            }

            if (toFilter is not BooleanQuery booleanQuery)
            {
                return null;
            }

            var queryObj = filter as JObject;
            var first = queryObj.Properties().First();

            // A term query has only one member, which can either be a string or an object
            WildcardQuery wildcardQuery;

            switch (first.Value.Type)
            {
                case JTokenType.String:
                    wildcardQuery = new WildcardQuery(new Term(first.Name, first.Value.ToString()));
                    break;
                case JTokenType.Object:
                    var obj = (JObject)first.Value;

                    if (!obj.TryGetValue("value", out var value))
                    {
                        throw new ArgumentException("Missing value in wildcard query");
                    }

                    wildcardQuery = new WildcardQuery(new Term(first.Name, value.Value<string>()));

                    if (obj.TryGetValue("boost", out var boost))
                    {
                        wildcardQuery.Boost = boost.Value<float>();
                    }

                    break;
                default: throw new ArgumentException("Invalid wildcard query");
            }

            booleanQuery.Add(wildcardQuery, Occur.MUST);
            var queryFilter = new QueryWrapperFilter(wildcardQuery);

            return new FilteredQuery(booleanQuery, queryFilter);
        }
    }
}
