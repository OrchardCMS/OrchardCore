using System.Text;
using Elastic.Clients.Elasticsearch;
using Microsoft.Extensions.Logging;
using OrchardCore.ContentManagement;

namespace OrchardCore.Search.Elasticsearch.Core.Services;
public class ElasticsearchQueryService
{
    private readonly ElasticsearchIndexManager _elasticIndexManager;
    private readonly ElasticsearchClient _client;
    private readonly ILogger _logger;

    public ElasticsearchQueryService(
        ElasticsearchIndexManager elasticIndexManager,
        ElasticsearchClient client,
        ILogger<ElasticsearchQueryService> logger)
    {
        _elasticIndexManager = elasticIndexManager;
        _client = client;
        _logger = logger;
    }

    public async Task<IList<string>> GetContentItemIdsAsync(ElasticsearchSearchContext request)
    {
        var results = await _elasticIndexManager.SearchAsync(request);

        if (results?.TopDocs is null || results.TopDocs.Count == 0)
        {
            return [];
        }

        var contentItemIds = new List<string>();

        foreach (var item in results.TopDocs)
        {
            if (!item.Value.TryGetPropertyValue(nameof(ContentItem.ContentItemId), out var contentItemId))
            {
                continue;
            }

            contentItemIds.Add(contentItemId.GetValue<string>());
        }

        return contentItemIds;
    }

    public Task<ElasticsearchResult> SearchAsync(string indexName, string query)
    {
        ArgumentException.ThrowIfNullOrEmpty(indexName);
        ArgumentException.ThrowIfNullOrEmpty(query);

        try
        {
            using var stream = new MemoryStream(Encoding.UTF8.GetBytes(query));
            var request = _client.RequestResponseSerializer.Deserialize<SearchRequest>(stream);

            var searchTask = _elasticIndexManager.SearchAsync(new ElasticsearchSearchContext(indexName, request.Query)
            {
                From = request.From,
                Size = request.Size,
                Fields = request.Fields,
                Sorts = request.Sort,
                Source = request.Source,
                Highlight = request.Highlight,
            });

            return searchTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while querying elastic with exception: {Message}", ex.Message);
        }

        return Task.FromResult(new ElasticsearchResult());
    }
}
