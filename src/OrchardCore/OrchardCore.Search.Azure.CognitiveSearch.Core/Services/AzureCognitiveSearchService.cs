using System;
using System.Threading.Tasks;
using Azure.Search.Documents;
using Microsoft.Extensions.Logging;
using OrchardCore.Search.Abstractions;
using OrchardCore.Search.Azure.CognitiveSearch.Models;
using OrchardCore.Settings;

namespace OrchardCore.Search.Azure.CognitiveSearch.Services;

public class AzureCognitiveSearchService : ISearchService
{
    public AzureCognitiveSearchService(
        ISiteService siteService,
        AzureCognitiveSearchDocumentManager searchDocumentManager,
        CognitiveSearchIndexSettingsService cognitiveSearchIndexSettingsService,
        ILogger<AzureCognitiveSearchService> logger
        )
    {
        _siteService = siteService;
        _searchDocumentManager = searchDocumentManager;
        _cognitiveSearchIndexSettingsService = cognitiveSearchIndexSettingsService;
        _logger = logger;
    }

    public const string Key = "Azure Cognitive Search";

    private readonly ISiteService _siteService;
    private readonly AzureCognitiveSearchDocumentManager _searchDocumentManager;
    private readonly CognitiveSearchIndexSettingsService _cognitiveSearchIndexSettingsService;
    private readonly ILogger<AzureCognitiveSearchService> _logger;

    public string Name => Key;

    public async Task<SearchResult> SearchAsync(string indexName, string term, int start, int size)
    {
        var siteSettings = await _siteService.GetSiteSettingsAsync();
        var searchSettings = siteSettings.As<AzureCognitiveSearchSettings>();

        var index = !string.IsNullOrWhiteSpace(indexName) ? indexName.Trim() : searchSettings.SearchIndex;

        var result = new SearchResult();
        if (string.IsNullOrEmpty(index))
        {
            _logger.LogWarning("Azure Cognitive Search: Couldn't execute search. No search provider settings was defined.");

            return result;
        }

        var indexSettings = await _cognitiveSearchIndexSettingsService.GetSettingsAsync(index);
        result.Latest = indexSettings.IndexLatest;

        if (searchSettings.DefaultSearchFields?.Length == 0)
        {
            _logger.LogWarning("Azure Cognitive Search: Couldn't execute search. No search provider settings was defined.");

            return result;
        }
        try
        {
            result.ContentItemIds = [];

            var searchOptions = new SearchOptions()
            {
                Skip = start,
                Size = size,
            };

            if (searchSettings.DefaultSearchFields?.Length > 0)
            {
                foreach (var field in searchSettings.DefaultSearchFields)
                {
                    searchOptions.SearchFields.Add(field);
                }
            }

            await _searchDocumentManager.SearchAsync(index, term, (doc) =>
            {
                if (doc.TryGetValue(CognitiveIndexingConstants.ContentItemIdKey, out var contentItemId))
                {
                    result.ContentItemIds.Add(contentItemId.ToString());
                }
            }, searchOptions);

            result.Success = true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Azure Cognitive Search: Couldn't execute search due to an exception.");
        }

        return result;
    }
}

