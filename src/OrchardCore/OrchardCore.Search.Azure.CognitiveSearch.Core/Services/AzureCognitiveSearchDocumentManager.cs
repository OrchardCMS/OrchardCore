using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Azure.Search.Documents;
using Azure.Search.Documents.Models;
using Microsoft.Extensions.Logging;
using OrchardCore.Contents.Indexing;
using OrchardCore.Indexing;

namespace OrchardCore.Search.Azure.CognitiveSearch.Services;

public class AzureCognitiveSearchDocumentManager
{
    private readonly SearchClientFactory _searchClientFactory;
    private readonly AzureCognitiveSearchIndexManager _searchIndexManager;
    private readonly ILogger _logger;
    private readonly static Dictionary<string, string> _fieldMaps = new()
    {
        { IndexingConstants.OwnerKey, CognitiveIndexingConstants.OwnerKey },
        { IndexingConstants.FullTextKey, CognitiveIndexingConstants.FullTextKey },
        { IndexingConstants.DisplayTextAnalyzedKey, CognitiveIndexingConstants.DisplayTextAnalyzedKey },
        { IndexingConstants.ContentTypeKey, CognitiveIndexingConstants.ContentTypeKey },
        { IndexingConstants.LatestKey, CognitiveIndexingConstants.LatestKey },
        { IndexingConstants.PublishedUtcKey, CognitiveIndexingConstants.PublishedUtcKey },
        { IndexingConstants.CreatedUtcKey, CognitiveIndexingConstants.CreatedUtcKey },
        { IndexingConstants.ModifiedUtcKey, CognitiveIndexingConstants.ModifiedUtcKey },
    };

    public AzureCognitiveSearchDocumentManager(
        SearchClientFactory searchClientFactory,
        AzureCognitiveSearchIndexManager searchIndexManager,
        ILogger<AzureCognitiveSearchDocumentManager> logger)
    {
        _searchClientFactory = searchClientFactory;
        _searchIndexManager = searchIndexManager;
        _logger = logger;
    }

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
        try
        {
            var client = GetSearchClient(indexName);

            await client.DeleteDocumentsAsync(CognitiveIndexingConstants.ContentItemIdKey, contentItemIds);
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
                if (doc.TryGetValue(CognitiveIndexingConstants.ContentItemIdKey, out var contentItemId))
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

            await client.DeleteDocumentsAsync(CognitiveIndexingConstants.ContentItemIdKey, contentItemIds);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unable to delete documents from Azure Cognitive Search Settings");
        }
    }

    public async Task MergeOrUploadDocumentsAsync(string indexName, IEnumerable<DocumentIndex> indexDocuments)
    {
        try
        {
            var client = GetSearchClient(indexName);

            var docs = CreateSearchDocuments(indexDocuments);

            var response = await client.MergeOrUploadDocumentsAsync(docs);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unable to delete documents from Azure Cognitive Search Settings");
        }
    }

    public async Task UploadDocumentsAsync(string indexName, IEnumerable<DocumentIndex> indexDocuments)
    {
        try
        {
            var client = GetSearchClient(indexName);

            var docs = CreateSearchDocuments(indexDocuments);

            await client.UploadDocumentsAsync(docs);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unable to delete documents from Azure Cognitive Search Settings");
        }
    }

    private static IEnumerable<SearchDocument> CreateSearchDocuments(IEnumerable<DocumentIndex> indexDocuments)
    {
        foreach (var indexDocument in indexDocuments)
        {
            yield return CreateSearchDocument(indexDocument);
        }
    }

    private static SearchDocument CreateSearchDocument(DocumentIndex documentIndex)
    {
        var doc = new SearchDocument()
            {
                { CognitiveIndexingConstants.ContentItemIdKey, documentIndex.ContentItemId },
                { CognitiveIndexingConstants.ContentItemVersionIdKey, documentIndex.ContentItemVersionId },
            };

        var properties = new List<KeyValuePair<string, string>>();

        foreach (var entry in documentIndex.Entries)
        {
            if (!_fieldMaps.TryGetValue(entry.Name, out var key))
            {
                var stringValue = entry.Value?.ToString();

                if (string.IsNullOrEmpty(stringValue))
                {
                    continue;
                }

                properties.Add(new KeyValuePair<string, string>(entry.Name.Replace(".", "__"), stringValue));

                continue;
            }

            switch (entry.Type)
            {
                case DocumentIndex.Types.Boolean:
                    if (entry.Value is bool boolValue)
                    {
                        AddValue(doc, key, boolValue);
                    }
                    break;

                case DocumentIndex.Types.DateTime:

                    if (entry.Value is DateTimeOffset offsetValue)
                    {
                        AddValue(doc, key, offsetValue);
                    }
                    else if (entry.Value is DateTime dateTimeValue)
                    {
                        AddValue(doc, key, dateTimeValue.ToUniversalTime());
                    }

                    break;

                case DocumentIndex.Types.Integer:
                    if (entry.Value != null && long.TryParse(entry.Value.ToString(), out var value))
                    {
                        AddValue(doc, key, value);
                    }

                    break;

                case DocumentIndex.Types.Number:
                    if (entry.Value != null)
                    {
                        AddValue(doc, key, Convert.ToDouble(entry.Value));
                    }
                    break;

                case DocumentIndex.Types.Text:
                    if (entry.Value != null)
                    {
                        var stringValue = Convert.ToString(entry.Value);

                        if (!string.IsNullOrEmpty(stringValue))
                        {
                            AddValue(doc, key, stringValue);
                        }
                    }
                    break;
            }
        }

        doc[CognitiveIndexingConstants.Properties] = properties;

        return doc;
    }

    private static void AddValue(SearchDocument doc, string key, object value)
    {
        if (doc.TryAdd(key, value))
        {
            return;
        }

        // At this point, we know that a value already exists.
        if (doc[key] is List<object> list)
        {
            list.Add(value);

            doc[key] = list;

            return;
        }

        // Convert the existing value to a list of values.
        var values = new List<object>()
            {
                doc[key],
                value,
            };

        doc[key] = values;
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
