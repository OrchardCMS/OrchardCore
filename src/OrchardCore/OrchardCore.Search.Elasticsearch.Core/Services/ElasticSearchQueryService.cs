using Nest;
using OrchardCore.ContentManagement;

namespace OrchardCore.Search.Elasticsearch.Core.Services;

public class ElasticSearchQueryService : IElasticSearchQueryService
{
    private readonly ElasticIndexManager _elasticIndexManager;

    public ElasticSearchQueryService(ElasticIndexManager elasticIndexManager)
    {
        _elasticIndexManager = elasticIndexManager;
    }

    public async Task<IList<string>> ExecuteQueryAsync(string indexName, QueryContainer query, List<ISort> sort, int from, int size)
    {
        var results = await _elasticIndexManager.SearchAsync(indexName, query, sort, from, size);

        if (results?.TopDocs is null || results.TopDocs.Count == 0)
        {
            return [];
        }

        var contentItemIds = new List<string>();

        foreach (var item in results.TopDocs)
        {
            contentItemIds.Add(item.GetValueOrDefault(nameof(ContentItem.ContentItemId)).ToString());
        }

        return contentItemIds;
    }
}
