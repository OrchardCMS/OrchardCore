using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Azure.Search.Documents;
using Azure.Search.Documents.Models;
using Microsoft.Extensions.Logging;
using OrchardCore.Contents.Indexing;
using OrchardCore.Indexing;
using OrchardCore.Modules;
using OrchardCore.Search.Azure.CognitiveSearch.Models;

namespace OrchardCore.Search.Azure.CognitiveSearch.Services;

public class AzureCognitiveSearchDocumentManager(
    SearchClientFactory searchClientFactory,
    AzureCognitiveSearchIndexManager searchIndexManager,
    ILogger<AzureCognitiveSearchDocumentManager> logger)
{
    private readonly SearchClientFactory _searchClientFactory = searchClientFactory;
    private readonly AzureCognitiveSearchIndexManager _searchIndexManager = searchIndexManager;
    private readonly ILogger _logger = logger;

    public async Task<IEnumerable<SearchDocument>> SearchAsync(string indexName, string searchText, SearchOptions searchOptions = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(searchText, nameof(searchText));

        var client = GetSearchClient(indexName);

        var searchResult = await client.SearchAsync<SearchDocument>(searchText, searchOptions);
        var docs = new List<SearchDocument>();
        await foreach (var doc in searchResult.Value.GetResultsAsync())
        {
            docs.Add(doc.Document);
        }

        return docs;
    }

    public Task<IEnumerable<SearchDocument>> SearchAsync(string indexName, string searchText, int start, int size, string[] fields = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(searchText, nameof(searchText));

        var searchOptions = new SearchOptions()
        {
            Skip = start,
            Size = size,
        };

        if (fields?.Length > 0)
        {
            foreach (var field in fields)
            {
                searchOptions.SearchFields.Add(field);
            }
        }

        return SearchAsync(indexName, searchText, searchOptions);
    }

    public async Task<long?> SearchAsync(string indexName, string searchText, Action<SearchDocument> action, SearchOptions searchOptions = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(searchText, nameof(searchText));
        ArgumentNullException.ThrowIfNull(action);

        var client = GetSearchClient(indexName);

        var searchResult = await client.SearchAsync<SearchDocument>(searchText, searchOptions);
        var counter = 0L;

        await foreach (var doc in searchResult.Value.GetResultsAsync())
        {
            action(doc.Document);
            counter++;
        }

        return searchResult.Value.TotalCount ?? counter;
    }

    public async Task DeleteDocumentsAsync(string indexName, IEnumerable<string> contentItemIds)
    {
        ArgumentNullException.ThrowIfNull(contentItemIds, nameof(contentItemIds));

        try
        {
            var client = GetSearchClient(indexName);

            await client.DeleteDocumentsAsync(IndexingConstants.ContentItemIdKey, contentItemIds);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unable to delete documents from Azure Cognitive Search Settings");
        }
    }

    public async Task DeleteAllDocumentsAsync(string indexName)
    {
        var contentItemIds = new List<string>();

        try
        {
            // Match-all documents.
            var totalRecords = SearchAsync(indexName, "*", (doc) =>
            {
                if (doc.TryGetValue(IndexingConstants.ContentItemIdKey, out var contentItemId))
                {
                    contentItemIds.Add(contentItemId.ToString());
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unable to search documents using Azure Cognitive Search");
        }

        if (contentItemIds.Count == 0)
        {
            return;
        }

        try
        {
            var client = GetSearchClient(indexName);

            await client.DeleteDocumentsAsync(IndexingConstants.ContentItemIdKey, contentItemIds);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unable to delete documents from Azure Cognitive Search Settings");
        }
    }

    public async Task MergeOrUploadDocumentsAsync(string indexName, IEnumerable<DocumentIndex> indexDocuments, AzureCognitiveSearchIndexSettings indexSettings)
    {
        ArgumentNullException.ThrowIfNull(indexDocuments, nameof(indexDocuments));
        ArgumentNullException.ThrowIfNull(indexSettings, nameof(indexSettings));

        try
        {
            var client = GetSearchClient(indexName);

            var docs = CreateSearchDocuments(indexDocuments, indexSettings);

            var response = await client.MergeOrUploadDocumentsAsync(docs);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unable to delete documents from Azure Cognitive Search Settings");
        }
    }

    public async Task UploadDocumentsAsync(string indexName, IEnumerable<DocumentIndex> indexDocuments, AzureCognitiveSearchIndexSettings indexSettings)
    {
        ArgumentNullException.ThrowIfNull(indexDocuments, nameof(indexDocuments));
        ArgumentNullException.ThrowIfNull(indexSettings, nameof(indexSettings));

        try
        {
            var client = GetSearchClient(indexName);

            var docs = CreateSearchDocuments(indexDocuments, indexSettings);

            await client.UploadDocumentsAsync(docs);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unable to delete documents from Azure Cognitive Search Settings");
        }
    }

    private IEnumerable<SearchDocument> CreateSearchDocuments(IEnumerable<DocumentIndex> indexDocuments, AzureCognitiveSearchIndexSettings indexSettings)
    {
        foreach (var indexDocument in indexDocuments)
        {
            yield return CreateSearchDocument(indexDocument, indexSettings);
        }
    }

    private SearchDocument CreateSearchDocument(DocumentIndex documentIndex, AzureCognitiveSearchIndexSettings indexSettings)
    {
        var doc = new SearchDocument()
        {
            { IndexingConstants.ContentItemIdKey, documentIndex.ContentItemId },
            { IndexingConstants.ContentItemVersionIdKey, documentIndex.ContentItemVersionId },
        };

        foreach (var entry in documentIndex.Entries)
        {
            if (!_searchIndexManager.TryGetSafeFieldName(entry.Name, out var key))
            {
                continue;
            }

            if (!indexSettings.IndexMappings.Any(map => key.EqualsOrdinalIgnoreCase(map.Key)))
            {
                continue;
            }

            switch (entry.Type)
            {
                case DocumentIndex.Types.Boolean:
                    if (entry.Value is bool boolValue)
                    {
                        doc.TryAdd(key, boolValue);
                    }
                    break;

                case DocumentIndex.Types.DateTime:

                    if (entry.Value is DateTimeOffset offsetValue)
                    {
                        doc.TryAdd(key, offsetValue);
                    }
                    else if (entry.Value is DateTime dateTimeValue)
                    {
                        doc.TryAdd(key, dateTimeValue.ToUniversalTime());
                    }

                    break;

                case DocumentIndex.Types.Integer:
                    if (entry.Value != null && long.TryParse(entry.Value.ToString(), out var value))
                    {
                        doc.TryAdd(key, value);
                    }

                    break;

                case DocumentIndex.Types.Number:
                    if (entry.Value != null)
                    {
                        doc.TryAdd(key, Convert.ToDouble(entry.Value));
                    }
                    break;

                case DocumentIndex.Types.GeoPoint:
                    if (entry.Value != null)
                    {
                        doc.TryAdd(key, entry.Value);
                    }
                    break;

                case DocumentIndex.Types.Text:
                    if (entry.Value != null)
                    {
                        var stringValue = Convert.ToString(entry.Value);

                        if (!string.IsNullOrEmpty(stringValue))
                        {
                            // Only full-test field is single string value. All others, support a collection of strings.
                            if (key == AzureCognitiveSearchIndexManager.FullTextKey)
                            {
                                doc.TryAdd(key, stringValue);
                            }
                            else
                            {
                                if (!doc.TryGetValue(key, out var obj) || obj is not List<string> existingValue)
                                {
                                    existingValue = [];
                                }

                                existingValue.Add(stringValue);

                                doc[key] = existingValue;
                            }
                        }
                    }
                    break;
            }
        }

        return doc;
    }

    private SearchClient GetSearchClient(string indexName)
    {
        var fullIndexName = _searchIndexManager.GetFullIndexName(indexName);

        var client = _searchClientFactory.Create(fullIndexName);

        if (client is null)
        {
            throw new Exception("Endpoint is missing from Azure Cognitive Search Settings");
        }

        return client;
    }
}
