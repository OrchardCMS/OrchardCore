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

public class AzureAISearchIndexDocumentManager : IIndexDocumentManager
{
    private readonly AzureAIClientFactory _clientFactory;
    private readonly IEnumerable<IAzureAISearchDocumentEvents> _documentEvents;
    private readonly IIndexEntityStore _indexStore;
    private readonly ILogger _logger;

    public AzureAISearchIndexDocumentManager(
        AzureAIClientFactory clientFactory,
        IEnumerable<IAzureAISearchDocumentEvents> documentEvents,
        IIndexEntityStore indexStore,
        ILogger<AzureAISearchIndexDocumentManager> logger)
    {
        _clientFactory = clientFactory;
        _documentEvents = documentEvents;
        _indexStore = indexStore;
        _logger = logger;
    }

    public async Task<IEnumerable<SearchDocument>> SearchAsync(string indexFullName, string searchText, SearchOptions searchOptions = null)
    {
        ArgumentException.ThrowIfNullOrEmpty(indexFullName);
        ArgumentException.ThrowIfNullOrWhiteSpace(searchText);

        var client = _clientFactory.CreateSearchClient(indexFullName);

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
        ArgumentException.ThrowIfNullOrEmpty(indexFullName);
        ArgumentException.ThrowIfNullOrWhiteSpace(searchText);
        ArgumentNullException.ThrowIfNull(action);

        var client = _clientFactory.CreateSearchClient(indexFullName);

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

    public async Task<bool> DeleteDocumentsAsync(IndexEntity index, IEnumerable<string> documentIds)
    {
        ArgumentNullException.ThrowIfNull(index);
        ArgumentNullException.ThrowIfNull(documentIds);

        if (!documentIds.Any())
        {
            return false;
        }

        try
        {
            var client = _clientFactory.CreateSearchClient(index.IndexFullName);

            var keyName = GetKeyFieldNameOrThrow(index);

            await client.DeleteDocumentsAsync(keyName, documentIds);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unable to delete documents from Azure AI Search Settings");
        }

        return false;
    }

    public async Task<bool> DeleteAllDocumentsAsync(IndexEntity index)
    {
        ArgumentNullException.ThrowIfNull(index);

        var documentIds = new List<string>();

        try
        {
            var keyName = GetKeyFieldNameOrThrow(index);

            // Match-all documents.
            var totalRecords = SearchAsync(index.IndexFullName, "*", (doc) =>
            {
                if (doc.TryGetValue(keyName, out var contentItemId))
                {
                    documentIds.Add(contentItemId.ToString());
                }
            }, new SearchOptions()
            {
                Select = { keyName },
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unable to search documents using Azure AI Search");
        }

        if (documentIds.Count == 0)
        {
            return false;
        }

        return await DeleteDocumentsAsync(index, documentIds);
    }

    public async Task<bool> MergeOrUploadDocumentsAsync(IndexEntity index, IEnumerable<DocumentIndexBase> indexDocuments)
    {
        ArgumentNullException.ThrowIfNull(index);

        ArgumentNullException.ThrowIfNull(indexDocuments);

        if (!indexDocuments.Any())
        {
            return false;
        }

        try
        {
            var client = _clientFactory.CreateSearchClient(index.IndexFullName);

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

    public async Task UploadDocumentsAsync(IndexEntity index, IEnumerable<DocumentIndex> indexDocuments)
    {
        ArgumentNullException.ThrowIfNull(index);
        ArgumentNullException.ThrowIfNull(indexDocuments);

        try
        {
            var client = _clientFactory.CreateSearchClient(index.IndexFullName);

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

    private static string GetKeyFieldNameOrThrow(IndexEntity index)
    {
        var keyName = index.As<AzureAISearchIndexMetadata>().IndexMappings.FirstOrDefault(x => x.IsKey)?.AzureFieldKey;

        if (string.IsNullOrEmpty(keyName))
        {
            throw new InvalidOperationException($"The index {index.IndexFullName} does not have a key field.");
        }

        return keyName;
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
}
