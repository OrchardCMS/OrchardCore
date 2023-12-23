using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Azure.Search.Documents;
using Azure.Search.Documents.Models;
using Microsoft.Extensions.Logging;
using OrchardCore.ContentManagement;
using OrchardCore.Contents.Indexing;
using OrchardCore.Indexing;
using OrchardCore.Modules;
using OrchardCore.Search.AzureAI.Models;

namespace OrchardCore.Search.AzureAI.Services;

public class AzureAIIndexDocumentManager(
    SearchClientFactory searchClientFactory,
    AzureAISearchIndexManager indexManager,
    IIndexingTaskManager indexingTaskManager,
    IContentManager contentManager,
    IEnumerable<IContentItemIndexHandler> contentItemIndexHandlers,
    ILogger<AzureAIIndexDocumentManager> logger)
{
    private readonly SearchClientFactory _searchClientFactory = searchClientFactory;
    private readonly AzureAISearchIndexManager _indexManager = indexManager;
    private readonly IIndexingTaskManager _indexingTaskManager = indexingTaskManager;
    private readonly IContentManager _contentManager = contentManager;
    private readonly IEnumerable<IContentItemIndexHandler> _contentItemIndexHandlers = contentItemIndexHandlers;
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

    public async Task<bool> MergeOrUploadDocumentsAsync(string indexName, IList<DocumentIndex> indexDocuments, AzureAISearchIndexSettings indexSettings)
    {
        ArgumentNullException.ThrowIfNull(indexDocuments, nameof(indexDocuments));
        ArgumentNullException.ThrowIfNull(indexSettings, nameof(indexSettings));

        if (indexDocuments.Count == 0)
        {
            return true;
        }

        try
        {
            var client = GetSearchClient(indexName);
            var pages = indexDocuments.PagesOf(32000);

            foreach (var page in pages)
            {
                var docs = CreateSearchDocuments(page, indexSettings.IndexMappings);

                var response = await client.MergeOrUploadDocumentsAsync(docs);
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
        ArgumentNullException.ThrowIfNull(indexDocuments, nameof(indexDocuments));
        ArgumentNullException.ThrowIfNull(indexSettings, nameof(indexSettings));

        try
        {
            var client = GetSearchClient(indexName);

            var docs = CreateSearchDocuments(indexDocuments, indexSettings.IndexMappings);

            await client.UploadDocumentsAsync(docs);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unable to delete documents from Azure AI Search Settings");
        }
    }

    public async Task<IList<AzureAISearchIndexMap>> GetMappingsAsync(string[] idexedContentTypes)
    {
        ArgumentNullException.ThrowIfNull(idexedContentTypes, nameof(idexedContentTypes));

        var mapping = new List<AzureAISearchIndexMap>();

        foreach (var contentType in idexedContentTypes)
        {
            var contentItem = await _contentManager.NewAsync(contentType);
            var index = new DocumentIndex(contentItem.ContentItemId, contentItem.ContentItemVersionId);
            var buildIndexContext = new BuildIndexContext(index, contentItem, [contentType], new AzureAISearchContentIndexSettings());
            await _contentItemIndexHandlers.InvokeAsync(x => x.BuildIndexAsync(buildIndexContext), _logger);

            foreach (var entry in index.Entries)
            {
                mapping.Add(new AzureAISearchIndexMap(entry.Name, entry.Type, entry.Options));
            }
        }

        return mapping;
    }

    private IEnumerable<SearchDocument> CreateSearchDocuments(IEnumerable<DocumentIndex> indexDocuments, IList<AzureAISearchIndexMap> mappings)
    {
        foreach (var indexDocument in indexDocuments)
        {
            yield return CreateSearchDocument(indexDocument, mappings);
        }
    }

    private SearchDocument CreateSearchDocument(DocumentIndex documentIndex, IList<AzureAISearchIndexMap> mappings)
    {
        var doc = new SearchDocument()
        {
            { IndexingConstants.ContentItemIdKey, documentIndex.ContentItemId },
            { IndexingConstants.ContentItemVersionIdKey, documentIndex.ContentItemVersionId },
        };

        foreach (var entry in documentIndex.Entries)
        {
            if (!mappings.Any(map => entry.Name.EqualsOrdinalIgnoreCase(map.Key)))
            {
                continue;
            }

            if (!AzureAISearchIndexManager.TryGetSafeFieldName(entry.Name, out var key))
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

                        if (!string.IsNullOrEmpty(stringValue) && stringValue != "NULL")
                        {
                            // Only full-test and display-text and keyword fields contains single string. All others, support a collection of strings.
                            if (key == AzureAISearchIndexManager.FullTextKey
                                || key == AzureAISearchIndexManager.DisplayTextAnalyzedKey
                                || entry.Options.HasFlag(DocumentIndexOptions.Keyword))
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
        var fullIndexName = _indexManager.GetFullIndexName(indexName);

        var client = _searchClientFactory.Create(fullIndexName);

        if (client is null)
        {
            throw new Exception("Endpoint is missing from Azure AI Search Settings");
        }

        return client;
    }
}
