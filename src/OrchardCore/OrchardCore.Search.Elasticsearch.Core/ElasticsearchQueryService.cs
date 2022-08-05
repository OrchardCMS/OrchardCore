using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Nest;
using Newtonsoft.Json.Linq;

namespace OrchardCore.Search.Elasticsearch
{
    public class ElasticsearchQueryService : IElasticsearchQueryService
    {
        private readonly IEnumerable<IElasticsearchQueryProvider> _queryProviders;
        private readonly IElasticClient _elasticClient;
        private readonly ILogger<ElasticsearchQueryService> _logger;

        public ElasticsearchQueryService(
            IEnumerable<IElasticsearchQueryProvider> queryProviders,
            IElasticClient elasticClient,
            ILogger<ElasticsearchQueryService> logger
            )
        {
            _queryProviders = queryProviders;
            _elasticClient = elasticClient;
            _logger = logger;
        }

        public async Task<ElasticsearchTopDocs> SearchAsync(ElasticsearchQueryContext context, string query)
        {
            var elasticTopDocs = new ElasticsearchTopDocs();

            if (_elasticClient == null)
            {
                _logger.LogWarning("Elasticsearch Client is not setup, please validate your Elasticsearch Configurations");
            }

            try
            {
                var deserializedSearchRequest = _elasticClient.RequestResponseSerializer.Deserialize<SearchRequest>(new MemoryStream(Encoding.UTF8.GetBytes(query)));

                var searchRequest = new SearchRequest(context.IndexName)
                {
                    Query = deserializedSearchRequest.Query,
                    From = deserializedSearchRequest.From,
                    Size = deserializedSearchRequest.Size,
                    Fields = deserializedSearchRequest.Fields,
                    Sort = deserializedSearchRequest.Sort,
                    Source = deserializedSearchRequest.Source
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
                            row[keyValuePair.Key] = keyValuePair.Value.As<string[]>();
                        }

                        hits.Add(row);
                    }
                }

                if (searchResponse.IsValid)
                {
                    elasticTopDocs.Count = searchResponse.Total;
                    elasticTopDocs.TopDocs = searchResponse.Documents.ToList();
                    elasticTopDocs.Fields = hits;
                }
                else
                {
                    _logger.LogError("Received failure response from Elasticsearch: {ServerError}", searchResponse.ServerError);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while querying elastic with exception: {Message}", ex.Message);
            }

            return elasticTopDocs;
        }
    }
}
