using System.Threading.Tasks;
using Lucene.Net.Search;
using Newtonsoft.Json.Linq;

namespace OrchardCore.Search.Lucene
{
    public interface ILuceneQueryService
    {
        /// <summary>
        /// <para>Provides a way to execute a search request in Lucene based on a JSON object.</para>
        /// <para>OC implementation serializes that JSON object to a Lucene Query based on its <see cref="ILuceneQueryProvider"/> implementations.</para>
        /// </summary>
        /// <param name="context"></param>
        /// <param name="queryObj"></param>
        /// <returns><see cref="LuceneTopDocs"/></returns>
        Task<LuceneTopDocs> SearchAsync(LuceneQueryContext context, JObject queryObj);
        Query CreateQueryFragment(LuceneQueryContext context, JObject queryObj);
    }
}
