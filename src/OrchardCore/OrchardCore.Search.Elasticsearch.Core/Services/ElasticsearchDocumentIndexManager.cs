using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.Fluent;
using Elastic.Clients.Elasticsearch.IndexManagement;
using Elastic.Clients.Elasticsearch.Mapping;
using Elastic.Clients.Elasticsearch.QueryDsl;
using Microsoft.Extensions.Logging;
using OrchardCore.Contents.Indexing;
using OrchardCore.Entities;
using OrchardCore.Indexing;
using OrchardCore.Indexing.Models;
using OrchardCore.Search.Elasticsearch.Core.Models;

namespace OrchardCore.Search.Elasticsearch.Core.Services;

public sealed class ElasticsearchDocumentIndexManager : IDocumentIndexManager
{
    private readonly ElasticsearchClient _client;
    private readonly ILogger _logger;

    public ElasticsearchDocumentIndexManager(
        ElasticsearchClient client,
        ILogger<ElasticsearchDocumentIndexManager> logger)
    {
        _client = client;
        _logger = logger;
    }

    public async Task<bool> DeleteAllDocumentsAsync(IndexEntity index)
    {
        ArgumentNullException.ThrowIfNull(index);

        var response = await _client.DeleteByQueryAsync(index.IndexFullName, descriptor => descriptor
            .Query(q => q
                .MatchAll(new MatchAllQuery())
             )
        );

        if (!response.IsValidResponse)
        {
            if (response.TryGetOriginalException(out var ex))
            {
                _logger.LogError(ex, "There were issues deleting documents in an Elasticsearch index");
            }
            else
            {
                _logger.LogWarning("There were issues deleting documents in an Elasticsearch index");
            }
        }

        return response.IsValidResponse;
    }

    public async Task<bool> DeleteDocumentsAsync(IndexEntity index, IEnumerable<string> ids)
    {
        ArgumentNullException.ThrowIfNull(index);
        ArgumentNullException.ThrowIfNull(ids);

        if (!ids.Any())
        {
            return false;
        }

        var metadata = index.As<ElasticsearchIndexMetadata>();

        var response = await _client.DeleteByQueryAsync<Dictionary<string, object>>(index.IndexFullName, descriptor => descriptor
            .Query(q => q
                .Bool(b => b
                    .Filter(f => f
                        .Terms(t => t
                            .Field(metadata.IndexMappings.KeyFieldName)
                            .Terms(new TermsQueryField(ids.Select(id => FieldValue.String(id)).ToArray()))
                        )
                    )
                )
            )
        );

        if (!response.IsValidResponse)
        {
            if (response.TryGetOriginalException(out var ex))
            {
                _logger.LogError(ex, "There were issues deleting documents in an Elasticsearch index");
            }
            else
            {
                _logger.LogWarning("There were issues deleting documents in an Elasticsearch index");
            }
        }

        return response.IsValidResponse;
    }

    public IContentIndexSettings GetContentIndexSettings()
         => new ElasticContentIndexSettings();

    public async Task<long> GetLastTaskIdAsync(IndexEntity index)
    {
        ArgumentNullException.ThrowIfNull(index);

        var mappings = await GetIndexMappingsAsync(index.IndexFullName);

        if (mappings?.Meta is null)
        {
            return 0;
        }

        if (!mappings.Meta.TryGetValue(ElasticsearchConstants.LastTaskIdMetadataKey, out var lastTaskId))
        {
            return 0;
        }

        return Convert.ToInt64(lastTaskId);
    }

    public async Task<bool> MergeOrUploadDocumentsAsync(IndexEntity index, IEnumerable<DocumentIndexBase> documents)
    {
        ArgumentNullException.ThrowIfNull(index);
        ArgumentNullException.ThrowIfNull(documents);

        if (!documents.Any())
        {
            return false;
        }

        foreach (var batch in documents.PagesOf(2500))
        {
            var response = await _client.BulkAsync(index.IndexFullName, descriptor =>
            {
                descriptor.Refresh(Refresh.True);

                foreach (var document in batch)
                {
                    descriptor.Index(CreateElasticDocument(document), i => i.Id(document.Id));
                }
            });

            if (!response.IsValidResponse)
            {
                if (response.TryGetOriginalException(out var ex))
                {
                    _logger.LogError(ex, "There were issues indexing a document using Elasticsearch");
                }
                else
                {
                    _logger.LogWarning("There were issues indexing a document using Elasticsearch");
                }
            }
        }

        return true;
    }

