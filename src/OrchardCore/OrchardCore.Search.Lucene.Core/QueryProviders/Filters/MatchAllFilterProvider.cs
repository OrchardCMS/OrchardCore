using Lucene.Net.Search;
using Newtonsoft.Json.Linq;

namespace OrchardCore.Search.Lucene.QueryProviders.Filters
{
    public class MatchAllFilterProvider : ILuceneBooleanFilterProvider
    {
        public FilteredQuery CreateFilteredQuery(ILuceneQueryService builder, LuceneQueryContext context, string type, JToken filter, Query toFilter)
        {
            if (type != "match_all")
            {
                return null;
            }

            if (toFilter is not BooleanQuery booleanQuery)
            {
                return null;
            }

            var matchAllQuery = new MatchAllDocsQuery();

            booleanQuery.Add(matchAllQuery, Occur.MUST);
            var queryFilter = new QueryWrapperFilter(matchAllQuery);

            return new FilteredQuery(booleanQuery, queryFilter);
        }
    }
}
