using System.Text.Json;
using Microsoft.Extensions.Logging;
using OpenSearch.Client;
using OrchardCore.ContentManagement;
using OrchardCore.Contents.Indexing;
using OrchardCore.Entities;
using OrchardCore.Indexing;
using OrchardCore.Indexing.Models;
using OrchardCore.OpenSearch.Core.Models;
using OrchardCore.OpenSearch.Models;

namespace OrchardCore.OpenSearch.Core.Services;

public sealed class OpenSearchDocumentIndexManager : IDocumentIndexManager
{
    private readonly OpenSearchClient _client;
    private readonly ILogger _logger;

    public OpenSearchDocumentIndexManager(
        OpenSearchClient client,
        ILogger<OpenSearchDocumentIndexManager> logger)
    {
        _client = client;
        _logger = logger;
    }

    public async Task<bool> DeleteAllDocumentsAsync(IndexProfile index)
    {
        ArgumentNullException.ThrowIfNull(index);

        var response = await _client.DeleteByQueryAsync<Dictionary<string, object>>(d => d
            .Index(index.IndexFullName)
            .Query(q => q.MatchAll())
        );

        if (!response.IsValid)
        {
            if (response.OriginalException != null)
            {
                _logger.LogError(response.OriginalException, "There were issues deleting documents in an OpenSearch index");
            }
            else
            {
                _logger.LogWarning("There were issues deleting documents in an OpenSearch index");
            }
        }

        return response.IsValid;
    }

    public async Task<bool> DeleteDocumentsAsync(IndexProfile index, IEnumerable<string> ids)
    {
        ArgumentNullException.ThrowIfNull(index);
        ArgumentNullException.ThrowIfNull(ids);

        if (!ids.Any())
        {
            return false;
        }

        var metadata = index.As<OpenSearchIndexMetadata>();

        var response = await _client.DeleteByQueryAsync<Dictionary<string, object>>(d => d
            .Index(index.IndexFullName)
            .Query(q => q
                .Bool(b => b
                    .Filter(f => f
                        .Terms(t => t
                            .Field(metadata.IndexMappings.KeyFieldName)
                            .Terms(ids.ToArray())
                        )
                    )
                )
            )
        );

        if (!response.IsValid)
        {
            if (response.OriginalException != null)
            {
                _logger.LogError(response.OriginalException, "There were issues deleting documents in an OpenSearch index");
            }
            else
            {
                _logger.LogWarning("There were issues deleting documents in an OpenSearch index");
            }
        }

        return response.IsValid;
    }

    public IContentIndexSettings GetContentIndexSettings()
         => new OpenSearchContentIndexSettings();

    public async Task<long> GetLastTaskIdAsync(IndexProfile index)
    {
        ArgumentNullException.ThrowIfNull(index);

        var mappings = await GetIndexMappingsAsync(index.IndexFullName);

        if (mappings?.Meta is null)
        {
            return 0;
        }

        if (!mappings.Meta.TryGetValue(OpenSearchConstants.LastTaskIdMetadataKey, out var lastTaskIdObject))
        {
            return 0;
        }

        if (lastTaskIdObject is JsonElement element && element.ValueKind == JsonValueKind.Number && element.TryGetInt64(out var lastTaskId))
        {
            return lastTaskId;
        }

        if (long.TryParse(lastTaskIdObject?.ToString(), out var parsed))
        {
            return parsed;
        }

        return 0;
    }

    public async Task<bool> AddOrUpdateDocumentsAsync(IndexProfile index, IEnumerable<DocumentIndex> documents)
    {
        ArgumentNullException.ThrowIfNull(index);
        ArgumentNullException.ThrowIfNull(documents);

        if (!documents.Any())
        {
            return false;
        }

        var totalBatchesProcessed = 0;
        var totalBatchesSucceeded = 0;

        foreach (var batch in documents.PagesOf(2500))
        {
            totalBatchesProcessed++;

            var bulkDescriptor = new BulkDescriptor().Index(index.IndexFullName).Refresh(global::OpenSearch.Net.Refresh.True);

            foreach (var document in batch)
            {
                var doc = CreateOpenSearchDocument(document);
                var docId = document.Id;
                bulkDescriptor.Index<Dictionary<string, object>>(i => i.Document(doc).Id(docId));
            }

            var response = await _client.BulkAsync(bulkDescriptor);

            if (response.IsValid && !response.Errors)
            {
                totalBatchesSucceeded++;
                continue;
            }

            if (response.Errors && response.ItemsWithErrors != null)
            {
                foreach (var itemWithError in response.ItemsWithErrors)
                {
                    _logger.LogError(
                        "OpenSearch failed to index document {DocumentId} in index {Index}. Error: {ErrorType} - {Reason}",
                        itemWithError.Id,
                        index.IndexFullName,
                        itemWithError.Error?.Type,
                        itemWithError.Error?.Reason);
                }
            }

            if (!response.IsValid)
            {
                if (response.OriginalException != null)
                {
                    _logger.LogError(response.OriginalException, "Bulk indexing request failed for index {Index}", index.IndexFullName);
                    return false;
                }

                _logger.LogError("Bulk indexing request failed for index {Index} without exception", index.IndexFullName);
                return false;
            }
        }

        return totalBatchesProcessed == totalBatchesSucceeded;
    }

