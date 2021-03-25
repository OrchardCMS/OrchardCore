using Lucene.Net.Search;
using Newtonsoft.Json.Linq;

namespace OrchardCore.Lucene.QueryProviders.Filters
{
    public interface ILuceneBooleanFilterProvider
    {
        FilteredQuery CreateFilteredQuery(ILuceneQueryService builder, LuceneQueryContext context, string type, JObject queryObj, Query toFilter);
    }
}
