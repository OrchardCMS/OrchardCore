using System.Text;
using Elastic.Clients.Elasticsearch;
using Microsoft.Extensions.Logging;

namespace OrchardCore.Search.Elasticsearch.Core.Services;

public class ElasticQueryService : IElasticQueryService
{
    private readonly ElasticsearchClient _elasticClient;
    private readonly ElasticIndexManager _elasticIndexManager;
    private readonly ILogger _logger;

    public ElasticQueryService(
        ElasticsearchClient elasticClient,
        ElasticIndexManager elasticIndexManager,
        ILogger<ElasticQueryService> logger
        )
    {
        _elasticClient = elasticClient;
        _elasticIndexManager = elasticIndexManager;
        _logger = logger;
    }

    public async Task<ElasticTopDocs> SearchAsync(string indexName, string query)
    {
        ArgumentException.ThrowIfNullOrEmpty(indexName);

        var elasticTopDocs = new ElasticTopDocs();

        if (_elasticClient == null)
        {
            _logger.LogWarning("Elasticsearch Client is not setup, please validate your Elasticsearch Configurations");

            return elasticTopDocs;
        }

        try
        {
            using var stream = new MemoryStream(Encoding.UTF8.GetBytes(query));
            var deserializedSearchRequest = _elasticClient.RequestResponseSerializer.Deserialize<SearchRequest>(stream);

            var searchRequest = new SearchRequest(_elasticIndexManager.GetFullIndexName(indexName))
            {
                Query = deserializedSearchRequest.Query,
                From = deserializedSearchRequest.From,
                Size = deserializedSearchRequest.Size,
                Fields = deserializedSearchRequest.Fields,
                Sort = deserializedSearchRequest.Sort,
                Source = deserializedSearchRequest.Source,
            };

            var searchResponse = await _elasticClient.SearchAsync<Dictionary<string, object>>(searchRequest);
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
