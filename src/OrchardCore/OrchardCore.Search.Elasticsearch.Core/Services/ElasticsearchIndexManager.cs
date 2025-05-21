using System.Collections.Concurrent;
using System.Text.Json;
using System.Text.Json.Nodes;
using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.Analysis;
using Elastic.Clients.Elasticsearch.IndexManagement;
using Elastic.Clients.Elasticsearch.Mapping;
using Elastic.Clients.Elasticsearch.QueryDsl;
using Elastic.Transport;
using Elastic.Transport.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OrchardCore.ContentManagement;
using OrchardCore.Contents.Indexing;
using OrchardCore.Indexing;
using OrchardCore.Modules;
using OrchardCore.Search.Elasticsearch.Core.Models;
using OrchardCore.Search.Elasticsearch.Models;

namespace OrchardCore.Search.Elasticsearch.Core.Services;

/// <summary>
/// Provides methods to manage Elasticsearch indices.
/// </summary>
public sealed class ElasticsearchIndexManager
{
    private readonly ElasticsearchClient _elasticClient;
    private readonly ElasticsearchIndexNameService _elasticsearchIndexNameService;
    private readonly IEnumerable<IElasticsearchIndexEvents> _indexEvents;
    private readonly IClock _clock;
    private readonly ILogger _logger;
    private readonly ElasticsearchOptions _elasticSearchOptions;
    private readonly ConcurrentDictionary<string, DateTime> _timestamps = new(StringComparer.OrdinalIgnoreCase);

    private const string _lastTaskId = "last_task_id";

    private static readonly Dictionary<string, Type> _analyzerGetter = new(StringComparer.OrdinalIgnoreCase)
    {
        { ElasticsearchConstants.DefaultAnalyzer, typeof(StandardAnalyzer) },
        { ElasticsearchConstants.SimpleAnalyzer, typeof(SimpleAnalyzer) },
        { ElasticsearchConstants.KeywordAnalyzer, typeof(KeywordAnalyzer) },
        { ElasticsearchConstants.WhitespaceAnalyzer, typeof(WhitespaceAnalyzer) },
        { ElasticsearchConstants.PatternAnalyzer, typeof(PatternAnalyzer) },
        { ElasticsearchConstants.FingerprintAnalyzer, typeof(FingerprintAnalyzer) },
        { ElasticsearchConstants.CustomAnalyzer, typeof(CustomAnalyzer) },
        { ElasticsearchConstants.StopAnalyzer, typeof(StopAnalyzer) },
    };

