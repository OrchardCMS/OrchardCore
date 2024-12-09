using System.Collections.Concurrent;
using System.Text.Json;
using System.Text.Json.Nodes;
using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.Analysis;
using Elastic.Clients.Elasticsearch.Fluent;
using Elastic.Clients.Elasticsearch.IndexManagement;
using Elastic.Clients.Elasticsearch.Mapping;
using Elastic.Clients.Elasticsearch.QueryDsl;
using Elastic.Transport;
using Elastic.Transport.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OrchardCore.ContentManagement;
using OrchardCore.Contents.Indexing;
using OrchardCore.Environment.Shell;
using OrchardCore.Indexing;
using OrchardCore.Modules;
using OrchardCore.Search.Elasticsearch.Core.Mappings;
using OrchardCore.Search.Elasticsearch.Core.Models;

namespace OrchardCore.Search.Elasticsearch.Core.Services;

/// <summary>
/// Provides methods to manage Elasticsearch indices.
/// </summary>
public sealed class ElasticsearchIndexManager
{
    private const string _separator = "_";
    private const string _idsPostfixPattern = "*" + IndexingConstants.IdsKey;
    private const string _inheritedPostfixPattern = "*" + IndexingConstants.InheritedKey;
    private const string _locationPostFixPattern = "*.Location";

    private readonly ElasticsearchClient _elasticClient;
    private readonly ShellSettings _shellSettings;
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
        { ElasticsearchConstants.LanguageAnalyzer, typeof(LanguageAnalyzer) },
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
        { "word_delimiter", typeof(WordDelimiterTokenFilter) }
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

    private string _indexPrefix;

