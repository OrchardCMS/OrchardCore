using System.Threading.Tasks;
using Lucene.Net.Search;
using Newtonsoft.Json.Linq;

namespace Orchard.Lucene
{
    public interface ILuceneQueryService
    {
        Task<TopDocs> SearchAsync(LuceneQueryContext context, JObject queryObj);
        Query CreateQueryFragment(LuceneQueryContext context, JObject queryObj);
    }
}