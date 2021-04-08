using Lucene.Net.Search;
using Newtonsoft.Json.Linq;

namespace OrchardCore.Search.Elastic
{
    public interface IElasticQueryProvider
    {
        Query CreateQuery(IElasticQueryService builder, ElasticQueryContext context, string type, JObject query);
    }
}