    private static readonly Dictionary<string, Type> _tokenFilterGetter = new(StringComparer.OrdinalIgnoreCase)
    {
        { "asciifolding", typeof(AsciiFoldingTokenFilter) },
        { "common_grams", typeof(CommonGramsTokenFilter) },
        { "condition", typeof(ConditionTokenFilter) },
        { "delimited_payload", typeof(DelimitedPayloadTokenFilter) },
        { "dictionary_decompounder", typeof(DictionaryDecompounderTokenFilter) },
        { "edge_ngram", typeof(EdgeNGramTokenFilter) },
        { "elision", typeof(ElisionTokenFilter) },
        { "fingerprint", typeof(FingerprintTokenFilter) },
        { "hunspell", typeof(HunspellTokenFilter) },
        { "hyphenation_decompounder", typeof(HyphenationDecompounderTokenFilter) },
        { "icu_collation", typeof(IcuCollationTokenFilter) },
        { "icu_folding", typeof(IcuFoldingTokenFilter) },
        { "icu_normalizer", typeof(IcuNormalizationTokenFilter) },
        { "icu_transform", typeof(IcuTransformTokenFilter) },
        { "keep_types", typeof(KeepTypesTokenFilter) },
        { "keep", typeof(KeepWordsTokenFilter) },
        { "keyword_marker", typeof(KeywordMarkerTokenFilter) },
        { "kstem", typeof(KStemTokenFilter) },
        { "kuromoji_part_of_speech", typeof(KuromojiPartOfSpeechTokenFilter) },
        { "kuromoji_readingform", typeof(KuromojiReadingFormTokenFilter) },
        { "kuromoji_stemmer", typeof(KuromojiStemmerTokenFilter) },
        { "length", typeof(LengthTokenFilter) },
        { "limit", typeof(LimitTokenCountTokenFilter) },
        { "lowercase", typeof(LowercaseTokenFilter) },
        { "multiplexer", typeof(MultiplexerTokenFilter) },
        { "ngram", typeof(NGramTokenFilter) },
        { "nori_part_of_speech", typeof(NoriPartOfSpeechTokenFilter) },
        { "pattern_capture", typeof(PatternCaptureTokenFilter) },
        { "pattern_replace", typeof(PatternReplaceTokenFilter) },
        { "phonetic", typeof(PhoneticTokenFilter) },
        { "porter_stem", typeof(PorterStemTokenFilter) },
        { "remove_duplicates", typeof(RemoveDuplicatesTokenFilter) },
        { "reverse", typeof(ReverseTokenFilter) },
        { "shingle", typeof(ShingleTokenFilter) },
        { "snowball", typeof(SnowballTokenFilter) },
        { "stemmer_override", typeof(StemmerOverrideTokenFilter) },
        { "stemmer", typeof(StemmerTokenFilter) },
        { "stop", typeof(StopTokenFilter) },
        { "synonym_graph", typeof(SynonymGraphTokenFilter) },
        { "synonym", typeof(SynonymTokenFilter) },
        { "trim", typeof(TrimTokenFilter) },
        { "truncate", typeof(TruncateTokenFilter) },
        { "unique", typeof(UniqueTokenFilter) },
        { "uppercase", typeof(UppercaseTokenFilter) },
        { "word_delimiter_graph", typeof(WordDelimiterGraphTokenFilter) },
        { "word_delimiter", typeof(WordDelimiterTokenFilter) },
    };

    private static readonly List<char> _charsToRemove =
    [
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
    ];

    public ElasticsearchIndexManager(
        ElasticsearchClient elasticClient,
        ElasticsearchIndexNameService elasticsearchIndexNameService,
        IOptions<ElasticsearchOptions> elasticsearchOptions,
        IEnumerable<IElasticsearchIndexEvents> indexEvents,
        IClock clock,
        ILogger<ElasticsearchIndexManager> logger
        )
    {
        _elasticClient = elasticClient;
        _elasticsearchIndexNameService = elasticsearchIndexNameService;
        _indexEvents = indexEvents;
        _clock = clock;
        _logger = logger;
        _elasticSearchOptions = elasticsearchOptions.Value;
    }

    /// <summary>
    /// <para>Creates an Elasticsearch index with _source mapping.</para>
    /// <para><see href="https://www.elastic.co/guide/en/elasticsearch/reference/8.3/mapping-source-field.html#disable-source-field"/>.</para>
    /// <para>Specify an analyzer for an index based on the ElasticsearchIndexSettings.
    /// <see href="https://www.elastic.co/guide/en/elasticsearch/reference/current/specify-analyzer.html#specify-index-time-default-analyzer"/>,
    /// <see href="https://www.elastic.co/guide/en/elasticsearch/reference/master/analysis-analyzers.html"/>.
    /// </para>
    /// </summary>
    /// <returns><see cref="bool"/>.</returns>
    public async Task<bool> CreateIndexAsync(ElasticIndexSettings settings)
    {
        // Get Index name scoped by ShellName
        if (await ExistsAsync(settings.IndexName))
        {
            return true;
        }

        var context = new ElasticsearchIndexCreateContext(settings);

        await _indexEvents.InvokeAsync((handler, ctx) => handler.CreatingAsync(ctx), context, _logger);

        var createIndexRequest = GetCreateIndexRequest(settings);
        var response = await _elasticClient.Indices.CreateAsync(createIndexRequest);

        await _indexEvents.InvokeAsync((handler, ctx) => handler.CreatedAsync(ctx), context, _logger);

        return response.Acknowledged;
    }

