using Lucene.Net.Search;
using Newtonsoft.Json.Linq;

namespace OrchardCore.Search.Elastic.QueryProviders
{
    /// <summary>
    /// We may not need this at all as Elastic has full blown DSL with NEST OOTB
    /// </summary>
    public class MatchAllQueryProvider : IElasticQueryProvider
    {
        public Query CreateQuery(IElasticQueryService builder, ElasticQueryContext context, string type, JObject query)
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
