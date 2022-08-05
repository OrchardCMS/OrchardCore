using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace OrchardCore.Search.Elasticsearch
{
    public interface IElasticsearchQueryService
    {
        Task<ElasticsearchTopDocs> SearchAsync(ElasticsearchQueryContext context, string query);
    }
}