    public async Task<string> GetIndexMappings(string indexName)
    {
        ArgumentException.ThrowIfNullOrEmpty(indexName);

        var indexFullName = _elasticsearchIndexNameService.GetFullIndexName(indexName);

        var response = await _elasticClient.Indices.GetMappingAsync<GetMappingResponse>((t) =>
        {
            t.Indices(indexFullName);
        });

        if (!response.IsValidResponse)
        {
            if (response.TryGetOriginalException(out var ex))
            {
                _logger.LogWarning("There were issues retrieving index mappings from Elasticsearch. Exception: {OriginalException}", ex);
            }
            else
            {
                _logger.LogWarning("There were issues retrieving index mappings from Elasticsearch.");
            }

            return null;
        }

        var mappings = response.Mappings[indexFullName];

        return _elasticClient.RequestResponseSerializer.SerializeToString(mappings);
    }

    public async Task<string> GetIndexSettings(string indexName)
    {
        ArgumentException.ThrowIfNullOrEmpty(indexName);

        var indexFullName = _elasticsearchIndexNameService.GetFullIndexName(indexName);

        var response = await _elasticClient.Indices.GetAsync<GetIndexResponse>(o =>
        {
            o.Indices(indexFullName);
        });

        if (!response.IsValidResponse)
        {
            if (response.TryGetOriginalException(out var ex))
            {
                _logger.LogWarning("There were issues retrieving index settings from Elasticsearch. Exception: {OriginalException}", ex);
            }
            else
            {
                _logger.LogWarning("There were issues retrieving index settings from Elasticsearch.");
            }
        }

        var setting = response.Indices[indexFullName].Settings;

        return _elasticClient.RequestResponseSerializer.SerializeToString(setting);
    }

    public async Task<string> GetIndexInfo(string indexName)
    {
        ArgumentException.ThrowIfNullOrEmpty(indexName);

        var indexFullName = _elasticsearchIndexNameService.GetFullIndexName(indexName);

        var response = await _elasticClient.Indices.GetAsync<GetIndexResponse>(o =>
        {
            o.Indices(indexFullName);
        });

        if (!response.IsValidResponse)
        {
            if (response.TryGetOriginalException(out var ex))
            {
                _logger.LogWarning("There were issues retrieving index info from Elasticsearch. Exception: {OriginalException}", ex);
            }
            else
            {
                _logger.LogWarning("There were issues retrieving index info from Elasticsearch.");
            }
        }

        var info = response.Indices[indexFullName];

        return _elasticClient.RequestResponseSerializer.SerializeToString(info);
    }

    /// <summary>
    /// Store a last_task_id in the Elasticsearch index _meta mappings.
    /// This allows storing the last indexing task id executed on the Elasticsearch index.
    /// <see href="https://www.elastic.co/guide/en/elasticsearch/reference/current/mapping-meta-field.html"/>.
    /// </summary>
    public async Task SetLastTaskIdAsync(string indexName, long lastTaskId)
    {
        ArgumentException.ThrowIfNullOrEmpty(indexName);

        var indexFullName = _elasticsearchIndexNameService.GetFullIndexName(indexName);

        var IndexingState = new FluentDictionary<string, object>()
        {
            { _lastTaskId, lastTaskId },
        };

        var putMappingRequest = new PutMappingRequest(indexFullName)
        {
            Meta = IndexingState,
        };

        await _elasticClient.Indices.PutMappingAsync(putMappingRequest);
    }

    /// <summary>
    /// Get a last_task_id in the Elasticsearch index _meta mappings.
    /// This allows retrieving the last indexing task id executed on the index.
    /// </summary>
    public async Task<long> GetLastTaskId(string indexName)
    {
        ArgumentException.ThrowIfNullOrEmpty(indexName);

        var mappings = await GetIndexMappings(indexName);

        var jsonDocument = JsonDocument.Parse(mappings);
        jsonDocument.RootElement.TryGetProperty("_meta", out var meta);
        meta.TryGetProperty(_lastTaskId, out var lastTaskId);
        lastTaskId.TryGetInt64(out var longValue);

        return longValue;
    }

