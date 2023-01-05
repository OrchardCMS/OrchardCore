using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Elasticsearch.Net;
using Microsoft.Extensions.Logging;
using Nest;
using OrchardCore.Contents.Indexing;
using OrchardCore.Environment.Shell;
using OrchardCore.Indexing;
using OrchardCore.Modules;
using OrchardCore.Search.Elasticsearch.Core.Mappings;
using OrchardCore.Search.Elasticsearch.Core.Models;

namespace OrchardCore.Search.Elasticsearch.Core.Services
{
    /// <summary>
    /// Provides methods to manage Elasticsearch indices.
    /// </summary>
    public class ElasticIndexManager : IDisposable
    {
        private readonly IElasticClient _elasticClient;
        private readonly ElasticAnalyzerManager _elasticAnalyzerManager;
        private readonly string _indexPrefix;
        private readonly IClock _clock;
        private readonly ILogger _logger;
        private bool _disposing;
        private readonly ConcurrentDictionary<string, DateTime> _timestamps = new(StringComparer.OrdinalIgnoreCase);
        private readonly string _lastTaskId = "last_task_id";
        private readonly string[] IgnoredFields = Array.Empty<string>();

        public ElasticIndexManager(
            IElasticClient elasticClient,
            ShellSettings shellSettings,
            ElasticAnalyzerManager elasticAnalyzerManager,
            IClock clock,
            ILogger<ElasticIndexManager> logger
            )
        {
            _elasticClient = elasticClient;
            _indexPrefix = shellSettings.Name.ToLowerInvariant() + "_";
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
        /// <returns><see cref="Boolean"/></returns>
        public async Task<bool> CreateIndexAsync(ElasticIndexSettings elasticIndexSettings)
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
                    analyzersDescriptor = new AnalyzersDescriptor();
                    analysisDescriptor.Analyzers(a => a.UserDefined(elasticIndexSettings.AnalyzerName, currentAnalyzer));

                    indexSettingsDescriptor = new IndexSettingsDescriptor();
                    indexSettingsDescriptor.Analysis(an => analysisDescriptor);
                }

                // Custom metadata to store the last indexing task id
                var IndexingState = new FluentDictionary<string, object>() {
                    { _lastTaskId, 0 }
                };

                var createIndexDescriptor = new CreateIndexDescriptor(_indexPrefix + elasticIndexSettings.IndexName)
                    .Settings(s => indexSettingsDescriptor)
                    .Map(m => m
                        .SourceField(s => s
                            .Enabled(elasticIndexSettings.StoreSourceData)
                            .Excludes(new string[] { IndexingConstants.DisplayTextAnalyzedKey }))
                        .Meta(me => IndexingState));

                var response = await _elasticClient.Indices.CreateAsync(createIndexDescriptor);

                // We force some mappings for common fields.
                await _elasticClient.MapAsync<string>(p => p
                    .Index(_indexPrefix + elasticIndexSettings.IndexName)
                    .Properties(p => p
                        .Keyword(obj => obj
                            .Name(IndexingConstants.ContentItemIdKey)
                        )
                        .Keyword(obj => obj
                            .Name(IndexingConstants.ContentItemVersionIdKey)
                        )
                        .Keyword(obj => obj
                            .Name(IndexingConstants.OwnerKey)
                        )
                        .Text(obj => obj
                            .Name(IndexingConstants.FullTextKey)
                        )
                    ));

                // ContainedPart mappings
                await _elasticClient.MapAsync<ContainedPartModel>(p => p
                    .Index(_indexPrefix + elasticIndexSettings.IndexName)
                    .Properties(p => p
                        .Object<ContainedPartModel>(obj => obj
                            .Name(IndexingConstants.ContainedPartKey)
                            .AutoMap()
                        )
                    ));

                // We map DisplayText here because we have 3 different fields with it.
                // We can't have Content.ContentItem.DisplayText as it is mapped as an Object in Elasticsearch.
                await _elasticClient.MapAsync<DisplayTextModel>(p => p
                    .Index(_indexPrefix + elasticIndexSettings.IndexName)
                    .Properties(p => p
                        .Object<DisplayTextModel>(obj => obj
                            .Name(IndexingConstants.DisplayTextKey)
                            .AutoMap()
                        )
                    ));

                // We map ContentType as a keyword because else the automatic mapping will break the queries.
                // We need to access it with Content.ContentItem.ContentType as a keyword
                // for the ContentPickerResultProvider(s).
                await _elasticClient.MapAsync<string>(p => p
                    .Index(_indexPrefix + elasticIndexSettings.IndexName)
                    .Properties(p => p
                        .Keyword(obj => obj
                            .Name(IndexingConstants.ContentTypeKey)
                        )
                    ));

                // DynamicTemplates mapping for Taxonomy indexing mostly.
                await _elasticClient.MapAsync<string>(p => p
                    .Index(_indexPrefix + elasticIndexSettings.IndexName)
                    .DynamicTemplates(d => d
                        .DynamicTemplate("*.Inherited", dyn => dyn
                            .MatchMappingType("string")
                            .PathMatch("*" + IndexingConstants.InheritedKey)
                            .Mapping(m => m
                                .Keyword(k => k))
                        )
                        .DynamicTemplate("*.Ids", dyn => dyn
                            .MatchMappingType("string")
                            .PathMatch("*" + IndexingConstants.IdsKey)
                            .Mapping(m => m
                                .Keyword(k => k))
                            )
                        )
                    );

