using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Nest;
using Newtonsoft.Json.Linq;
using OrchardCore.Search.Elastic.Model;

namespace OrchardCore.Search.Elastic
{
    public class ElasticQueryService : IElasticQueryService
    {
        private readonly IEnumerable<IElasticQueryProvider> _queryProviders;
        private readonly IElasticClient _elasticClient;
        private readonly ILogger<ElasticQueryService> _logger;

        public ElasticQueryService(
            IEnumerable<IElasticQueryProvider> queryProviders,
            IElasticClient elasticClient,
            ILogger<ElasticQueryService> logger
            )
        {
            _queryProviders = queryProviders;
            _elasticClient = elasticClient;
            _logger = logger;
        }

        public async Task<ElasticTopDocs> SearchAsync(ElasticQueryContext context, JObject queryObj)
        {
            var queryOptions = JsonSerializer.Deserialize<QueryOptionsModel>(queryObj.ToString());

            var queryProp = queryObj["query"] as JObject;

            if (queryProp == null)
            {
                throw new ArgumentException("Query DSL requires a [query] property");
            }

            var sortProp = queryObj["sort"] as JObject;

            var fieldNames = new List<string>();



            var elasticTopDocs = new ElasticTopDocs();
            if (_elasticClient == null)
            {
                _logger.LogWarning("Elastic Client is not setup, please validate your Elastic Configurations");
            }

            try
            {
                queryProp.TryGetValue("_source", out var source);

                var sortDescriptor = new SortDescriptor<Dictionary<string, object>>();

                var searchDescriptor = new SearchDescriptor<Dictionary<string, object>>(context.IndexName)
                    .Source((bool)(queryOptions.Source != null ? queryOptions.Source : false))
                    .From(queryOptions.From)
                    .Size(queryOptions.Size)
                    .Fields(queryOptions.Fields)
                    .Sort(s => sortDescriptor)
                    .Query(q => new RawQuery(queryProp.ToString()));

                var searchResponse = await _elasticClient.SearchAsync<Dictionary<string, object>>(searchDescriptor);
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
                    _logger.LogError("Received failure response from Elastic: {ServerError}", searchResponse.ServerError);
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