    /// <summary>
    /// Deletes documents from the index using the specified contentItemIds in a single request.
    /// <see href="https://www.elastic.co/guide/en/elasticsearch/reference/master/docs-delete-by-query.html"/>.
    /// </summary>
    public async Task<bool> DeleteDocumentsAsync(string indexName, IEnumerable<string> contentItemIds)
    {
        ArgumentException.ThrowIfNullOrEmpty(indexName);
        ArgumentNullException.ThrowIfNull(contentItemIds);

        if (contentItemIds.Any())
        {
            var indexFullName = _elasticsearchIndexNameService.GetFullIndexName(indexName);

            var response = await _elasticClient.DeleteByQueryAsync<Dictionary<string, object>>(indexFullName, descriptor => descriptor
                .Query(q => q
                    .Bool(b => b
                        .Filter(f => f
                            .Terms(t => t
                                .Field(IndexingConstants.ContentItemIdKey)
                                .Terms(new TermsQueryField(contentItemIds.Select(id => FieldValue.String(id)).ToArray()))
                            )
                        )
                    )
                )
            );

            return response.IsValidResponse;
        }

        return true;
    }

    /// <summary>
    /// Deletes all documents from the index in a single request.
    /// <see href="https://www.elastic.co/guide/en/elasticsearch/reference/master/docs-delete-by-query.html"/>.
    /// </summary>
    public async Task<bool> DeleteAllDocumentsAsync(string indexName)
    {
        ArgumentException.ThrowIfNullOrEmpty(indexName);

        var indexFullName = _elasticsearchIndexNameService.GetFullIndexName(indexName);

        var response = await _elasticClient.DeleteByQueryAsync<Dictionary<string, object>>(indexFullName, descriptor => descriptor
            .Query(q => q
                .MatchAll(new MatchAllQuery())
             )
        );

        return response.IsValidResponse;
    }

    public async Task<bool> DeleteIndexAsync(string indexName)
    {
        ArgumentException.ThrowIfNullOrEmpty(indexName);

        if (await ExistsAsync(indexName))
        {
            var indexFullName = _elasticsearchIndexNameService.GetFullIndexName(indexName);

            var context = new ElasticsearchIndexRemoveContext(indexName, indexFullName);

            await _indexEvents.InvokeAsync((handler, ctx) => handler.RemovingAsync(ctx), context, _logger);

            var result = await _elasticClient.Indices.DeleteAsync(indexFullName);

            await _indexEvents.InvokeAsync((handler, ctx) => handler.RemovedAsync(ctx), context, _logger);

            return result.Acknowledged;
        }

        return true;
    }

    public async Task<bool> RebuildIndexAsync(ElasticIndexSettings settings)
    {
        ArgumentNullException.ThrowIfNull(settings);

        if (await ExistsAsync(settings.IndexName))
        {
            var context = new ElasticsearchIndexRebuildContext(settings);

            await _indexEvents.InvokeAsync((handler, ctx) => handler.RebuildingAsync(ctx), context, _logger);

            var createIndexRequest = GetCreateIndexRequest(settings);

            await _elasticClient.Indices.DeleteAsync(settings.IndexFullName);

            var response = await _elasticClient.Indices.CreateAsync(createIndexRequest);

            await _indexEvents.InvokeAsync((handler, ctx) => handler.RebuiltAsync(ctx), context, _logger);

            return response.Acknowledged;
        }

        return true;
    }

    /// <summary>
    /// Verify if an index exists for the current tenant.
    /// </summary>
    public async Task<bool> ExistsAsync(string indexName)
    {
        ArgumentException.ThrowIfNullOrEmpty(indexName);

        var indexFullName = _elasticsearchIndexNameService.GetFullIndexName(indexName);

        var existResponse = await _elasticClient.Indices.ExistsAsync(indexFullName);

        return existResponse.Exists;
    }

    /// <summary>
    /// Makes sure that the index names are compliant with Elasticsearch specifications.
    /// <see href="https://www.elastic.co/guide/en/elasticsearch/reference/current/indices-create-index.html#indices-create-api-path-params"/>.
    /// </summary>
    public static string ToSafeIndexName(string indexName)
    {
        ArgumentException.ThrowIfNullOrEmpty(indexName);

        indexName = indexName.ToLowerInvariant();

        if (indexName[0] == '-' || indexName[0] == '_' || indexName[0] == '+' || indexName[0] == '.')
        {
            indexName = indexName.Remove(0, 1);
        }

        _charsToRemove.ForEach(c => indexName = indexName.Replace(c.ToString(), string.Empty));

        return indexName;
    }

