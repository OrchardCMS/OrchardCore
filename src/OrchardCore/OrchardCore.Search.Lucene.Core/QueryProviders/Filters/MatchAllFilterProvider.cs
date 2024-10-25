using System.Text.Json.Nodes;
using Lucene.Net.Search;

namespace OrchardCore.Search.Lucene.QueryProviders.Filters;

public class MatchAllFilterProvider : ILuceneBooleanFilterProvider
{
    public FilteredQuery CreateFilteredQuery(ILuceneQueryService builder, LuceneQueryContext context, string type, JsonNode filter, Query toFilter)
    {
        if (type != "match_all")
        {
            return null;
        }

        if (toFilter is not BooleanQuery booleanQuery)
        {
            return null;
        }

        var matchAllQuery = new MatchAllDocsQuery();

        booleanQuery.Add(matchAllQuery, Occur.MUST);
        var queryFilter = new QueryWrapperFilter(matchAllQuery);

        return new FilteredQuery(booleanQuery, queryFilter);
    }
}
