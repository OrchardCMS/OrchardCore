using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Elasticsearch.Net;
using Microsoft.Extensions.Logging;
using Nest;
using OrchardCore.Indexing;
using OrchardCore.Modules;
using OrchardCore.Search.Elasticsearch.Model;
using OrchardCore.Search.Elasticsearch.Services;

namespace OrchardCore.Search.Elasticsearch
{
    /// <summary>
    /// Provides methods to manage physical Elasticsearch indices.
    /// </summary>
    public class ElasticsearchIndexManager : IDisposable
    {
        private readonly IElasticClient _elasticClient;
        private readonly ElasticsearchAnalyzerManager _elasticAnalyzerManager;
        private readonly IClock _clock;
        private readonly ILogger _logger;
        private bool _disposing;
        private readonly ConcurrentDictionary<string, DateTime> _timestamps = new(StringComparer.OrdinalIgnoreCase);

        private readonly string[] IgnoredFields = Array.Empty<string>();

        public ElasticsearchIndexManager(
            IElasticClient elasticClient,
            ElasticsearchAnalyzerManager elasticAnalyzerManager,
            IClock clock,
            ILogger<ElasticsearchIndexManager> logger
            )
        {
            _elasticClient = elasticClient;
            _elasticAnalyzerManager = elasticAnalyzerManager;
            _clock = clock;
            _logger = logger;
        }

        /// <summary>
        /// <para>Creates an Elasticsearch index with _source mapping.</para>
        /// <para><see href="https://www.elastic.co/guide/en/elasticsearch/reference/8.3/mapping-source-field.html#disable-source-field"/></para>
        /// <para>Specify an analyzer for an index based on the ElasticsearchIndexSettings
        /// <see href="https://www.elastic.co/guide/en/elasticsearch/reference/current/specify-analyzer.html#specify-index-time-default-analyzer"/>
        /// <see href="https://www.elastic.co/guide/en/elasticsearch/reference/master/analysis-analyzers.html"/>
        /// </para>
        /// </summary>
        /// <param name="elasticIndexSettings"></param>
        /// <returns><see cref="Boolean"/></returns>
        public async Task<bool> CreateIndexAsync(ElasticsearchIndexSettings elasticIndexSettings)
        {
            //Get Index name scoped by ShellName
            if (!await Exists(elasticIndexSettings.IndexName))
            {
                var analysisDescriptor = new AnalysisDescriptor();
                var analyzersDescriptor = new AnalyzersDescriptor();
                var indexSettingsDescriptor = new IndexSettingsDescriptor();

                var analyzers = _elasticAnalyzerManager.GetAnalyzers();

                if (analyzers.Any())
                {
                    var currentAnalyzer = analyzers.FirstOrDefault(a => a.Name == elasticIndexSettings.AnalyzerName).CreateAnalyzer();
                    var currentAnalyzerType = currentAnalyzer.Type;

                    analyzersDescriptor = new AnalyzersDescriptor();
                    analysisDescriptor.Analyzers(a => a.UserDefined(elasticIndexSettings.AnalyzerName, currentAnalyzer));

                    indexSettingsDescriptor = new IndexSettingsDescriptor();
                    indexSettingsDescriptor.Analysis(an => analysisDescriptor);
                }

                var createIndexDescriptor = new CreateIndexDescriptor(elasticIndexSettings.IndexName)
                    .Settings(s => indexSettingsDescriptor)
                    .Map(m => m.SourceField(s => s.Enabled(elasticIndexSettings.StoreSourceData)));

                var response = await _elasticClient.Indices.CreateAsync(createIndexDescriptor);

                return response.Acknowledged;
            }
            else
            {
                return true;
            }
        }

        public async Task<string> GetIndexMappings(string indexName)
        {
            var response = await _elasticClient.LowLevel.Indices.GetMappingAsync<StringResponse>(indexName);
            return response.Body;
        }

        public async Task<bool> DeleteDocumentsAsync(string indexName, IEnumerable<string> contentItemIds)
        {
            var success = true;

            if (contentItemIds.Any())
            {
                var descriptor = new BulkDescriptor();

                foreach (var id in contentItemIds)
                    descriptor.Delete<Dictionary<string, object>>(d => d
                        .Index(indexName)
                        .Id(id)
                    );

                var response = await _elasticClient.BulkAsync(descriptor);

                if (response.Errors)
                {
                    _logger.LogWarning("There were issues deleting documents from Elasticsearch. {result.OriginalException}", response.OriginalException);
                }

                success = response.IsValid;
            }

            return success;
        }

        public async Task<bool> DeleteIndex(string indexName)
        {
            if (await Exists(indexName))
            {
                var result = await _elasticClient.Indices.DeleteAsync(indexName);
                return result.Acknowledged;
            }
            else
            {
                return true;
            }
        }

