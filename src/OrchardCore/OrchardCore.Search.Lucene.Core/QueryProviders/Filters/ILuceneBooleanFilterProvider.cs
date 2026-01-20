using System.Text.Json.Nodes;
using Lucene.Net.Search;

namespace OrchardCore.Search.Lucene.QueryProviders.Filters;

public interface ILuceneBooleanFilterProvider
{
    FilteredQuery CreateFilteredQuery(ILuceneQueryService builder, LuceneQueryContext context, string type, JsonNode token, Query toFilter);
}
