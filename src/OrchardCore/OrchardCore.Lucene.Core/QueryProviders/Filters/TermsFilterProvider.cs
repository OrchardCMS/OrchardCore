using System;
using System.Linq;
using Lucene.Net.Index;
using Lucene.Net.Search;
using Newtonsoft.Json.Linq;

namespace OrchardCore.Lucene.QueryProviders.Filters
{
    public class TermsFilterProvider : ILuceneBooleanFilterProvider
    {
        public FilteredQuery CreateFilteredQuery(ILuceneQueryService builder, LuceneQueryContext context, string type, JToken filter, Query toFilter)
        {
            if (type != "terms")
            {
                return null;
            }

            if (!(toFilter is BooleanQuery booleanQuery))
            {
                return null;
            }

            var queryObj = filter as JObject;
            var first = queryObj.Properties().First();

            var field = first.Name;
            var boolQuery = new BooleanQuery();

            switch (first.Value.Type)
            {
                case JTokenType.Array:

                    foreach (var item in ((JArray)first.Value))
                    {
                        if (item.Type != JTokenType.String)
                        {
                            throw new ArgumentException($"Invalid term in terms query");
                        }

                        boolQuery.Add(new TermQuery(new Term(field, item.Value<string>())), Occur.SHOULD);
                    }

                    break;
                case JTokenType.Object:
                    throw new ArgumentException("The terms lookup query is not supported");
                default: throw new ArgumentException("Invalid terms query");
            }

            booleanQuery.Add(boolQuery, Occur.MUST);
            var queryFilter = new QueryWrapperFilter(boolQuery);

            return new FilteredQuery(booleanQuery, queryFilter);
        }
    }
}
