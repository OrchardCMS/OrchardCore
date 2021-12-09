using System.Threading.Tasks;
using Lucene.Net.Search;
using Newtonsoft.Json.Linq;

namespace OrchardCore.Lucene
{
    public interface ILuceneQueryService
    {
        Task<LuceneTopDocs> SearchAsync(LuceneQueryContext context, JObject queryObj);
        Query CreateQueryFragment(LuceneQueryContext context, JObject queryObj);
    }
}