    public async Task StoreDocumentsAsync(ElasticIndexSettings settings, IEnumerable<DocumentIndex> indexDocuments)
    {
        ArgumentNullException.ThrowIfNull(settings);
        ArgumentNullException.ThrowIfNull(indexDocuments);

        if (indexDocuments == null || !indexDocuments.Any())
        {
            return;
        }

        foreach (var batch in indexDocuments.PagesOf(2500))
        {
            var response = await _elasticClient.BulkAsync(settings.IndexFullName,
                descriptor => descriptor.CreateElasticDocument(batch).Refresh(Refresh.True));

            if (!response.IsValidResponse)
            {
                if (response.TryGetOriginalException(out var ex))
                {
                    _logger.LogWarning("There were issues indexing a document using Elasticsearch. Exception: {OriginalException}", ex);
                }
                else
                {
                    _logger.LogWarning("There were issues indexing a document using Elasticsearch.");
                }
            }
        }
    }

    public async Task<ElasticsearchResult> SearchAsync(ElasticsearchSearchContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        var elasticTopDocs = new ElasticsearchResult()
        {
            TopDocs = [],
        };

        if (await ExistsAsync(context.Settings.IndexName))
        {
            var searchRequest = new SearchRequest(context.Settings.IndexFullName)
            {
                Query = context.Query,
                From = context.From,
                Size = context.Size,
                Sort = context.Sorts ?? [],
                Source = context.Source,
                Fields = context.Fields ?? [],
                Highlight = context.Highlight,
            };

            var searchResponse = await _elasticClient.SearchAsync<JsonObject>(searchRequest);

            if (searchResponse.IsSuccess())
            {
                ProcessSuccessfulSearchResponse(elasticTopDocs, searchResponse);
            }

            _timestamps[context.Settings.IndexFullName] = _clock.UtcNow;
        }

        return elasticTopDocs;
    }

    /// <summary>
    /// Returns results from a search made with NEST Fluent DSL query.
    /// </summary>
    public async Task SearchAsync(string indexName, Func<ElasticsearchClient, Task> elasticClient)
    {
        ArgumentException.ThrowIfNullOrEmpty(indexName);
        ArgumentNullException.ThrowIfNull(elasticClient);

        if (await ExistsAsync(indexName))
        {
            await elasticClient(_elasticClient);

            var indexFullName = _elasticsearchIndexNameService.GetFullIndexName(indexName);

            _timestamps[indexFullName] = _clock.UtcNow;
        }
    }

    private CreateIndexRequest GetCreateIndexRequest(ElasticIndexSettings settings)
    {
        var indexSettings = new IndexSettings
        {
            Analysis = new IndexSettingsAnalysis()
            {
                Analyzers = new Analyzers(),
                TokenFilters = new TokenFilters(),
            },
        };

        // The name "standardanalyzer" is a legacy used prior OC 1.6 release. It can be removed in future releases.
        var analyzerName = (settings.AnalyzerName == "standardanalyzer"
        ? null
        : settings.AnalyzerName) ?? ElasticsearchConstants.DefaultAnalyzer;

        if (_elasticSearchOptions.Analyzers is not null && _elasticSearchOptions.Analyzers.TryGetValue(analyzerName, out var analyzerProperties))
        {
            var analyzer = GetAnalyzer(analyzerProperties);

            indexSettings.Analysis.Analyzers.Add(analyzerName, analyzer);
        }

        if (_elasticSearchOptions.TokenFilters is not null && _elasticSearchOptions.TokenFilters.Count > 0)
        {
            foreach (var filter in _elasticSearchOptions.TokenFilters)
            {
                if (!filter.Value.TryGetPropertyValue("type", out var typeObject) ||
                    !_tokenFilterGetter.TryGetValue(typeObject.ToString(), out var tokenFilterType))
                {
                    continue;
                }

                RemoveTypeNode(filter.Value);

                var tokenFilter = filter.Value.ToObject(tokenFilterType) as ITokenFilter;

                if (tokenFilter is not null)
                {
                    indexSettings.Analysis.TokenFilters.Add(filter.Key, tokenFilter);
                }
            }
        }

        var createIndexRequest = new CreateIndexRequest(settings.IndexFullName)
        {
            Settings = indexSettings,
            Mappings = settings.IndexMappings ?? new TypeMapping(),
        };

        // Custom metadata to store the last indexing task id.
        createIndexRequest.Mappings.Meta ??= new FluentDictionary<string, object>();

        createIndexRequest.Mappings.Meta[_lastTaskId] = 0;

        return createIndexRequest;
    }

