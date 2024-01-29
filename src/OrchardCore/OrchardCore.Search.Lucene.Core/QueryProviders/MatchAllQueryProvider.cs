using Lucene.Net.Search;
using Newtonsoft.Json.Linq;

namespace OrchardCore.Search.Lucene.QueryProviders
{
    public class MatchAllQueryProvider : ILuceneQueryProvider
    {
        public Query CreateQuery(ILuceneQueryService builder, LuceneQueryContext context, string type, JObject query)
        {
            if (type != "match_all")
            {
                return null;
            }

            var matchAllQuery = new MatchAllDocsQuery();

            if (query.TryGetValue("boost", out var boost))
            {
                matchAllQuery.Boost = boost.Value<float>();
            }

            return matchAllQuery;
        }
    }
}
