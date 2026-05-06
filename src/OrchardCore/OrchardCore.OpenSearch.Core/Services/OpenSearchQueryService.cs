using Microsoft.Extensions.Logging;
using OrchardCore.ContentManagement;
using OrchardCore.Indexing.Models;
using OrchardCore.Search.Abstractions;

namespace OrchardCore.OpenSearch.Core.Services;

public class OpenSearchQueryService
{
    private readonly OpenSearchIndexManager _openSearchIndexManager;
    private readonly ILogger _logger;

    public OpenSearchQueryService(
        OpenSearchIndexManager openSearchIndexManager,
        ILogger<OpenSearchQueryService> logger)
    {
        _openSearchIndexManager = openSearchIndexManager;
        _logger = logger;
    }

    public async Task PopulateResultAsync(OpenSearchSearchContext context, SearchResult result)
    {
        var searchResult = await _openSearchIndexManager.SearchAsync(context);

        result.ContentItemIds = [];

        if (searchResult?.TopDocs is null || searchResult.TopDocs.Count == 0)
        {
            return;
        }

        result.TotalCount = searchResult.TotalCount;
        result.Highlights = [];

        foreach (var item in searchResult.TopDocs)
        {
            if (!item.Value.TryGetPropertyValue(nameof(ContentItem.ContentItemId), out var id))
            {
                continue;
            }

            var contentItemId = id?.GetValue<string>();

            if (string.IsNullOrEmpty(contentItemId))
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

    public async Task<IList<string>> GetContentItemIdsAsync(OpenSearchSearchContext request)
    {
        var results = await _openSearchIndexManager.SearchAsync(request);

        if (results?.TopDocs is null || results.TopDocs.Count == 0)
        {
            return [];
        }

        var contentItemIds = new List<string>();

        foreach (var item in results.TopDocs)
        {
            if (!item.Value.TryGetPropertyValue(nameof(ContentItem.ContentItemId), out var id))
            {
                continue;
            }

            var contentItemId = id?.GetValue<string>();

            if (string.IsNullOrEmpty(contentItemId))
            {
                continue;
            }

            contentItemIds.Add(contentItemId);
        }

        return contentItemIds;
    }

    public Task<OpenSearchResult> SearchAsync(IndexProfile index, string query)
    {
        ArgumentNullException.ThrowIfNull(index);
        ArgumentException.ThrowIfNullOrEmpty(query);

        try
        {
            return _openSearchIndexManager.SearchAsync(index, query);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while querying OpenSearch with exception: {Message}", ex.Message);
        }

        return Task.FromResult(new OpenSearchResult());
    }
}
