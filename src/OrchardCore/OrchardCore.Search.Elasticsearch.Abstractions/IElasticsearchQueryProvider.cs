using Nest;
using Newtonsoft.Json.Linq;

namespace OrchardCore.Search.Elasticsearch
{
    public interface IElasticsearchQueryProvider
    {
        IQuery CreateQuery(IElasticsearchQueryService builder, ElasticsearchQueryContext context, string type, JObject query);
    }
}
