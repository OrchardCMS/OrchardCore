using System.Threading.Tasks;
using Lucene.Net.Search;
using Newtonsoft.Json.Linq;

namespace OrchardCore.Search.Elastic
{
    public interface IElasticQueryService
    {
        Task<ElasticTopDocs> SearchAsync(ElasticQueryContext context, JObject queryObj);
        Query CreateQueryFragment(ElasticQueryContext context, JObject queryObj);
    }
}
