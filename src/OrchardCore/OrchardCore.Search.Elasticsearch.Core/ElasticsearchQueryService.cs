using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Nest;
using Newtonsoft.Json.Linq;
using OrchardCore.Search.Elasticsearch.Model;

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

        public async Task<ElasticsearchTopDocs> SearchAsync(ElasticsearchQueryContext context, JObject queryObj)
        {
            var queryOptions = JsonSerializer.Deserialize<QueryOptionsModel>(queryObj.ToString());

            var queryProp = queryObj["query"] as JObject;

            if (queryProp == null)
            {
                throw new ArgumentException("Query DSL requires a [query] property");
            }

            var sortProperty = queryObj["sort"];
            var sortFields = new List<FieldSort>();
            string sortField = null;
            string sortOrder = null;

            if (sortProperty != null)
            {
                //string sortType;

                if (sortProperty.Type == JTokenType.String)
                {
                    sortField = sortProperty.ToString();
                    sortFields.Add(new FieldSort { Field = sortField, Order = SortOrder.Descending, UnmappedType = FieldType.Text });
                }
                else if (sortProperty.Type == JTokenType.Object)
                {
                    sortField = ((JProperty)sortProperty.First).Name;
                    sortOrder = ((JProperty)sortProperty.First).Value["order"].ToString();
                    //sortType = ((JProperty)sortProperty.First).Value["type"]?.ToString();

                    var sortFieldType = FieldType.Text;

                    //if (sortType != null)
                    //{
                    //    var fieldTypeArray = Enum.GetValues(typeof(FieldType));
                    //    var myFieldType = fieldTypeArray.GetValue(0);
                    //    //sortFieldType = fieldTypeArray.Single(sortField.ToLower());
                    //}

                    var sortOrderType = SortOrder.Descending;

                    if (sortOrder != null)
                    {
                        if (sortOrder == "asc")
                        {
                            sortOrderType = SortOrder.Ascending;
                        }
                    }

                    sortFields.Add(new FieldSort { Field = sortField, Order = sortOrderType, UnmappedType = sortFieldType });
                }
                else if (sortProperty.Type == JTokenType.Array)
                {
                    foreach (var item in sortProperty.Children())
                    {
                        sortField = ((JProperty)item.First).Name;
                        sortOrder = ((JProperty)item.First).Value["order"].ToString();
                        //sortType = ((JProperty)item.First).Value["type"]?.ToString();

                        var sortFieldType = FieldType.Text;

                        //if (sortType != null)
                        //{
                        //    sortFieldType = (FieldType)Enum.Parse(typeof(FieldType), sortType.ToLower());
                        //}

                        var sortOrderType = SortOrder.Descending;

                        if (sortOrder != null)
                        {
                            if (sortOrder == "asc")
                            {
                                sortOrderType = SortOrder.Ascending;
                            }
                        }

                        sortFields.Add(new FieldSort { Field = sortField, Order = sortOrderType, UnmappedType = sortFieldType });
                    }
                }
            }

            var fieldNames = new List<string>();

            var elasticTopDocs = new ElasticsearchTopDocs();
            if (_elasticClient == null)
            {
                _logger.LogWarning("Elasticsearch Client is not setup, please validate your Elasticsearch Configurations");
            }

            try
            {
                queryProp.TryGetValue("_source", out var source);

                var sortDescriptor = new SortDescriptor<Dictionary<string, object>>();

                foreach (var sortFieldItem in sortFields)
                {
                    sortDescriptor.Field(sortFieldItem.Field, (SortOrder)sortFieldItem.Order);
                }

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
