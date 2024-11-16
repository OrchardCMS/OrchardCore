using System.Text;
using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.QueryDsl;
using Microsoft.Extensions.Logging;
using OrchardCore.ContentManagement;

namespace OrchardCore.Search.Elasticsearch.Core.Services;

public class ElasticsearchQueryService
{
    private readonly ElasticsearchIndexManager _elasticIndexManager;
    private readonly ElasticsearchClient _client;
    private readonly ILogger _logger;

    public ElasticsearchQueryService(
        ElasticsearchIndexManager elasticIndexManager,
        ElasticsearchClient client,
        ILogger<ElasticsearchQueryService> logger)
    {
        _elasticIndexManager = elasticIndexManager;
        _client = client;
        _logger = logger;
    }

    public async Task<IList<string>> ExecuteQueryAsync(string indexName, Query query, List<SortOptions> sort, int from, int size)
    {
        var results = await _elasticIndexManager.SearchAsync(indexName, query, sort, from, size);

        if (results?.TopDocs is null || results.TopDocs.Count == 0)
        {
            return [];
        }

        var contentItemIds = new List<string>();

        foreach (var item in results.TopDocs)
        {
            contentItemIds.Add(item.GetValueOrDefault(nameof(ContentItem.ContentItemId)).ToString());
        }

        return contentItemIds;
    }

    public async Task<ElasticsearchTopDocs> SearchAsync(string indexName, string query)
    {
        ArgumentException.ThrowIfNullOrEmpty(indexName);

        var elasticTopDocs = new ElasticsearchTopDocs();

        try
        {
            using var stream = new MemoryStream(Encoding.UTF8.GetBytes(query));
            var deserializedSearchRequest = _client.RequestResponseSerializer.Deserialize<SearchRequest>(stream);

            var searchRequest = new SearchRequest(_elasticIndexManager.GetFullIndexName(indexName))
            {
                Query = deserializedSearchRequest.Query,
                From = deserializedSearchRequest.From,
                Size = deserializedSearchRequest.Size,
                Fields = deserializedSearchRequest.Fields,
                Sort = deserializedSearchRequest.Sort,
                Source = deserializedSearchRequest.Source,
            };

            var searchResponse = await _client.SearchAsync<Dictionary<string, object>>(searchRequest);

            var hits = new List<Dictionary<string, object>>();

            foreach (var hit in searchResponse.Hits)
            {
                if (hit.Fields != null)
                {
                    var row = new Dictionary<string, object>();

                    foreach (var keyValuePair in hit.Fields)
                    {
                        row[keyValuePair.Key] = keyValuePair.Value;
                    }

                    hits.Add(row);
                }
            }

            if (searchResponse.IsValidResponse)
            {
                elasticTopDocs.Count = searchResponse.Total;
                elasticTopDocs.TopDocs = new List<Dictionary<string, object>>(searchResponse.Documents);
                elasticTopDocs.Fields = hits;
            }
            else
            {
                _logger.LogError("Received failure response from Elasticsearch.");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while querying elastic with exception: {Message}", ex.Message);
        }

        return elasticTopDocs;
    }
}