                return response.Acknowledged;
            }
            else
            {
                return true;
            }
        }

        public async Task<string> GetIndexMappings(string indexName)
        {
            var response = await _elasticClient.LowLevel.Indices.GetMappingAsync<StringResponse>(_indexPrefix + indexName);
            return response.Body;
        }

        /// <summary>
        /// Store a last_task_id in the Elasticsearch index _meta mappings.
        /// This allows storing the last indexing task id executed on the Elasticsearch index.
        /// <see href="https://www.elastic.co/guide/en/elasticsearch/reference/current/mapping-meta-field.html"/>
        /// </summary>
        public async Task SetLastTaskId(string indexName, int lastTaskId)
        {
            var IndexingState = new FluentDictionary<string, object>() {
                { _lastTaskId, lastTaskId }
            };

            var putMappingRequest = new PutMappingRequest(_indexPrefix + indexName)
            {
                Meta = IndexingState
            };

            await _elasticClient.Indices.PutMappingAsync(putMappingRequest);
        }

        /// <summary>
        /// Get a last_task_id in the Elasticsearch index _meta mappings.
        /// This allows retrieving the last indexing task id executed on the index.
        /// </summary>
        public async Task<int> GetLastTaskId(string indexName)
        {
            var jsonDocument = JsonDocument.Parse(await GetIndexMappings(indexName));
            jsonDocument.RootElement.TryGetProperty(_indexPrefix + indexName, out var jsonElement);
            jsonElement.TryGetProperty("mappings", out var mappings);
            mappings.TryGetProperty("_meta", out var meta);
            meta.TryGetProperty(_lastTaskId, out var lastTaskId);
            lastTaskId.TryGetInt32(out var intValue);

            return intValue;
        }

        public async Task<bool> DeleteDocumentsAsync(string indexName, IEnumerable<string> contentItemIds)
        {
            var success = true;

            if (contentItemIds.Any())
            {
                var descriptor = new BulkDescriptor();

                foreach (var id in contentItemIds)
                    descriptor.Delete<Dictionary<string, object>>(d => d
                        .Index(_indexPrefix + indexName)
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

        /// <summary>
        /// Deletes all documents in an index in one request.
        /// <see href="https://www.elastic.co/guide/en/elasticsearch/reference/master/docs-delete-by-query.html"/>
        /// </summary>
        public async Task<bool> DeleteAllDocumentsAsync(string indexName)
        {
            var response = await _elasticClient.DeleteByQueryAsync<Dictionary<string, object>>(del => del
                .Index(_indexPrefix + indexName)
                .Query(q => q.MatchAll())
            );

            return response.IsValid;
        }

        public async Task<bool> DeleteIndex(string indexName)
        {
            if (await Exists(indexName))
            {
                var result = await _elasticClient.Indices.DeleteAsync(_indexPrefix + indexName);
                return result.Acknowledged;
            }
            else
            {
                return true;
            }
        }

        /// <summary>
        /// Verify if an index exists for the current tenant.
        /// </summary>
        public async Task<bool> Exists(string indexName)
        {
            if (String.IsNullOrWhiteSpace(_indexPrefix + indexName))
            {
                return false;
            }

            var existResponse = await _elasticClient.Indices.ExistsAsync(_indexPrefix + indexName);

            return existResponse.Exists;
        }

        /// <summary>
        /// Makes sure that the index names are compliant with Elasticsearch specifications.
        /// <see href="https://www.elastic.co/guide/en/elasticsearch/reference/current/indices-create-index.html#indices-create-api-path-params"/>
        /// </summary>
        public static string ToSafeIndexName(string indexName)
        {
            indexName = indexName.ToLowerInvariant();

            if (indexName[0] == '-' || indexName[0] == '_' || indexName[0] == '+' || indexName[0] == '.')
            {
                indexName = indexName.Remove(0, 1);
            }

            var charsToRemove = new List<char>();
            charsToRemove.AddRange(new List<char>(){ 
                '\\',
                '/',
                '*',
                '\"',
                '|',
                '<',
                '>',
                '`',
                '\'',
                ' ',
                '#',
                ':',
                '.',
            });

            charsToRemove.ForEach(c => indexName = indexName.Replace(c.ToString(), String.Empty));
            
            return indexName;
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
                        .Index(_indexPrefix + indexName)
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
        /// <returns><see cref="ElasticTopDocs"/></returns>
        public async Task<ElasticTopDocs> SearchAsync(string indexName, QueryContainer query, List<ISort> sort, int from, int size)
        {
            var elasticTopDocs = new ElasticTopDocs();

            if (await Exists(indexName))
            {
                var searchRequest = new SearchRequest(_indexPrefix + indexName)
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

                _timestamps[_indexPrefix + indexName] = _clock.UtcNow;
            }

            return elasticTopDocs;
        }

        /// <summary>
        /// Returns results from a search made with NEST Fluent DSL query.
        /// </summary>
        public async Task SearchAsync(string indexName, Func<IElasticClient, Task> elasticClient)
        {
            if (await Exists(indexName))
            {
                await elasticClient(_elasticClient);

                _timestamps[_indexPrefix + indexName] = _clock.UtcNow;
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

        ~ElasticIndexManager()
        {
            Dispose();
        }
    }
}
