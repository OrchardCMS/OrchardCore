using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.QueryDsl;
using OrchardCore.ContentManagement;

namespace OrchardCore.Search.Elasticsearch.Core.Services;

internal class ElasticsearchQueryService
{
    private readonly ElasticsearchIndexManager _elasticIndexManager;

    public ElasticsearchQueryService(ElasticsearchIndexManager elasticIndexManager)
    {
        _elasticIndexManager = elasticIndexManager;
    }

    public async Task<IList<string>> ExecuteQueryAsync(string indexName, Query query, List<SortOptions> sort, int from, int size)
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