    public ElasticsearchIndexManager(
        ElasticsearchClient elasticClient,
        ShellSettings shellSettings,
        IOptions<ElasticsearchOptions> elasticsearchOptions,
        IClock clock,
        ILogger<ElasticsearchIndexManager> logger
        )
    {
        _elasticClient = elasticClient;
        _shellSettings = shellSettings;
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
    public async Task<bool> CreateIndexAsync(ElasticIndexSettings elasticIndexSettings)
    {
        // Get Index name scoped by ShellName
        if (await ExistsAsync(elasticIndexSettings.IndexName))
        {
            return true;
        }

        var indexSettings = new IndexSettings
        {
            Analysis = new IndexSettingsAnalysis()
            {
                Analyzers = new Analyzers(),
                TokenFilters = new TokenFilters(),
            },
        };

        // The name "standardanalyzer" is a legacy used prior OC 1.6 release. It can be removed in future releases.
        var analyzerName = (elasticIndexSettings.AnalyzerName == "standardanalyzer"
        ? null
        : elasticIndexSettings.AnalyzerName) ?? ElasticsearchConstants.DefaultAnalyzer;

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

        // Custom metadata to store the last indexing task id.
        var IndexingState = new FluentDictionary<string, object>()
        {
            { _lastTaskId, 0 },
        };

        var createIndexRequest = new CreateIndexRequest(GetFullIndexName(elasticIndexSettings.IndexName))
        {
            Settings = indexSettings,
            Mappings = new TypeMapping
            {
                Source = new SourceField()
                {
                    Enabled = elasticIndexSettings.StoreSourceData,
                    Excludes = [IndexingConstants.DisplayTextAnalyzedKey],
                },
                Meta = IndexingState,
                Properties = new Properties
                {
                    [IndexingConstants.ContentItemIdKey] = new KeywordProperty(),
                    [IndexingConstants.ContentItemVersionIdKey] = new KeywordProperty(),
                    [IndexingConstants.OwnerKey] = new KeywordProperty(),
                    [IndexingConstants.FullTextKey] = new TextProperty(),
                    [IndexingConstants.ContainedPartKey] = new ObjectProperty()
                    {
                        Properties = new Properties()
                        {
                            [nameof(ContainedPartModel.Ids)] = new KeywordProperty(),
                            [nameof(ContainedPartModel.Order)] = new FloatNumberProperty(),
                        },
                    },

                    // We map DisplayText here because we have 3 different fields with it.
                    // We can't have Content.ContentItem.DisplayText as it is mapped as an Object in Elasticsearch.
                    [IndexingConstants.DisplayTextKey] = new ObjectProperty()
                    {
                        Properties = new Properties()
                        {
                            [nameof(DisplayTextModel.Analyzed)] = new TextProperty(),
                            [nameof(DisplayTextModel.Normalized)] = new KeywordProperty(),
                            [nameof(DisplayTextModel.Keyword)] = new KeywordProperty(),
                        },
                    },

                    // We map ContentType as a keyword because else the automatic mapping will break the queries.
                    // We need to access it with Content.ContentItem.ContentType as a keyword
                    // for the ContentPickerResultProvider(s).
                    [IndexingConstants.ContentTypeKey] = new KeywordProperty(),
                },

                DynamicTemplates = new List<IDictionary<string, DynamicTemplate>>()
            }
        };

        var inheritedPostfix = DynamicTemplate.Mapping(new KeywordProperty());
        inheritedPostfix.PathMatch = [_inheritedPostfixPattern];
        inheritedPostfix.MatchMappingType = ["string"];
        createIndexRequest.Mappings.DynamicTemplates.Add(new Dictionary<string, DynamicTemplate>()
        {
            { _inheritedPostfixPattern, inheritedPostfix },
        });

        var idsPostfix = DynamicTemplate.Mapping(new KeywordProperty());
        idsPostfix.PathMatch = [_idsPostfixPattern];
        idsPostfix.MatchMappingType = ["string"];
        createIndexRequest.Mappings.DynamicTemplates.Add(new Dictionary<string, DynamicTemplate>()
        {
            { _idsPostfixPattern, idsPostfix },
        });

        var locationPostfix = DynamicTemplate.Mapping(new GeoPointProperty());
        locationPostfix.PathMatch = [_locationPostFixPattern];
        locationPostfix.MatchMappingType = ["object"];
        createIndexRequest.Mappings.DynamicTemplates.Add(new Dictionary<string, DynamicTemplate>()
        {
            { _locationPostFixPattern, locationPostfix },
        });

        var response = await _elasticClient.Indices.CreateAsync(createIndexRequest);

        return response.Acknowledged;
    }

    public async Task<string> GetIndexMappings(string indexName)
    {
        IndexName indexFullName = GetFullIndexName(indexName);

        var response = await _elasticClient.Indices.GetMappingAsync<GetMappingResponse>(indexFullName);

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

        var mappings = response.Indices[indexFullName].Mappings;

        return _elasticClient.RequestResponseSerializer.SerializeToString(mappings);
    }

    public async Task<string> GetIndexSettings(string indexName)
    {
        IndexName fullName = GetFullIndexName(indexName);

        var response = await _elasticClient.Indices.GetAsync<GetIndexResponse>(fullName);

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

        var settings = response.Indices[fullName].Settings;

        return _elasticClient.RequestResponseSerializer.SerializeToString(settings);
    }

    public async Task<string> GetIndexInfo(string indexName)
    {
        IndexName fullName = GetFullIndexName(indexName);

        var response = await _elasticClient.Indices.GetAsync<GetIndexResponse>(fullName);

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

        var info = response.Indices[fullName];

        return _elasticClient.RequestResponseSerializer.SerializeToString(info);
    }

    /// <summary>
    /// Store a last_task_id in the Elasticsearch index _meta mappings.
    /// This allows storing the last indexing task id executed on the Elasticsearch index.
    /// <see href="https://www.elastic.co/guide/en/elasticsearch/reference/current/mapping-meta-field.html"/>.
    /// </summary>
    public async Task SetLastTaskId(string indexName, long lastTaskId)
    {
        ArgumentException.ThrowIfNullOrEmpty(indexName);

        var IndexingState = new FluentDictionary<string, object>()
        {
            { _lastTaskId, lastTaskId },
        };

        var putMappingRequest = new PutMappingRequest(GetFullIndexNameInternal(indexName))
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
            IndexName fullName = GetFullIndexName(indexName);

            var response = await _elasticClient.DeleteByQueryAsync<Dictionary<string, object>>(fullName, descriptor => descriptor
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
        IndexName fullName = GetFullIndexName(indexName);

        var response = await _elasticClient.DeleteByQueryAsync<Dictionary<string, object>>(fullName, descriptor => descriptor
            .Query(q => q
                .MatchAll(new MatchAllQuery())
             )
        );

        return response.IsValidResponse;
    }

    public async Task<bool> DeleteIndex(string indexName)
    {
        if (await ExistsAsync(indexName))
        {
            var result = await _elasticClient.Indices.DeleteAsync(GetFullIndexName(indexName));

            return result.Acknowledged;
        }

        return true;
    }

    /// <summary>
    /// Verify if an index exists for the current tenant.
    /// </summary>
    public async Task<bool> ExistsAsync(string indexName)
    {
        if (string.IsNullOrWhiteSpace(indexName))
        {
            return false;
        }

        var existResponse = await _elasticClient.Indices.ExistsAsync(GetFullIndexNameInternal(indexName));

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

    public async Task StoreDocumentsAsync(string indexName, IEnumerable<DocumentIndex> indexDocuments)
    {
        ArgumentException.ThrowIfNullOrEmpty(indexName);
        ArgumentNullException.ThrowIfNull(indexDocuments);

        if (indexDocuments == null || !indexDocuments.Any())
        {
            return;
        }

        var indexFullName = GetFullIndexNameInternal(indexName);

        foreach (var batch in indexDocuments.PagesOf(2500))
        {
            var response = await _elasticClient.BulkAsync(indexFullName,
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

        if (await ExistsAsync(context.IndexName))
        {
            var fullIndexName = GetFullIndexName(context.IndexName);

            var searchRequest = new SearchRequest(fullIndexName)
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

            _timestamps[fullIndexName] = _clock.UtcNow;
        }

        return elasticTopDocs;
    }

    /// <summary>
    /// Returns results from a search made with a NEST QueryContainer query.
    /// </summary>
    /// <param name="indexName"></param>
    /// <param name="query"></param>
    /// <param name="sort"></param>
    /// <param name="from"></param>
    /// <param name="size"></param>
    /// <returns><see cref="ElasticsearchResult"/>.</returns>
    [Obsolete("This method will be removed in future release. Instead use SearchAsync(ElasticsearchSearchContext) method instead.")]
    public Task<ElasticsearchResult> SearchAsync(string indexName, Query query, IList<SortOptions> sort, int from, int size)
    {
        var context = new ElasticsearchSearchContext(indexName, query)
        {
            Sorts = sort,
            From = from,
            Size = size,
        };

        return SearchAsync(context);
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

            _timestamps[GetFullIndexName(indexName)] = _clock.UtcNow;
        }
    }

    public string GetFullIndexName(string indexName)
    {
        ArgumentException.ThrowIfNullOrEmpty(indexName);

        return GetFullIndexNameInternal(indexName);
    }

    internal async Task<ElasticsearchResult> SearchAsync(string indexName, string query)
    {
        ArgumentException.ThrowIfNullOrEmpty(indexName);
        ArgumentException.ThrowIfNullOrEmpty(query);

        var elasticTopDocs = new ElasticsearchResult()
        {
            TopDocs = [],
        };

        if (await ExistsAsync(indexName))
        {
            var fullIndexName = GetFullIndexName(indexName);

            var endpoint = new EndpointPath(Elastic.Transport.HttpMethod.GET, fullIndexName + "/_search");
            var searchResponse = await _elasticClient.Transport
                .RequestAsync<SearchResponse<JsonObject>>(endpoint, postData: PostData.String(query));

            if (searchResponse.IsSuccess())
            {
                ProcessSuccessfulSearchResponse(elasticTopDocs, searchResponse);
            }

            _timestamps[fullIndexName] = _clock.UtcNow;
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

    private string GetFullIndexNameInternal(string indexName)
        => GetIndexPrefix() + _separator + indexName;

    private string GetIndexPrefix()
    {
        if (_indexPrefix == null)
        {
            var parts = new List<string>();

            if (!string.IsNullOrWhiteSpace(_elasticSearchOptions.IndexPrefix))
            {
                parts.Add(_elasticSearchOptions.IndexPrefix.ToLowerInvariant());
            }

            parts.Add(_shellSettings.Name.ToLowerInvariant());

            _indexPrefix = string.Join(_separator, parts);
        }

        return _indexPrefix;
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
                    Highlights = hit?.Highlight
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
                Highlights = hit.Highlight
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
