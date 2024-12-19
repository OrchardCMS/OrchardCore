using Azure.Search.Documents;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OrchardCore.Contents.Indexing;
using OrchardCore.Search.Abstractions;
using OrchardCore.Search.AzureAI.Models;
using OrchardCore.Settings;

namespace OrchardCore.Search.AzureAI.Services;

public class AzureAISearchService : ISearchService
{
    public const string Key = "Azure AI Search";

    private readonly ISiteService _siteService;
    private readonly AzureAIIndexDocumentManager _indexDocumentManager;
    private readonly AzureAISearchIndexSettingsService _indexSettingsService;
    private readonly ILogger<AzureAISearchService> _logger;
    private readonly AzureAISearchDefaultOptions _azureAIOptions;

    public AzureAISearchService(
        ISiteService siteService,
        AzureAIIndexDocumentManager indexDocumentManager,
        AzureAISearchIndexSettingsService indexSettingsService,
        ILogger<AzureAISearchService> logger,
        IOptions<AzureAISearchDefaultOptions> azureAIOptions)
    {
        _siteService = siteService;
        _indexDocumentManager = indexDocumentManager;
        _indexSettingsService = indexSettingsService;
        _logger = logger;
        _azureAIOptions = azureAIOptions.Value;
    }

    public string Name => Key;

    public async Task<SearchResult> SearchAsync(string indexName, string term, int start, int size)
    {
        var result = new SearchResult();

        if (!_azureAIOptions.ConfigurationExists())
        {
            _logger.LogWarning("Azure AI Search: Couldn't execute search. Azure AI Search has not been configured yet.");

            return result;
        }

        var searchSettings = await _siteService.GetSettingsAsync<AzureAISearchSettings>();

        var index = !string.IsNullOrWhiteSpace(indexName) ? indexName.Trim() : searchSettings.SearchIndex;

        if (string.IsNullOrEmpty(index))
        {
            _logger.LogWarning("Azure AI Search: Couldn't execute search. No search provider settings was defined.");

            return result;
        }

        var indexSettings = await _indexSettingsService.GetAsync(index);

        if (indexSettings is null)
        {
            _logger.LogWarning("Azure AI Search: Couldn't execute search. Unable to get the search index settings. Index name {IndexName}", index);

            return result;
        }

        result.Latest = indexSettings.IndexLatest;

        try
        {
            result.ContentItemIds = [];

            var searchOptions = new SearchOptions()
            {
                Skip = start,
                Size = size,
            };

            searchOptions.Select.Add(IndexingConstants.ContentItemIdKey);

            if (searchSettings.DefaultSearchFields?.Length > 0)
            {
                foreach (var field in searchSettings.DefaultSearchFields)
                {
                    searchOptions.SearchFields.Add(field);
                }
            }

            await _indexDocumentManager.SearchAsync(index, term, (doc) =>
            {
                if (doc.TryGetValue(IndexingConstants.ContentItemIdKey, out var contentItemId))
                {
                    result.ContentItemIds.Add(contentItemId.ToString());
                }
            }, searchOptions);

            result.Success = true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Azure AI Search: Couldn't execute search due to an exception.");
        }

        return result;
    }
}

