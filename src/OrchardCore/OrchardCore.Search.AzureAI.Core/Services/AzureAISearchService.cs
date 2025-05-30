using Azure.Search.Documents;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OrchardCore.Contents.Indexing;
using OrchardCore.Entities;
using OrchardCore.Indexing.Core.Models;
using OrchardCore.Indexing.Models;
using OrchardCore.Search.Abstractions;
using OrchardCore.Search.AzureAI.Models;

namespace OrchardCore.Search.AzureAI.Services;

public class AzureAISearchService : ISearchService
{
    private readonly AzureAISearchIndexDocumentManager _indexDocumentManager;
    private readonly ILogger<AzureAISearchService> _logger;
    private readonly AzureAISearchDefaultOptions _azureAIOptions;

    public AzureAISearchService(
        AzureAISearchIndexDocumentManager indexDocumentManager,
        ILogger<AzureAISearchService> logger,
        IOptions<AzureAISearchDefaultOptions> azureAIOptions)
    {
        _indexDocumentManager = indexDocumentManager;
        _logger = logger;
        _azureAIOptions = azureAIOptions.Value;
    }

    public async Task<SearchResult> SearchAsync(IndexEntity index, string term, int start, int size)
    {
        ArgumentNullException.ThrowIfNull(index);

        var result = new SearchResult();

        if (!_azureAIOptions.ConfigurationExists())
        {
            _logger.LogWarning("Azure AI Search: Couldn't execute search. Azure AI Search has not been configured yet.");

            return result;
        }

        result.Latest = index.As<ContentIndexMetadata>().IndexLatest;
        var queryMetadata = index.As<AzureAISearchDefaultQueryMetadata>();

        try
        {
            result.ContentItemIds = [];

            var searchOptions = new SearchOptions()
            {
                Skip = start,
                Size = size,
                Select = { ContentIndexingConstants.ContentItemIdKey },
            };


            if (queryMetadata.DefaultSearchFields?.Length > 0)
            {
                foreach (var field in queryMetadata.DefaultSearchFields)
                {
                    searchOptions.SearchFields.Add(field);
                }
            }
            else
            {
                var indexMetadata = index.As<AzureAISearchIndexMetadata>();

                foreach (var field in indexMetadata.IndexMappings)
                {
                    if (!field.IsSearchable)
                    {
                        continue;
                    }

                    searchOptions.SearchFields.Add(field.AzureFieldKey);
                }
            }

            await _indexDocumentManager.SearchAsync(index.IndexFullName, term, (doc) =>
            {
                if (doc.TryGetValue(ContentIndexingConstants.ContentItemIdKey, out var contentItemId))
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

