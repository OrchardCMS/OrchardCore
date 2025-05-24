using Azure.Search.Documents;
using Azure.Search.Documents.Models;
using Microsoft.Extensions.Logging;
using OrchardCore.Contents.Indexing;
using OrchardCore.Entities;
using OrchardCore.Indexing;
using OrchardCore.Indexing.Models;
using OrchardCore.Modules;
using OrchardCore.Search.AzureAI.Models;
using static OrchardCore.Indexing.DocumentIndexBase;

namespace OrchardCore.Search.AzureAI.Services;

public class AzureAIIndexDocumentManager : IIndexDocumentManager
{
    private readonly AzureAIClientFactory _clientFactory;
    private readonly AzureAISearchIndexNameService _indexNameService;
    private readonly IEnumerable<IAzureAISearchDocumentEvents> _documentEvents;
    private readonly IIndexEntityStore _indexStore;
    private readonly ILogger _logger;

    public AzureAIIndexDocumentManager(
        AzureAIClientFactory clientFactory,
        AzureAISearchIndexNameService indexNameService,
        IEnumerable<IAzureAISearchDocumentEvents> documentEvents,
        IIndexEntityStore indexStore,
        ILogger<AzureAIIndexDocumentManager> logger)
    {
        _clientFactory = clientFactory;
        _indexNameService = indexNameService;
        _documentEvents = documentEvents;
        _indexStore = indexStore;
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

    public async Task<long?> SearchAsync(string indexFullName, string searchText, Action<SearchDocument> action, SearchOptions searchOptions = null)
    {
        ArgumentException.ThrowIfNullOrEmpty(indexFullName, nameof(indexFullName));
        ArgumentException.ThrowIfNullOrWhiteSpace(searchText, nameof(searchText));
        ArgumentNullException.ThrowIfNull(action);

        var client = GetSearchClient(indexFullName);

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

    public async Task<bool> DeleteDocumentsAsync(string indexFullName, IEnumerable<string> documentIds)
    {
        ArgumentException.ThrowIfNullOrEmpty(indexFullName);
        ArgumentNullException.ThrowIfNull(documentIds);

        try
        {
            var client = GetSearchClient(indexFullName);

            await client.DeleteDocumentsAsync(ContentIndexingConstants.ContentItemIdKey, documentIds);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unable to delete documents from Azure AI Search Settings");
        }

        return false;
    }

    public async Task<bool> DeleteAllDocumentsAsync(string indexFullName)
    {
        var contentItemIds = new List<string>();

        try
        {
            var searchOptions = new SearchOptions();
            searchOptions.Select.Add(ContentIndexingConstants.ContentItemIdKey);

            // Match-all documents.
            var totalRecords = SearchAsync(indexFullName, "*", (doc) =>
            {
                if (doc.TryGetValue(ContentIndexingConstants.ContentItemIdKey, out var contentItemId))
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
            return false;
        }

        return await DeleteDocumentsAsync(indexFullName, contentItemIds);
    }

    public async Task<bool> MergeOrUploadDocumentsAsync(string indexFullName, IEnumerable<DocumentIndexBase> indexDocuments, IndexEntity index)
    {
        ArgumentException.ThrowIfNullOrEmpty(indexFullName);
        ArgumentNullException.ThrowIfNull(indexDocuments);
        ArgumentNullException.ThrowIfNull(index);

        if (!indexDocuments.Any())
        {
            return false;
        }

        try
        {
            var client = GetSearchClient(indexFullName);

            // The dictionary key should be indexingKey Not AzureFieldKey.
            var maps = index.As<AzureAISearchIndexMetadata>().GetMaps();

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

    public async Task UploadDocumentsAsync(string indexFullName, IEnumerable<DocumentIndex> indexDocuments, IndexEntity index)
    {
        ArgumentException.ThrowIfNullOrEmpty(indexFullName);
        ArgumentNullException.ThrowIfNull(indexDocuments);
        ArgumentNullException.ThrowIfNull(index);

        try
        {
            var client = GetSearchClient(indexFullName);

            var maps = index.As<AzureAISearchIndexMetadata>().GetMaps();

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

    public Task<long> GetLastTaskIdAsync(IndexEntity index)
    {
        ArgumentNullException.ThrowIfNull(index);

        return Task.FromResult(index.As<ContentIndexingMetadata>().LastTaskId);
    }

    public async Task SetLastTaskIdAsync(IndexEntity index, long lastTaskId)
    {
        ArgumentNullException.ThrowIfNull(index);

        index.Alter<ContentIndexingMetadata>(metadata =>
        {
            metadata.LastTaskId = lastTaskId;
        });

        await _indexStore.UpdateAsync(index);
    }

    public IContentIndexSettings GetContentIndexSettings()
        => new AzureAISearchContentIndexSettings();

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
            doc.Add(ContentIndexingConstants.ContentItemIdKey, index.ContentItemId);
            doc.Add(ContentIndexingConstants.ContentItemVersionIdKey, index.ContentItemVersionId);
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

                        if (!string.IsNullOrEmpty(stringValue) && stringValue != ContentIndexingConstants.NullValue)
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
        var fullIndexName = _indexNameService.GetFullIndexName(indexName);

        return _clientFactory.CreateSearchClient(fullIndexName);
    }
}