    internal async Task<ITypeMapping> GetIndexMappingsAsync(string indexFullName)
    {
        ArgumentException.ThrowIfNullOrEmpty(indexFullName);

        var response = await _client.Indices.GetMappingAsync(new GetMappingRequest(indexFullName));

        if (!response.IsValid)
        {
            if (response.OriginalException != null)
            {
                _logger.LogError(response.OriginalException, "There were issues retrieving index mappings from OpenSearch");
            }
            else
            {
                _logger.LogWarning("There were issues retrieving index mappings from OpenSearch");
            }

            return null;
        }

        return response.GetMappingFor(indexFullName);
    }

    private static Dictionary<string, object> CreateOpenSearchDocument(DocumentIndex documentIndex)
    {
        var entries = new Dictionary<string, object>();

        if (documentIndex is ContentItemDocumentIndex doc)
        {
            entries.Add(ContentIndexingConstants.ContentItemIdKey, doc.ContentItemId);
            entries.Add(ContentIndexingConstants.ContentItemVersionIdKey, doc.ContentItemVersionId);
        }

        foreach (var entry in documentIndex.Entries)
        {
            switch (entry.Type)
            {
                case DocumentIndex.Types.Boolean:
                    if (entry.Value is bool boolValue)
                    {
                        AddValue(entries, entry.Name, boolValue);
                    }
                    break;

                case DocumentIndex.Types.DateTime:
                    if (entry.Value is DateTimeOffset offsetValue)
                    {
                        AddValue(entries, entry.Name, offsetValue);
                    }
                    else if (entry.Value is DateTime dateTimeValue)
                    {
                        AddValue(entries, entry.Name, dateTimeValue.ToUniversalTime());
                    }
                    break;

                case DocumentIndex.Types.Integer:
                    if (entry.Value != null && long.TryParse(entry.Value.ToString(), out var value))
                    {
                        AddValue(entries, entry.Name, value);
                    }
                    break;

                case DocumentIndex.Types.Number:
                    if (entry.Value != null)
                    {
                        AddValue(entries, entry.Name, Convert.ToDouble(entry.Value));
                    }
                    break;

                case DocumentIndex.Types.Complex:
                    entries[entry.Name] = entry.Value;
                    break;

                case DocumentIndex.Types.Vector:
                    entries[entry.Name] = entry.Value;
                    break;

                case DocumentIndex.Types.Text:
                    if (entry.Value != null)
                    {
                        var stringValue = Convert.ToString(entry.Value);
                        if (!string.IsNullOrEmpty(stringValue))
                        {
                            AddValue(entries, entry.Name, stringValue);
                        }
                    }
                    break;

                case DocumentIndex.Types.GeoPoint:
                    if (entry.Value is DocumentIndex.GeoPoint point)
                    {
                        AddValue(entries, entry.Name, new GeoLocation((double)point.Latitude, (double)point.Longitude));
                    }
                    break;
            }
        }

        return entries;
    }

    private static void AddValue(Dictionary<string, object> entries, string key, object value)
    {
        if (value is null)
        {
            return;
        }

        if (entries.TryAdd(key, value))
        {
            return;
        }

        if (entries[key] is List<object> list)
        {
            list.Add(value);
            entries[key] = list;
            return;
        }

        var values = new List<object>
        {
            entries[key],
            value,
        };

        entries[key] = values;
    }

    public async Task SetLastTaskIdAsync(IndexProfile index, long lastTaskId)
    {
        ArgumentNullException.ThrowIfNull(index);

        var indexingState = new Dictionary<string, object>
        {
            { OpenSearchConstants.LastTaskIdMetadataKey, lastTaskId },
        };

        var putMappingRequest = new PutMappingRequest(index.IndexFullName)
        {
            Meta = indexingState,
        };

        var response = await _client.Indices.PutMappingAsync(putMappingRequest);

        if (!response.IsValid)
        {
            if (response.OriginalException != null)
            {
                _logger.LogError(response.OriginalException, "There were issues updating mappings in an OpenSearch index");
            }
            else
            {
                _logger.LogWarning("There were issues updating mappings in an OpenSearch index");
            }
        }
    }
}
