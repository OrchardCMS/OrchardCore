using Lucene.Net.Search;
using Newtonsoft.Json.Linq;

namespace Orchard.Lucene
{
    public interface IQueryDslBuilder
    {
        Query CreateQuery(LuceneQueryContext context, JObject query);
    }
}