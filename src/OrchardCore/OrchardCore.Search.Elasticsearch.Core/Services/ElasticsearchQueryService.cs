using Json.Path;
using Microsoft.Extensions.Logging;
using OrchardCore.ContentManagement;
using OrchardCore.Indexing.Models;
using OrchardCore.Search.Abstractions;

namespace OrchardCore.Search.Elasticsearch.Core.Services;

public class ElasticsearchQueryService
{
    private readonly ElasticsearchIndexManager _elasticIndexManager;
    private readonly ILogger _logger;

    public ElasticsearchQueryService(
        ElasticsearchIndexManager elasticIndexManager,
        ILogger<ElasticsearchQueryService> logger)
    {
        _elasticIndexManager = elasticIndexManager;
        _logger = logger;
    }

    public async Task PopulateResultAsync(ElasticsearchSearchContext request, SearchResult result)
    {
        var searchResult = await _elasticIndexManager.SearchAsync(request);

        result.ContentItemIds = [];

        if (searchResult?.TopDocs is null || searchResult.TopDocs.Count == 0)
        {
            return;
        }

        result.TotalCount = searchResult.TotalCount;
        result.Highlights = [];

        foreach (var item in searchResult.TopDocs)
        {
            if (!item.Value.TryGetPropertyValue(nameof(ContentItem.ContentItemId), out var id) ||
                !id.TryGetValue<string>(out var contentItemId))
            {
                continue;
            }

            if (item.Highlights is not null && item.Highlights.Count > 0)
            {
                result.Highlights[contentItemId] = item.Highlights;
            }

            result.ContentItemIds.Add(contentItemId);
        }
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
            if (!item.Value.TryGetPropertyValue(nameof(ContentItem.ContentItemId), out var id) ||
                !id.TryGetValue<string>(out var contentItemId))
            {
                continue;
            }

            contentItemIds.Add(contentItemId);
        }

        return contentItemIds;
    }

    public Task<ElasticsearchResult> SearchAsync(IndexProfile index, string query)
    {
        ArgumentNullException.ThrowIfNull(index);
        ArgumentException.ThrowIfNullOrEmpty(query);

        try
        {
            return _elasticIndexManager.SearchAsync(index, query);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while querying elastic with exception: {Message}", ex.Message);
        }

        return Task.FromResult(new ElasticsearchResult());
    }
}
