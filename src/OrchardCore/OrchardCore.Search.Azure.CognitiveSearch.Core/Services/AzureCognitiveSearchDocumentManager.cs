using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Azure.Search.Documents;
using Azure.Search.Documents.Models;
using Microsoft.Extensions.Logging;
using OrchardCore.Indexing;

namespace OrchardCore.Search.Azure.CognitiveSearch.Services;

public class AzureCognitiveSearchDocumentManager
{
    private readonly SearchClientFactory _searchClientFactory;
    private readonly AzureCognitiveSearchIndexManager _indexManager;
    private readonly ILogger _logger;

    public AzureCognitiveSearchDocumentManager(
        SearchClientFactory searchClientFactory,
        AzureCognitiveSearchIndexManager indexManager,
        ILogger<AzureCognitiveSearchDocumentManager> logger)
    {
        _searchClientFactory = searchClientFactory;
        _indexManager = indexManager;
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

    public Task<IEnumerable<SearchDocument>> SearchAsync(string indexName, string searchText, int start, int size, string[] fields)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(searchText, nameof(searchText));
        ArgumentNullException.ThrowIfNull(fields, nameof(fields));

        var searchOptions = new SearchOptions()
        {
            Skip = start,
            Size = size,
        };

        foreach (var field in fields)
        {
            searchOptions.SearchFields.Add(field);
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

            await client.DeleteDocumentsAsync(IndexingConstants.ContentItemIdKey, contentItemIds);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unable to delete documents from Azure Cognative Search Settings");
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
            _logger.LogError(ex, "Unable to search documents using Azure Cognative Search");
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
            _logger.LogError(ex, "Unable to delete documents from Azure Cognative Search Settings");
        }
    }

    public async Task MergeOrUploadDocumentsAsync(string indexName, IEnumerable<DocumentIndex> indexDocuments)
    {
        try
        {
            var client = GetSearchClient(indexName);

            await client.MergeOrUploadDocumentsAsync(CreateSearchDocuments(indexDocuments));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unable to delete documents from Azure Cognative Search Settings");
        }
    }

    public async Task UploadDocumentsAsync(string indexName, IEnumerable<DocumentIndex> indexDocuments)
    {
        try
        {
            var client = GetSearchClient(indexName);

            await client.UploadDocumentsAsync(CreateSearchDocuments(indexDocuments));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unable to delete documents from Azure Cognative Search Settings");
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
                { IndexingConstants.ContentItemIdKey, documentIndex.ContentItemId },
                { IndexingConstants.ContentItemVersionIdKey, documentIndex.ContentItemVersionId }
            };

        foreach (var entry in documentIndex.Entries)
        {
            switch (entry.Type)
            {
                case DocumentIndex.Types.Boolean:
                    if (entry.Value is bool boolValue)
                    {
                        AddValue(doc, entry.Name, boolValue);
                    }
                    break;

                case DocumentIndex.Types.DateTime:

                    if (entry.Value is DateTimeOffset offsetValue)
                    {
                        AddValue(doc, entry.Name, offsetValue);
                    }
                    else if (entry.Value is DateTime dateTimeValue)
                    {
                        AddValue(doc, entry.Name, dateTimeValue.ToUniversalTime());
                    }

                    break;

                case DocumentIndex.Types.Integer:
                    if (entry.Value != null && long.TryParse(entry.Value.ToString(), out var value))
                    {
                        AddValue(doc, entry.Name, value);
                    }

                    break;

                case DocumentIndex.Types.Number:
                    if (entry.Value != null)
                    {
                        AddValue(doc, entry.Name, Convert.ToDouble(entry.Value));
                    }
                    break;

                case DocumentIndex.Types.Text:
                    if (entry.Value != null)
                    {
                        var stringValue = Convert.ToString(entry.Value);

                        if (!string.IsNullOrEmpty(stringValue))
                        {
                            AddValue(doc, entry.Name, stringValue);
                        }
                    }
                    break;
            }
        }

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
        var fullIndexName = _indexManager.GetFullIndexName(indexName);

        var client = _searchClientFactory.Create(fullIndexName);

        if (client is null)
        {
            throw new Exception("Endpoint is missing from Azure Cognative Search Settings");
        }

        return client;
    }
}
