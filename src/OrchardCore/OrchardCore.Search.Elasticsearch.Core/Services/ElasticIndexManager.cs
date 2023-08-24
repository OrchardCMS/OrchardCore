
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using Elasticsearch.Net;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
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
    public sealed class ElasticIndexManager : IDisposable
    {
        private readonly IElasticClient _elasticClient;
        private readonly ShellSettings _shellSettings;
        private readonly IClock _clock;
        private readonly ILogger _logger;
        private readonly ElasticsearchOptions _elasticsearchOptions;
        private readonly ConcurrentDictionary<string, DateTime> _timestamps = new(StringComparer.OrdinalIgnoreCase);
        private readonly string _lastTaskId = "last_task_id";
        private readonly string[] _ignoredFields = Array.Empty<string>();
        private readonly Dictionary<string, Func<IAnalyzer>> _analyzerGetter = new(StringComparer.OrdinalIgnoreCase)
        {
            { "standard", () => new StandardAnalyzer() },
            { "simple", () => new SimpleAnalyzer() },
            { "keyword" , () => new KeywordAnalyzer() },
            { "whitespace", () => new WhitespaceAnalyzer() },
            { "pattern", () => new PatternAnalyzer() },
            { "language", () => new LanguageAnalyzer() },
            { "fingerprint", () => new FingerprintAnalyzer() },
            { "custom", () => new CustomAnalyzer() },
            { "stop", () => new StopAnalyzer() },
        };
        private static readonly List<char> _charsToRemove = new()
        {
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
        };

        private string _indexPrefix;
        private bool _disposed;

        public ElasticIndexManager(
            IElasticClient elasticClient,
            ShellSettings shellSettings,
            IOptions<ElasticsearchOptions> elasticsearchOptions,
            IClock clock,
            ILogger<ElasticIndexManager> logger
            )
        {
            _elasticClient = elasticClient;
            _shellSettings = shellSettings;
            _clock = clock;
            _logger = logger;
            _elasticsearchOptions = elasticsearchOptions.Value;
        }

        /// <summary>
        /// <para>Creates an Elasticsearch index with _source mapping.</para>
        /// <para><see href="https://www.elastic.co/guide/en/elasticsearch/reference/8.3/mapping-source-field.html#disable-source-field"/>.</para>
        /// <para>Specify an analyzer for an index based on the ElasticsearchIndexSettings.
        /// <see href="https://www.elastic.co/guide/en/elasticsearch/reference/current/specify-analyzer.html#specify-index-time-default-analyzer"/>,
        /// <see href="https://www.elastic.co/guide/en/elasticsearch/reference/master/analysis-analyzers.html"/>.
        /// </para>
        /// </summary>
        /// <returns><see cref="Boolean"/>.</returns>
        public async Task<bool> CreateIndexAsync(ElasticIndexSettings elasticIndexSettings)
        {
            //Get Index name scoped by ShellName
            if (await Exists(elasticIndexSettings.IndexName))
            {
                return true;
            }

            var analysisDescriptor = new AnalysisDescriptor();
            var analyzersDescriptor = new AnalyzersDescriptor();
            var indexSettingsDescriptor = new IndexSettingsDescriptor();

            // The name "standardanalyzer" is a legacy used prior OC 1.6 release. It can be removed in future releases.
            var analyzerName = elasticIndexSettings.AnalyzerName == "standardanalyzer" ? "standard" : elasticIndexSettings.AnalyzerName;

            if (_elasticsearchOptions.Analyzers.TryGetValue(analyzerName, out var analyzerProperties))
            {
                var analyzer = CreateAnalyzer(analyzerProperties);
                analyzersDescriptor = new AnalyzersDescriptor();
                analysisDescriptor.Analyzers(a => a.UserDefined(analyzerName, analyzer));

                indexSettingsDescriptor = new IndexSettingsDescriptor();
                indexSettingsDescriptor.Analysis(an => analysisDescriptor);
            }

            // Custom metadata to store the last indexing task id.
            var IndexingState = new FluentDictionary<string, object>() {
                    { _lastTaskId, 0 }
                };
            var fullIndexName = GetFullIndexName(elasticIndexSettings.IndexName);
            var createIndexDescriptor = new CreateIndexDescriptor(fullIndexName)
                .Settings(s => indexSettingsDescriptor)
                .Map(m => m
                    .SourceField(s => s
                        .Enabled(elasticIndexSettings.StoreSourceData)
                        .Excludes(new string[] { IndexingConstants.DisplayTextAnalyzedKey }))
                    .Meta(me => IndexingState));

            var response = await _elasticClient.Indices.CreateAsync(createIndexDescriptor);

            // We force some mappings for common fields.
            await _elasticClient.MapAsync<string>(p => p
                .Index(fullIndexName)
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

            // ContainedPart mappings.
            await _elasticClient.MapAsync<ContainedPartModel>(p => p
                .Index(fullIndexName)
                .Properties(p => p
                    .Object<ContainedPartModel>(obj => obj
                        .Name(IndexingConstants.ContainedPartKey)
                        .AutoMap()
                    )
                ));

            // We map DisplayText here because we have 3 different fields with it.
            // We can't have Content.ContentItem.DisplayText as it is mapped as an Object in Elasticsearch.
            await _elasticClient.MapAsync<DisplayTextModel>(p => p
                .Index(fullIndexName)
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
                .Index(fullIndexName)
                .Properties(p => p
                    .Keyword(obj => obj
                        .Name(IndexingConstants.ContentTypeKey)
                    )
                ));

            // DynamicTemplates mapping for Taxonomy indexing mostly.
            await _elasticClient.MapAsync<string>(p => p
                .Index(fullIndexName)
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

        private IAnalyzer CreateAnalyzer(JsonObject analyzerProperties)
        {
            IAnalyzer analyzer = null;

            if (analyzerProperties.TryGetPropertyValue("type", out var typeObject)
                && _analyzerGetter.TryGetValue(typeObject.ToString(), out var getter))
            {
                analyzer = getter.Invoke();

                var properties = analyzer.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);

                foreach (var analyzerProperty in analyzerProperties)
                {
                    if (analyzerProperty.Value == null || String.Equals(analyzerProperty.Key, "type", StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }

                    var key = analyzerProperty.Key.Replace("_", String.Empty);

                    var property = properties.FirstOrDefault(p => p.Name.Equals(key, StringComparison.OrdinalIgnoreCase));

                    if (property == null)
                    {
                        continue;
                    }

                    try
                    {
                        if (property.PropertyType == typeof(StopWords))
                        {
                            if (analyzerProperty.Value is JsonArray)
                            {
                                var values = JsonSerializer.Deserialize<string[]>(analyzerProperty.Value);

                                property.SetValue(analyzer, new StopWords(values));
                            }

                            continue;
                        }

                        if (analyzerProperty.Value is JsonArray)
                        {
                            var values = JsonSerializer.Deserialize<string[]>(analyzerProperty.Value);

                            property.SetValue(analyzer, values);
                        }
                        else
                        {
                            var value = JsonSerializer.Deserialize(analyzerProperty.Value, property.PropertyType);

                            property.SetValue(analyzer, value);
                        }
                    }
                    catch { }
                }
            }

            if (analyzer == null)
            {
                if (_analyzerGetter.TryGetValue(ElasticsearchConstants.DefaultAnalyzer, out getter))
                {
                    analyzer = getter.Invoke();
                }
                else
                {
                    analyzer = _analyzerGetter.First().Value.Invoke();
                }
            }

            return analyzer;
        }

        public async Task<string> GetIndexMappings(string indexName)
        {
            var response = await _elasticClient.LowLevel.Indices.GetMappingAsync<StringResponse>(GetFullIndexName(indexName));

            return response.Body;
        }

        /// <summary>
        /// Store a last_task_id in the Elasticsearch index _meta mappings.
        /// This allows storing the last indexing task id executed on the Elasticsearch index.
        /// <see href="https://www.elastic.co/guide/en/elasticsearch/reference/current/mapping-meta-field.html"/>.
        /// </summary>
        public async Task SetLastTaskId(string indexName, long lastTaskId)
        {
            var IndexingState = new FluentDictionary<string, object>() {
                { _lastTaskId, lastTaskId }
            };

            var putMappingRequest = new PutMappingRequest(GetFullIndexName(indexName))
            {
                Meta = IndexingState
            };

            await _elasticClient.Indices.PutMappingAsync(putMappingRequest);
        }

        /// <summary>
        /// Get a last_task_id in the Elasticsearch index _meta mappings.
        /// This allows retrieving the last indexing task id executed on the index.
        /// </summary>
        public async Task<long> GetLastTaskId(string indexName)
        {
            var jsonDocument = JsonDocument.Parse(await GetIndexMappings(indexName));
            jsonDocument.RootElement.TryGetProperty(GetFullIndexName(indexName), out var jsonElement);
            jsonElement.TryGetProperty("mappings", out var mappings);
            mappings.TryGetProperty("_meta", out var meta);
            meta.TryGetProperty(_lastTaskId, out var lastTaskId);
            lastTaskId.TryGetInt64(out var longValue);

            return longValue;
        }

        public async Task<bool> DeleteDocumentsAsync(string indexName, IEnumerable<string> contentItemIds)
        {
            var success = true;

            if (contentItemIds.Any())
            {
                var descriptor = new BulkDescriptor();

                foreach (var id in contentItemIds)
                    descriptor.Delete<Dictionary<string, object>>(d => d
                        .Index(GetFullIndexName(indexName))
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
                .Index(GetFullIndexName(indexName))
                .Query(q => q.MatchAll())
            );

            return response.IsValid;
        }

        public async Task<bool> DeleteIndex(string indexName)
        {
            if (await Exists(indexName))
            {
                var result = await _elasticClient.Indices.DeleteAsync(GetFullIndexName(indexName));

                return result.Acknowledged;
            }

            return true;
        }

        /// <summary>
        /// Verify if an index exists for the current tenant.
        /// </summary>
        public async Task<bool> Exists(string indexName)
        {
            if (String.IsNullOrWhiteSpace(indexName))
            {
                return false;
            }

            var existResponse = await _elasticClient.Indices.ExistsAsync(GetFullIndexName(indexName));

            return existResponse.Exists;
        }

        /// <summary>
        /// Makes sure that the index names are compliant with Elasticsearch specifications.
        /// <see href="https://www.elastic.co/guide/en/elasticsearch/reference/current/indices-create-index.html#indices-create-api-path-params"/>.
        /// </summary>
        public static string ToSafeIndexName(string indexName)
        {
            indexName = indexName.ToLowerInvariant();

            if (indexName[0] == '-' || indexName[0] == '_' || indexName[0] == '+' || indexName[0] == '.')
            {
                indexName = indexName.Remove(0, 1);
            }

            _charsToRemove.ForEach(c => indexName = indexName.Replace(c.ToString(), String.Empty));

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
                        .Index(GetFullIndexName(indexName))
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
        /// <returns><see cref="ElasticTopDocs"/>.</returns>
        public async Task<ElasticTopDocs> SearchAsync(string indexName, QueryContainer query, List<ISort> sort, int from, int size)
        {
            var elasticTopDocs = new ElasticTopDocs();

            if (await Exists(indexName))
            {
                var fullIndexName = GetFullIndexName(indexName);

                var searchRequest = new SearchRequest(fullIndexName)
                {
                    Query = query,
                    From = from,
                    Size = size,
                    Sort = sort
                };

                var searchResponse = await _elasticClient.SearchAsync<Dictionary<string, object>>(searchRequest);

                if (searchResponse.IsValid)
                {
                    elasticTopDocs.Count = searchResponse.Hits.Count;

                    var topDocs = new List<Dictionary<string, object>>();

                    var documents = searchResponse.Documents.GetEnumerator();
                    var hits = searchResponse.Hits.GetEnumerator();

                    while (documents.MoveNext() && hits.MoveNext())
                    {
                        var document = documents.Current;

                        if (document != null)
                        {
                            topDocs.Add(document);

                            continue;
                        }

                        var hit = hits.Current;

                        var topDoc = new Dictionary<string, object>
                        {
                            { "ContentItemId", hit.Id }
                        };

                        topDocs.Add(topDoc);
                    }

                    elasticTopDocs.TopDocs = topDocs;
                }

                _timestamps[fullIndexName] = _clock.UtcNow;
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

                _timestamps[GetFullIndexName(indexName)] = _clock.UtcNow;
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
                if (Array.Exists(_ignoredFields, x => entry.Name.Contains(x)))
                {
                    continue;
                }

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
                        if (entry.Value != null && Int64.TryParse(entry.Value.ToString(), out var value))
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

                    case DocumentIndex.Types.Text:
                        if (entry.Value != null)
                        {
                            var stringValue = Convert.ToString(entry.Value);

                            if (!String.IsNullOrEmpty(stringValue))
                            {
                                AddValue(entries, entry.Name, stringValue);
                            }
                        }
                        break;
                }
            }

            return entries;
        }

        public string GetFullIndexName(string indexName)
        {
            if (String.IsNullOrEmpty(indexName))
            {
                throw new ArgumentException($"{nameof(indexName)} cannot be null or empty.");
            }

            return GetIndexPrefix() + "_" + indexName;
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
            var values = new List<object>()
            {
                entries[key],
                value,
            };

            entries[key] = values;
        }

        private string GetIndexPrefix()
        {
            if (_indexPrefix == null)
            {
                var parts = new List<string>();

                if (!String.IsNullOrWhiteSpace(_elasticsearchOptions.IndexPrefix))
                {
                    parts.Add(_elasticsearchOptions.IndexPrefix.ToLowerInvariant());
                }

                parts.Add(_shellSettings.Name.ToLowerInvariant());

                _indexPrefix = String.Join("_", parts);
            }

            return _indexPrefix;
        }

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;
        }
    }
}
