using System;
using System.Linq;
using Lucene.Net.Index;
using Lucene.Net.Search;
using Newtonsoft.Json.Linq;

namespace OrchardCore.Lucene.QueryProviders.Filters
{
    public class TermFilterProvider : ILuceneBooleanFilterProvider
    {
        public FilteredQuery CreateFilteredQuery(ILuceneQueryService builder, LuceneQueryContext context, string type, JToken filter, Query toFilter)
        {
            if (type != "term")
            {
                return null;
            }

            if (!(toFilter is BooleanQuery booleanQuery))
            {
                return null;
            }

            var queryObj = filter as JObject;
            var first = queryObj.Properties().First();

            // A term query has only one member, which can either be a string or an object
            TermQuery termQuery;

            switch (first.Value.Type)
            {
                case JTokenType.String:
                    termQuery = new TermQuery(new Term(first.Name, first.Value.ToString()));
                    break;
                case JTokenType.Object:
                    var obj = (JObject)first.Value;
                    var value = obj.Property("value").Value.Value<string>();
                    termQuery = new TermQuery(new Term(first.Name, value));

                    if (obj.TryGetValue("boost", out var boost))
                    {
                        termQuery.Boost = boost.Value<float>();
                    }
                    break;
                default: throw new ArgumentException("Invalid term query");
            }

            booleanQuery.Add(termQuery, Occur.MUST);
            var queryFilter = new QueryWrapperFilter(termQuery);

            return new FilteredQuery(booleanQuery, queryFilter);
        }
    }
}
