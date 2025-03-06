using Azure.Search.Documents;
using Azure.Search.Documents.Models;
using Microsoft.Extensions.Logging;
using OrchardCore.Contents.Indexing;
using OrchardCore.Indexing;
using OrchardCore.Modules;
using OrchardCore.Search.AzureAI.Models;
using static OrchardCore.Indexing.DocumentIndexBase;

namespace OrchardCore.Search.AzureAI.Services;

public class AzureAIIndexDocumentManager
{
    private readonly AzureAIClientFactory _clientFactory;
    private readonly AzureAISearchIndexManager _indexManager;
    private readonly IEnumerable<IAzureAISearchDocumentEvents> _documentEvents;
    private readonly IEnumerable<IAzureAISearchEvents> _azureAISearchEvents;
    private readonly ILogger _logger;

    public AzureAIIndexDocumentManager(
        AzureAIClientFactory clientFactory,
        AzureAISearchIndexManager indexManager,
        IEnumerable<IAzureAISearchDocumentEvents> documentEvents,
        IEnumerable<IAzureAISearchEvents> azureAISearchEvents,
        ILogger<AzureAIIndexDocumentManager> logger)
    {
        _clientFactory = clientFactory;
        _indexManager = indexManager;
        _documentEvents = documentEvents;
        _azureAISearchEvents = azureAISearchEvents;
        _logger = logger;
    }

    public async Task<IEnumerable<SearchDocument>> SearchAsync(string indexName, string searchText, SearchOptions searchOptions = null)
    {
        ArgumentException.ThrowIfNullOrEmpty(indexName);
        ArgumentException.ThrowIfNullOrWhiteSpace(searchText);

        var client = GetSearchClient(indexName);

        var searchResult = await client.SearchAsync<SearchDocument>(searchText, searchOptions);

        var docs = new List<SearchDocument>();

        await foreach (var doc in searchResult.Value.GetResultsAsync())
        {
            docs.Add(doc.Document);
        }

        return docs;
    }

    public async Task<long?> SearchAsync(string indexName, string searchText, Action<SearchDocument> action, SearchOptions searchOptions = null)
    {
        ArgumentException.ThrowIfNullOrEmpty(indexName, nameof(indexName));
        ArgumentException.ThrowIfNullOrWhiteSpace(searchText, nameof(searchText));
        ArgumentNullException.ThrowIfNull(action);

        var client = GetSearchClient(indexName);

        var searchResult = await client.SearchAsync<SearchDocument>(searchText, searchOptions);
        var counter = 0L;

        if (searchResult.Value is null)
        {
            return counter;
        }

        await foreach (var doc in searchResult.Value.GetResultsAsync())
        {
            action(doc.Document);
            counter++;
        }

        return searchResult.Value.TotalCount ?? counter;
    }

    public async Task DeleteDocumentsAsync(string indexName, IEnumerable<string> contentItemIds)
    {
        ArgumentException.ThrowIfNullOrEmpty(indexName);
        ArgumentNullException.ThrowIfNull(contentItemIds);

        try
        {
            var client = GetSearchClient(indexName);

            await client.DeleteDocumentsAsync(IndexingConstants.ContentItemIdKey, contentItemIds);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unable to delete documents from Azure AI Search Settings");
        }
    }

