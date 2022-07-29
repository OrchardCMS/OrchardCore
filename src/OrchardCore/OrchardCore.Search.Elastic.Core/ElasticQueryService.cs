using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Nest;
using Newtonsoft.Json.Linq;

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
            var queryProp = queryObj["query"] as JObject;
            var sourceProp = queryObj["_source"] as JValue;
            var sortProp = queryObj["sort"] as JObject;
            var sizeProp = queryObj["size"] as JValue;
            var fromProp = queryObj["from"] as JValue;

            if (queryProp == null)
            {
                throw new ArgumentException("Query DSL requires a [query] property");
            }

            var elasticTopDocs = new ElasticTopDocs();
            if (_elasticClient == null)
            {
                _logger.LogWarning("Elastic Client is not setup, please validate your Elastic Configurations");
            }

            try
            {
                queryProp.TryGetValue("_source", out var source);

                var sortDescriptor = new SortDescriptor<Dictionary<string, object>>();

                var searchDescriptor = new SearchDescriptor<Dictionary<string,object>>(context.IndexName)
                    .Source(sourceProp != null ? ((bool)sourceProp) : true)
                    .From(((int?)fromProp))
                    .Size((int?)sizeProp)
                    //.Sort(s => new SortDescriptor(s => s.))
                    .Query(q => new RawQuery(queryProp.ToString()));

                var searchResponse = await _elasticClient.SearchAsync<Dictionary<string, object>>(searchDescriptor);
                if (searchResponse.IsValid)
                {
                    elasticTopDocs.Count = searchResponse.Total;
                    elasticTopDocs.TopDocs = searchResponse.Documents.ToList();
                }
                else
                {
                    _logger.LogError($"Received failure response from Elastic: { searchResponse.ServerError }");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error while querying elastic with exception: { ex.Message}");
            }
            return elasticTopDocs;
        }
    }
}