        public async Task<bool> Exists(string indexName)
        {
            if (String.IsNullOrWhiteSpace(indexName))
            {
                return false;
            }

            var existResponse = await _elasticClient.Indices.ExistsAsync(indexName);

            return existResponse.Exists;
        }

        public async Task StoreDocumentsAsync(string indexName, IEnumerable<DocumentIndex> indexDocuments)
        {
            var documents = new List<Dictionary<string, object>>();

            foreach (var indexDocument in indexDocuments)
            {
                documents.Add(CreateElasticDocument(indexDocument));
            }

            if (documents.Any())
            {
                var descriptor = new BulkDescriptor();

                foreach (var document in documents)
                {
                    descriptor.Index<Dictionary<string, object>>(op => op
                        .Id(document.GetValueOrDefault("ContentItemId").ToString())
                        .Document(document)
                        .Index(indexName)
                    );
                }

                var result = await _elasticClient.BulkAsync(d => descriptor);

                if (result.Errors)
                {
                    _logger.LogWarning("There were issues reported indexing the documents. {result.ServerError}", result.ServerError);
                }
            }
        }

        /// <summary>
        /// Returns results from a search made with a NEST QueryContainer query.
        /// </summary>
        /// <param name="indexName"></param>
        /// <param name="query"></param>
        /// <param name="sort"></param>
        /// <param name="from"></param>
        /// <param name="size"></param>
        /// <returns><see cref="ElasticsearchTopDocs"/></returns>
        public async Task<ElasticsearchTopDocs> SearchAsync(string indexName, QueryContainer query, List<ISort> sort, int from, int size)
        {
            var elasticTopDocs = new ElasticsearchTopDocs();

            if (await Exists(indexName))
            {
                var searchRequest = new SearchRequest(indexName)
                {
                    Query = query,
                    From = from,
                    Size = size,
                    Sort = sort
                };

                var searchResponse = await _elasticClient.SearchAsync<Dictionary<string, object>>(searchRequest);

                if (searchResponse.IsValid)
                {
                    elasticTopDocs.Count = searchResponse.Documents.Count;
                    elasticTopDocs.TopDocs = searchResponse.Documents.ToList();
                }

                _timestamps[indexName] = _clock.UtcNow;
            }

            return elasticTopDocs;
        }

        /// <summary>
        /// Returns results from a search made with NEST Fluent DSL query.
        /// </summary>
        /// <param name="indexName"></param>
        /// <param name="elasticClient"></param>
        public async Task SearchAsync(string indexName, Func<IElasticClient, Task> elasticClient)
        {
            if (await Exists(indexName))
            {
                await elasticClient(_elasticClient);

                _timestamps[indexName] = _clock.UtcNow;
            }
        }

        private Dictionary<string, object> CreateElasticDocument(DocumentIndex documentIndex)
        {
            var entries = new Dictionary<string, object>
            {
                { "ContentItemId", documentIndex.ContentItemId },
                { "ContentItemVersionId", documentIndex.ContentItemVersionId }
            };

            foreach (var entry in documentIndex.Entries)
            {
                if (entries.ContainsKey(entry.Name)
                    || Array.Exists(IgnoredFields, x => entry.Name.Contains(x)))
                {
                    continue;
                }

                switch (entry.Type)
                {
                    case DocumentIndex.Types.Boolean:
                        // store "true"/"false" for booleans
                        entries.Add(entry.Name, (bool)(entry.Value));
                        break;

                    case DocumentIndex.Types.DateTime:
                        if (entry.Value != null)
                        {
                            if (entry.Value is DateTimeOffset)
                            {
                                entries.Add(entry.Name, ((DateTimeOffset)(entry.Value)).UtcDateTime);
                            }
                            else
                            {
                                entries.Add(entry.Name, ((DateTime)(entry.Value)).ToUniversalTime());
                            }
                        }
                        break;

                    case DocumentIndex.Types.Integer:
                        if (entry.Value != null && Int64.TryParse(entry.Value.ToString(), out var value))
                        {
                            entries.Add(entry.Name, value);
                        }

                        break;

                    case DocumentIndex.Types.Number:
                        if (entry.Value != null)
                        {
                            entries.Add(entry.Name, Convert.ToDouble(entry.Value));
                        }
                        break;

                    case DocumentIndex.Types.Text:
                        if (entry.Value != null && !String.IsNullOrEmpty(Convert.ToString(entry.Value)))
                        {
                            entries.Add(entry.Name, Convert.ToString(entry.Value));
                        }
                        break;
                }

            }
            return entries;
        }

        public void Dispose()
        {
            if (_disposing)
            {
                return;
            }

            _disposing = true;
        }

        ~ElasticsearchIndexManager()
        {
            Dispose();
        }
    }
}