    public async Task DeleteAllDocumentsAsync(string indexName)
    {
        var contentItemIds = new List<string>();

        try
        {
            var searchOptions = new SearchOptions();
            searchOptions.Select.Add(IndexingConstants.ContentItemIdKey);

            // Match-all documents.
            var totalRecords = SearchAsync(indexName, "*", (doc) =>
            {
                if (doc.TryGetValue(IndexingConstants.ContentItemIdKey, out var contentItemId))
                {
                    contentItemIds.Add(contentItemId.ToString());
                }
            }, searchOptions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unable to search documents using Azure AI Search");
        }

        if (contentItemIds.Count == 0)
        {
            return;
        }

        await DeleteDocumentsAsync(indexName, contentItemIds);
    }

    public async Task<bool> MergeOrUploadDocumentsAsync(string indexName, IList<DocumentIndexBase> indexDocuments, AzureAISearchIndexSettings indexSettings)
    {
        ArgumentException.ThrowIfNullOrEmpty(indexName);
        ArgumentNullException.ThrowIfNull(indexDocuments);
        ArgumentNullException.ThrowIfNull(indexSettings);

        if (indexDocuments.Count == 0)
        {
            return true;
        }

        try
        {
            var client = GetSearchClient(indexName);

            // The dictionary key should be indexingKey Not AzureFieldKey.
            var maps = indexSettings.GetMaps();

            var pages = indexDocuments.PagesOf(32000);

            foreach (var page in pages)
            {
                var docs = CreateSearchDocuments(page, maps);

                await _documentEvents.InvokeAsync(handler => handler.MergingOrUploadingAsync(docs), _logger);

                await client.MergeOrUploadDocumentsAsync(docs);
            }

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unable to delete documents from Azure AI Search Settings");
        }

        return false;
    }

    public async Task UploadDocumentsAsync(string indexName, IEnumerable<DocumentIndex> indexDocuments, AzureAISearchIndexSettings indexSettings)
    {
        ArgumentException.ThrowIfNullOrEmpty(indexName);
        ArgumentNullException.ThrowIfNull(indexDocuments);
        ArgumentNullException.ThrowIfNull(indexSettings);

        try
        {
            var client = GetSearchClient(indexName);

            var maps = indexSettings.GetMaps();

            var pages = indexDocuments.PagesOf(32000);

            foreach (var page in pages)
            {
                var docs = CreateSearchDocuments(page, maps);

                await _documentEvents.InvokeAsync(handler => handler.UploadingAsync(docs), _logger);

                await client.UploadDocumentsAsync(docs);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unable to delete documents from Azure AI Search Settings");
        }
    }

    public async Task<IList<AzureAISearchIndexMap>> GetMappingsAsync(AzureAISearchIndexSettings settings)
    {
        ArgumentNullException.ThrowIfNull(settings);

        var mappingContext = new AzureAISearchMappingContext(settings);

        await _azureAISearchEvents.InvokeAsync((handler, context) => handler.MappingAsync(context), mappingContext, _logger);

        return mappingContext.Mappings;
    }

    private static IEnumerable<SearchDocument> CreateSearchDocuments(IEnumerable<DocumentIndexBase> indexDocuments, Dictionary<string, IEnumerable<AzureAISearchIndexMap>> mappings)
    {
        foreach (var indexDocument in indexDocuments)
        {
            yield return CreateSearchDocument(indexDocument, mappings);
        }
    }

    private static SearchDocument CreateSearchDocument(DocumentIndexBase documentIndex, Dictionary<string, IEnumerable<AzureAISearchIndexMap>> mappingDictionary)
    {
        var doc = new SearchDocument();

        if (documentIndex is DocumentIndex index)
        {
            doc.Add(IndexingConstants.ContentItemIdKey, index.ContentItemId);
            doc.Add(IndexingConstants.ContentItemVersionIdKey, index.ContentItemVersionId);
        }

        foreach (var entry in documentIndex.Entries)
        {
            if (!mappingDictionary.TryGetValue(entry.Name, out var mappings))
            {
                continue;
            }

            var map = mappings.FirstOrDefault(x => x.IndexingKey == entry.Name);

            if (map == null)
            {
                continue;
            }

            switch (entry.Type)
            {
                case DocumentIndexBase.Types.Boolean:
                    if (entry.Value is bool boolValue)
                    {
                        doc.TryAdd(map.AzureFieldKey, boolValue);
                    }
                    break;

                case Types.DateTime:

                    if (entry.Value is DateTimeOffset offsetValue)
                    {
                        doc.TryAdd(map.AzureFieldKey, offsetValue);
                    }
                    else if (entry.Value is DateTime dateTimeValue)
                    {
                        doc.TryAdd(map.AzureFieldKey, dateTimeValue.ToUniversalTime());
                    }

                    break;

                case Types.Integer:
                    if (entry.Value != null && long.TryParse(entry.Value.ToString(), out var value))
                    {
                        doc.TryAdd(map.AzureFieldKey, value);
                    }

                    break;

                case Types.Number:
                    if (entry.Value != null)
                    {
                        doc.TryAdd(map.AzureFieldKey, Convert.ToDouble(entry.Value));
                    }
                    break;

                case Types.GeoPoint:
                    if (entry.Value != null)
                    {
                        doc.TryAdd(map.AzureFieldKey, entry.Value);
                    }
                    break;

                case Types.Text:
                    if (entry.Value != null)
                    {
                        var stringValue = Convert.ToString(entry.Value);

                        if (!string.IsNullOrEmpty(stringValue) && stringValue != IndexingConstants.NullValue)
                        {
                            if (UseSingleValue(map))
                            {
                                if (!doc.TryAdd(map.AzureFieldKey, stringValue))
                                {
                                    doc[map.AzureFieldKey] += stringValue;
                                }
                            }
                            else
                            {
                                if (!doc.TryGetValue(map.AzureFieldKey, out var obj) || obj is not List<string> existingValue)
                                {
                                    existingValue = [];
                                }

                                existingValue.Add(stringValue);

                                doc[map.AzureFieldKey] = existingValue;
                            }
                        }
                    }
                    break;
            }
        }

        return doc;
    }

    private static bool UseSingleValue(AzureAISearchIndexMap map)
    {
        // Full-text, Display-text-analyzed and non-collections support single value.
        return map.AzureFieldKey == AzureAISearchIndexManager.FullTextKey
            || map.AzureFieldKey == AzureAISearchIndexManager.DisplayTextAnalyzedKey
            || !map.IsCollection;
    }

    private SearchClient GetSearchClient(string indexName)
    {
        var fullIndexName = _indexManager.GetFullIndexName(indexName);

        var client = _clientFactory.CreateSearchClient(fullIndexName);

        return client;
    }
}
