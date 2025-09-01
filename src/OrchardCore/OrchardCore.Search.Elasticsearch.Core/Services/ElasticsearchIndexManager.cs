using System.Collections.Concurrent;
using System.Text.Json.Nodes;
using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.Analysis;
using Elastic.Clients.Elasticsearch.IndexManagement;
using Elastic.Clients.Elasticsearch.Mapping;
using Elastic.Transport;
using Elastic.Transport.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OrchardCore.ContentManagement;
using OrchardCore.Entities;
using OrchardCore.Indexing;
using OrchardCore.Indexing.Models;
using OrchardCore.Modules;
using OrchardCore.Search.Elasticsearch.Core.Models;

namespace OrchardCore.Search.Elasticsearch.Core.Services;

public sealed class ElasticsearchIndexManager : IIndexManager
{
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

    private readonly ElasticsearchClient _elasticClient;
    private readonly IEnumerable<IIndexEvents> _indexEvents;
    private readonly IClock _clock;
    private readonly ILogger _logger;
    private readonly ElasticsearchOptions _elasticSearchOptions;
    private readonly ConcurrentDictionary<string, DateTime> _timestamps = new(StringComparer.OrdinalIgnoreCase);

    public ElasticsearchIndexManager(
        ElasticsearchClient elasticClient,
        IOptions<ElasticsearchOptions> elasticsearchOptions,
        IEnumerable<IIndexEvents> indexEvents,
        IClock clock,
        ILogger<ElasticsearchIndexManager> logger
        )
    {
        _elasticClient = elasticClient;
        _elasticSearchOptions = elasticsearchOptions.Value;
        _indexEvents = indexEvents;
        _clock = clock;
        _logger = logger;
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
    public async Task<bool> CreateAsync(IndexProfile index)
    {
        if (await ExistsAsync(index.IndexFullName))
        {
            return false;
        }

        var context = new IndexCreateContext(index);

        await _indexEvents.InvokeAsync((handler, ctx) => handler.CreatingAsync(ctx), context, _logger);

        var createIndexRequest = GetCreateIndexRequest(index);
        var response = await _elasticClient.Indices.CreateAsync(createIndexRequest);

        if (!response.IsValidResponse)
        {
            if (response.TryGetOriginalException(out var ex))
            {
                _logger.LogError(ex, "There were issues creating an index in Elasticsearch");
            }
            else
            {
                _logger.LogWarning("There were issues creating an index in Elasticsearch");
            }

            return false;
        }

        await _indexEvents.InvokeAsync((handler, ctx) => handler.CreatedAsync(ctx), context, _logger);

        return true;
    }

    public async Task<string> GetIndexSettingsAsync(string indexFullName)
    {
        ArgumentNullException.ThrowIfNull(indexFullName);

        var response = await _elasticClient.Indices.GetAsync<GetIndexResponse>(descriptor => descriptor
            .Indices(indexFullName)
        );

        if (!response.IsValidResponse)
        {
            if (response.TryGetOriginalException(out var ex))
            {
                _logger.LogError(ex, "There were issues retrieving index settings from Elasticsearch");
            }
            else
            {
                _logger.LogWarning("There were issues retrieving index settings from Elasticsearch");
            }

            return null;
        }

        var setting = response.Indices[indexFullName].Settings;

        return _elasticClient.RequestResponseSerializer.SerializeToString(setting);
    }

    public async Task<string> GetIndexInfoAsync(string indexFullName)
    {
        ArgumentNullException.ThrowIfNull(indexFullName);

        var response = await _elasticClient.Indices.GetAsync<GetIndexResponse>(descriptor => descriptor
            .Indices(indexFullName)
        );

        if (!response.IsValidResponse)
        {
            if (response.TryGetOriginalException(out var ex))
            {
                _logger.LogError(ex, "There were issues retrieving index info from Elasticsearch");
            }
            else
            {
                _logger.LogWarning("There were issues retrieving index info from Elasticsearch");
            }

            return null;
        }

        var info = response.Indices[indexFullName];

        return _elasticClient.RequestResponseSerializer.SerializeToString(info);
    }

    public async Task<bool> DeleteAsync(IndexProfile index)
    {
        ArgumentNullException.ThrowIfNull(index);

        var context = new IndexRemoveContext(index.IndexFullName);

        await _indexEvents.InvokeAsync((handler, ctx) => handler.RemovingAsync(ctx), context, _logger);

        var request = new DeleteIndexRequest(index.IndexFullName);

        var response = await _elasticClient.Indices.DeleteAsync(request);

        if (!response.IsValidResponse)
        {
            if (response.TryGetOriginalException(out var ex))
            {
                _logger.LogError(ex, "There were issues deleting an index in Elasticsearch");
            }
            else if (response.ApiCallDetails.HttpStatusCode != 404)
            {
                _logger.LogWarning("There were issues deleting an index in Elasticsearch");
            }

            return false;
        }

        await _indexEvents.InvokeAsync((handler, ctx) => handler.RemovedAsync(ctx), context, _logger);

        return response.Acknowledged;
    }

    public async Task<bool> RebuildAsync(IndexProfile index)
    {
        ArgumentNullException.ThrowIfNull(index);

        var context = new IndexRebuildContext(index);

        await _indexEvents.InvokeAsync((handler, ctx) => handler.RebuildingAsync(ctx), context, _logger);

        if (await ExistsAsync(index.IndexFullName))
        {
            var deleteRequest = new DeleteIndexRequest(index.IndexFullName);

            var deleteResponse = await _elasticClient.Indices.DeleteAsync(deleteRequest);

            if (!deleteResponse.IsValidResponse)
            {
                if (deleteResponse.TryGetOriginalException(out var ex))
                {
                    _logger.LogError(ex, "There were issues removing an index in Elasticsearch");
                }
                else
                {
                    _logger.LogWarning("There were issues removing an index in Elasticsearch");
                }
            }
        }

        var createIndexRequest = GetCreateIndexRequest(index);

        var response = await _elasticClient.Indices.CreateAsync(createIndexRequest);

        if (!response.IsValidResponse)
        {
            if (response.TryGetOriginalException(out var ex))
            {
                _logger.LogError(ex, "There were issues creating an index in Elasticsearch");
            }
            else
            {
                _logger.LogWarning("There were issues creating an index in Elasticsearch");
            }

            return false;
        }

        await _indexEvents.InvokeAsync((handler, ctx) => handler.RebuiltAsync(ctx), context, _logger);

        return response.Acknowledged;
    }

    /// <summary>
    /// Verify if an index exists for the current tenant.
    /// </summary>
    public async Task<bool> ExistsAsync(string indexFullName)
    {
        ArgumentException.ThrowIfNullOrEmpty(indexFullName);

        var request = new Elastic.Clients.Elasticsearch.IndexManagement.ExistsRequest(indexFullName);

        var response = await _elasticClient.Indices.ExistsAsync(request);

        if (!response.IsValidResponse)
        {
            if (response.TryGetOriginalException(out var ex))
            {
                _logger.LogError(ex, "There were issues checking if an index in Elasticsearch Exists");
            }
            else if (response.ApiCallDetails.HttpStatusCode != 404)
            {
                _logger.LogWarning("There were issues checking if an index in Elasticsearch Exists");
            }
        }

        return response.Exists;
    }

    public async Task<ElasticsearchResult> SearchAsync(ElasticsearchSearchContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        var elasticTopDocs = new ElasticsearchResult()
        {
            TopDocs = [],
        };

        if (await ExistsAsync(context.IndexProfile.IndexFullName))
        {
            context.SearchRequest.Indices = context.IndexProfile.IndexFullName;

            var searchResponse = await _elasticClient.SearchAsync<JsonObject>(context.SearchRequest);

            if (!searchResponse.IsValidResponse)
            {
                if (searchResponse.TryGetOriginalException(out var ex))
                {
                    _logger.LogError(ex, "There were issues creating an index in Elasticsearch");
                }
                else
                {
                    _logger.LogWarning("There were issues creating an index in Elasticsearch");
                }
            }
            else
            {
                ProcessSuccessfulSearchResponse(context.IndexProfile, elasticTopDocs, searchResponse);
            }

            _timestamps[context.IndexProfile.IndexFullName] = _clock.UtcNow;
        }

        return elasticTopDocs;
    }

    /// <summary>
    /// Returns results from a search made with NEST Fluent DSL query.
    /// </summary>
    public async Task SearchAsync(IndexProfile index, Func<ElasticsearchClient, Task> elasticClient)
    {
        ArgumentNullException.ThrowIfNull(index);
        ArgumentNullException.ThrowIfNull(elasticClient);

        if (await ExistsAsync(index.IndexFullName))
        {
            await elasticClient(_elasticClient);

            _timestamps[index.IndexFullName] = _clock.UtcNow;
        }
    }

    internal async Task<ElasticsearchResult> SearchAsync(IndexProfile indexProfile, string query)
    {
        ArgumentNullException.ThrowIfNull(indexProfile);
        ArgumentException.ThrowIfNullOrEmpty(query);

        var elasticTopDocs = new ElasticsearchResult()
        {
            TopDocs = [],
        };

        if (await ExistsAsync(indexProfile.IndexFullName))
        {
            var response = await _elasticClient.Transport
                .RequestAsync<SearchResponse<JsonObject>>(Elastic.Transport.HttpMethod.GET, $"{indexProfile.IndexFullName}/_search", postData: PostData.String(query));

            if (!response.IsValidResponse)
            {
                if (response.TryGetOriginalException(out var ex))
                {
                    _logger.LogError(ex, "There were issues creating an index in Elasticsearch");
                }
                else
                {
                    _logger.LogWarning("There were issues creating an index in Elasticsearch");
                }
            }
            else
            {
                ProcessSuccessfulSearchResponse(indexProfile, elasticTopDocs, response);
            }

            _timestamps[indexProfile.IndexFullName] = _clock.UtcNow;
        }

        return elasticTopDocs;
    }

    private CreateIndexRequest GetCreateIndexRequest(IndexProfile indexProfile)
    {
        var indexSettings = new IndexSettings
        {
            Analysis = new IndexSettingsAnalysis()
            {
                Analyzers = new Analyzers(),
                TokenFilters = new TokenFilters(),
            },
        };

        var metadata = indexProfile.As<ElasticsearchIndexMetadata>();

        var analyzerName = metadata.GetAnalyzerName();

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

        if (metadata.IndexMappings is null)
        {
            throw new InvalidOperationException("Index mappings cannot be null.");
        }

        var createIndexRequest = new CreateIndexRequest(indexProfile.IndexFullName)
        {
            Settings = indexSettings,
            Mappings = metadata.IndexMappings.Mapping ?? new TypeMapping(),
        };

        // Custom metadata to store the last indexing task id.
        createIndexRequest.Mappings.Meta ??= new FluentDictionary<string, object>();

        createIndexRequest.Mappings.Meta[ElasticsearchConstants.LastTaskIdMetadataKey] = 0;

        return createIndexRequest;
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

    private static void ProcessSuccessfulSearchResponse(IndexProfile indexProfile, ElasticsearchResult elasticTopDocs, SearchResponse<JsonObject> searchResponse)
    {
        elasticTopDocs.Count = searchResponse.Hits.Count;
        elasticTopDocs.TotalCount = searchResponse.HitsMetadata?.Total?.Value2 ?? 0;

        var metadata = indexProfile.As<ElasticsearchIndexMetadata>();
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
                { metadata.IndexMappings.KeyFieldName, hit.Id },
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