    internal async Task<TypeMapping> GetIndexMappingsAsync(string indexFullName)
    {
        ArgumentException.ThrowIfNullOrEmpty(indexFullName);

        var response = await _client.Indices.GetMappingAsync<GetMappingResponse>(descriptor => descriptor
            .Indices(indexFullName)
        );

        if (!response.IsValidResponse)
        {
            if (response.TryGetOriginalException(out var ex))
            {
                _logger.LogError(ex, "There were issues retrieving index mappings from Elasticsearch");
            }
            else
            {
                _logger.LogWarning("There were issues retrieving index mappings from Elasticsearch");
            }

            return null;
        }

        return response.Indices[indexFullName].Mappings;
    }

    private static Dictionary<string, object> CreateElasticDocument(DocumentIndexBase documentIndex)
    {
        var entries = new Dictionary<string, object>();

        if (documentIndex is DocumentIndex doc)
        {
            entries.Add(ContentIndexingConstants.ContentItemIdKey, doc.ContentItemId);
            entries.Add(ContentIndexingConstants.ContentItemVersionIdKey, doc.ContentItemVersionId);
        }

        foreach (var entry in documentIndex.Entries)
        {
            switch (entry.Type)
            {
                case DocumentIndexBase.Types.Boolean:
                    if (entry.Value is bool boolValue)
                    {
                        AddValue(entries, entry.Name, boolValue);
                    }
                    break;

                case DocumentIndexBase.Types.DateTime:

                    if (entry.Value is DateTimeOffset offsetValue)
                    {
                        AddValue(entries, entry.Name, offsetValue);
                    }
                    else if (entry.Value is DateTime dateTimeValue)
                    {
                        AddValue(entries, entry.Name, dateTimeValue.ToUniversalTime());
                    }

                    break;

                case DocumentIndexBase.Types.Integer:
                    if (entry.Value != null && long.TryParse(entry.Value.ToString(), out var value))
                    {
                        AddValue(entries, entry.Name, value);
                    }

                    break;

                case DocumentIndexBase.Types.Number:
                    if (entry.Value != null)
                    {
                        AddValue(entries, entry.Name, Convert.ToDouble(entry.Value));
                    }
                    break;

                case DocumentIndexBase.Types.Text:
                    if (entry.Value != null)
                    {
                        var stringValue = Convert.ToString(entry.Value);

                        if (!string.IsNullOrEmpty(stringValue))
                        {
                            AddValue(entries, entry.Name, stringValue);
                        }
                    }
                    break;
                case DocumentIndexBase.Types.GeoPoint:
                    if (entry.Value is DocumentIndexBase.GeoPoint point)
                    {
                        AddValue(entries, entry.Name, GeoLocation.LatitudeLongitude(new LatLonGeoLocation
                        {
                            Lat = (double)point.Latitude,
                            Lon = (double)point.Longitude,
                        }));
                    }

                    break;
            }
        }

        return entries;
    }

    private static void AddValue(Dictionary<string, object> entries, string key, object value)
    {
        if (entries.TryAdd(key, value))
        {
            return;
        }

        // At this point, we know that a value already exists.
        if (entries[key] is List<object> list)
        {
            list.Add(value);

            entries[key] = list;

            return;
        }

        // Convert the existing value to a list of values.
        var values = new List<object>
        {
            entries[key],
            value,
        };

        entries[key] = values;
    }

    public async Task SetLastTaskIdAsync(IndexEntity index, long lastTaskId)
    {
        ArgumentNullException.ThrowIfNull(index);

        var IndexingState = new FluentDictionary<string, object>()
        {
            { ElasticsearchConstants.LastTaskIdMetadataKey, lastTaskId },
        };

        var putMappingRequest = new PutMappingRequest(index.IndexFullName)
        {
            Meta = IndexingState,
        };

        var response = await _client.Indices.PutMappingAsync(putMappingRequest);

        if (!response.IsValidResponse)
        {
            if (response.TryGetOriginalException(out var ex))
            {
                _logger.LogError(ex, "There were issues updating mappings in an Elasticsearch index");
            }
            else
            {
                _logger.LogWarning("There were issues updating mappings in an Elasticsearch index");
            }
        }
    }
}