    internal async Task<ElasticsearchResult> SearchAsync(string indexName, string query)
    {
        ArgumentException.ThrowIfNullOrEmpty(indexName);
        ArgumentException.ThrowIfNullOrEmpty(query);

        var elasticTopDocs = new ElasticsearchResult()
        {
            TopDocs = [],
        };

        var indexFullName = _elasticsearchIndexNameService.GetFullIndexName(indexName);

        if (await ExistsAsync(indexName))
        {
            var endpoint = new EndpointPath(Elastic.Transport.HttpMethod.GET, $"{indexFullName}/_search");
            var searchResponse = await _elasticClient.Transport
                .RequestAsync<SearchResponse<JsonObject>>(endpoint, postData: PostData.String(query));

            if (searchResponse.IsSuccess())
            {
                ProcessSuccessfulSearchResponse(elasticTopDocs, searchResponse);
            }

            _timestamps[indexFullName] = _clock.UtcNow;
        }

        return elasticTopDocs;
    }

    private static IAnalyzer GetAnalyzer(JsonObject analyzerProperties)
    {
        IAnalyzer analyzer = null;

        if (analyzerProperties.TryGetPropertyValue("type", out var typeObject)
            && _analyzerGetter.TryGetValue(typeObject.ToString(), out var analyzerType))
        {
            RemoveTypeNode(analyzerProperties);

            analyzer = analyzerProperties.ToObject(analyzerType) as IAnalyzer;
        }

        if (analyzer == null)
        {
            if (_analyzerGetter.TryGetValue(ElasticsearchConstants.DefaultAnalyzer, out analyzerType))
            {
                RemoveTypeNode(analyzerProperties);

                analyzer = analyzerProperties.ToObject(analyzerType) as IAnalyzer;
            }
            else
            {
                RemoveTypeNode(analyzerProperties);

                analyzer = analyzerProperties.ToObject(_analyzerGetter.First().Value) as IAnalyzer;
            }
        }

        return analyzer;
    }


    private static void ProcessSuccessfulSearchResponse(ElasticsearchResult elasticTopDocs, SearchResponse<JsonObject> searchResponse)
    {
        elasticTopDocs.Count = searchResponse.Hits.Count;

        var documents = searchResponse.Documents.GetEnumerator();
        var hits = searchResponse.Hits.GetEnumerator();

        while (documents.MoveNext() && hits.MoveNext())
        {
            var hit = hits.Current;

            var document = documents.Current;

            if (document != null)
            {
                elasticTopDocs.TopDocs.Add(new ElasticsearchRecord(document)
                {
                    Score = hit.Score,
                    Highlights = hit.Highlight,
                });

                continue;
            }

            var topDoc = new JsonObject
            {
                { nameof(ContentItem.ContentItemId), hit.Id },
            };

            elasticTopDocs.TopDocs.Add(new ElasticsearchRecord(topDoc)
            {
                Score = hit.Score,
                Highlights = hit.Highlight,
            });
        }
    }

    private static void RemoveTypeNode(JsonObject analyzerProperties)
    {
        var typeKey = analyzerProperties.FirstOrDefault(x => x.Key.Equals("type", StringComparison.OrdinalIgnoreCase)).Key;

        if (typeKey is not null)
        {
            analyzerProperties.Remove(typeKey);
        }
    }
}
