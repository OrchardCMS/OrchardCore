using System.Collections.Concurrent;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Nodes;
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

namespace OrchardCore.Search.Elasticsearch.Core.Services;

/// <summary>
/// Provides methods to manage Elasticsearch indices.
/// </summary>
public sealed class ElasticIndexManager
{
    private const string _separator = "_";

    private readonly IElasticClient _elasticClient;
    private readonly ShellSettings _shellSettings;
    private readonly IClock _clock;
    private readonly ILogger _logger;
    private readonly ElasticsearchOptions _elasticsearchOptions;
    private readonly ConcurrentDictionary<string, DateTime> _timestamps = new(StringComparer.OrdinalIgnoreCase);
    private readonly string _lastTaskId = "last_task_id";
    private readonly Dictionary<string, Func<IAnalyzer>> _analyzerGetter = new(StringComparer.OrdinalIgnoreCase)
    {
        { "standard", () => new StandardAnalyzer() },
        { "simple", () => new SimpleAnalyzer() },
        { "keyword", () => new KeywordAnalyzer() },
        { "whitespace", () => new WhitespaceAnalyzer() },
        { "pattern", () => new PatternAnalyzer() },
        { "language", () => new LanguageAnalyzer() },
        { "fingerprint", () => new FingerprintAnalyzer() },
        { "custom", () => new CustomAnalyzer() },
        { "stop", () => new StopAnalyzer() },
    };

    private List<string> _tokenFilterNames = new List<string>()
    {
        "asciifolding",
        "common_grams",
        "condition",
        "delimited_payload",
        "dictionary_decompounder",
        "edge_ngram",
        "elision",
        "fingerprint",
        "hunspell",
        "hyphenation_decompounder",
        "icu_collation",
        "icu_folding",
        "icu_normalizer",
        "icu_transform",
        "keep_types",
        "keep",
        "keyword_marker",
        "kstem",
        "kuromoji_part_of_speech",
        "kuromoji_readingform",
        "kuromoji_stemmer",
        "length",
        "limit",
        "lowercase",
        "multiplexer",
        "ngram",
        "nori_part_of_speech",
        "pattern_capture",
        "pattern_replace",
        "phonetic",
        "porter_stem",
        "predicate_token_filter",
        "remove_duplicates",
        "reverse",
        "shingle",
        "snowball",
        "stemmer_override",
        "stemmer",
        "stop",
        "synonym_graph",
        "synonym",
        "trim",
        "truncate",
        "unique",
        "uppercase",
        "word_delimiter_graph",
        "word_delimiter"
    };
    private sealed record TokenFilterBuildingInfo(ITokenFilter TokenFilter, Func<TokenFiltersDescriptor, ITokenFilter, string, TokenFiltersDescriptor> AddTokenFilter);
    private readonly Dictionary<string, TokenFilterBuildingInfo> _tokenFilterBuildingInfoGetter = new(StringComparer.OrdinalIgnoreCase)
    {
        {
            "asciifolding",
            new TokenFilterBuildingInfo( new AsciiFoldingTokenFilter(),
                (TokenFiltersDescriptor d, ITokenFilter tokenFilter, string name) =>
                    d.AsciiFolding(name, f => (AsciiFoldingTokenFilter)tokenFilter) )
        },
        {
            "common_grams",
            new TokenFilterBuildingInfo( new CommonGramsTokenFilter(),
                (TokenFiltersDescriptor d, ITokenFilter tokenFilter, string name) =>
                    d.CommonGrams(name, f => (CommonGramsTokenFilter)tokenFilter) )
        },
        {
            "condition",
            new TokenFilterBuildingInfo( new ConditionTokenFilter(),
                (TokenFiltersDescriptor d, ITokenFilter tokenFilter, string name) =>
                    d.Condition(name, f => (ConditionTokenFilter)tokenFilter) )
        },
        {
            "delimited_payload",
            new TokenFilterBuildingInfo( new DelimitedPayloadTokenFilter(),
                (TokenFiltersDescriptor d, ITokenFilter tokenFilter, string name) =>
                    d.DelimitedPayload(name, f => (DelimitedPayloadTokenFilter)tokenFilter) )
        },
        {
            "dictionary_decompounder",
            new TokenFilterBuildingInfo( new DictionaryDecompounderTokenFilter(),
                (TokenFiltersDescriptor d, ITokenFilter tokenFilter, string name) =>
                    d.DictionaryDecompounder(name, f => (DictionaryDecompounderTokenFilter)tokenFilter) )
        },
        {
            "edge_ngram",
            new TokenFilterBuildingInfo( new EdgeNGramTokenFilter(),
                (TokenFiltersDescriptor d, ITokenFilter tokenFilter, string name) =>
                    d.EdgeNGram(name, f => (EdgeNGramTokenFilter)tokenFilter) )
        },
        {
            "elision",
            new TokenFilterBuildingInfo( new ElisionTokenFilter(),
                (TokenFiltersDescriptor d, ITokenFilter tokenFilter, string name) =>
                    d.Elision(name, f => (ElisionTokenFilter)tokenFilter) )
        },
        {
            "fingerprint",
            new TokenFilterBuildingInfo( new FingerprintTokenFilter(),
                (TokenFiltersDescriptor d, ITokenFilter tokenFilter, string name) =>
                    d.Fingerprint(name, f => (FingerprintTokenFilter)tokenFilter) )
        },
        {
            "hunspell",
            new TokenFilterBuildingInfo( new HunspellTokenFilter(),
                (TokenFiltersDescriptor d, ITokenFilter tokenFilter, string name) =>
                    d.Hunspell(name, f => (HunspellTokenFilter)tokenFilter) )
        },
        {
            "hyphenation_decompounder",
            new TokenFilterBuildingInfo( new HyphenationDecompounderTokenFilter(),
                (TokenFiltersDescriptor d, ITokenFilter tokenFilter, string name) =>
                    d.HyphenationDecompounder(name, f => (HyphenationDecompounderTokenFilter)tokenFilter) )
        },
        {
            "icu_collation",
            new TokenFilterBuildingInfo( new IcuCollationTokenFilter(),
                (TokenFiltersDescriptor d, ITokenFilter tokenFilter, string name) =>
                    d.IcuCollation(name, f => (IcuCollationTokenFilter)tokenFilter) )
        },
        {
            "icu_folding",
            new TokenFilterBuildingInfo( new IcuFoldingTokenFilter(),
                (TokenFiltersDescriptor d, ITokenFilter tokenFilter, string name) =>
                    d.IcuFolding(name, f => (IcuFoldingTokenFilter)tokenFilter) )
        },
        {
            "icu_normalizer",
            new TokenFilterBuildingInfo( new IcuNormalizationTokenFilter(),
                (TokenFiltersDescriptor d, ITokenFilter tokenFilter, string name) =>
                    d.IcuNormalization(name, f => (IcuNormalizationTokenFilter)tokenFilter) )
        },
        {
            "icu_transform",
            new TokenFilterBuildingInfo( new IcuTransformTokenFilter(),
                (TokenFiltersDescriptor d, ITokenFilter tokenFilter, string name) =>
                    d.IcuTransform(name, f => (IcuTransformTokenFilter)tokenFilter) )
        },
        {
            "keep_types",
            new TokenFilterBuildingInfo( new KeepTypesTokenFilter(),
                (TokenFiltersDescriptor d, ITokenFilter tokenFilter, string name) =>
                    d.KeepTypes(name, f => (KeepTypesTokenFilter)tokenFilter) )
        },
        {
            "keep",
            new TokenFilterBuildingInfo( new KeepWordsTokenFilter(),
                (TokenFiltersDescriptor d, ITokenFilter tokenFilter, string name) =>
                    d.KeepWords(name, f => (KeepWordsTokenFilter)tokenFilter) )
        },
        {
            "keyword_marker",
            new TokenFilterBuildingInfo( new KeywordMarkerTokenFilter(),
                (TokenFiltersDescriptor d, ITokenFilter tokenFilter, string name) =>
                    d.KeywordMarker(name, f => (KeywordMarkerTokenFilter)tokenFilter) )
        },
        {
            "kstem",
            new TokenFilterBuildingInfo( new KStemTokenFilter(),
                (TokenFiltersDescriptor d, ITokenFilter tokenFilter, string name) =>
                    d.KStem(name, f => (KStemTokenFilter)tokenFilter) )
        },
        {
            "kuromoji_part_of_speech",
            new TokenFilterBuildingInfo( new KuromojiPartOfSpeechTokenFilter(),
                (TokenFiltersDescriptor d, ITokenFilter tokenFilter, string name) =>
                    d.KuromojiPartOfSpeech(name, f => (KuromojiPartOfSpeechTokenFilter)tokenFilter) )
        },
        {
            "kuromoji_readingform",
            new TokenFilterBuildingInfo( new KuromojiReadingFormTokenFilter(),
                (TokenFiltersDescriptor d, ITokenFilter tokenFilter, string name) =>
                    d.KuromojiReadingForm(name, f => (KuromojiReadingFormTokenFilter)tokenFilter) )
        },
        {
            "kuromoji_stemmer",
            new TokenFilterBuildingInfo( new KuromojiStemmerTokenFilter(),
                (TokenFiltersDescriptor d, ITokenFilter tokenFilter, string name) =>
                    d.KuromojiStemmer(name, f => (KuromojiStemmerTokenFilter)tokenFilter) )
        },
        {
            "length",
            new TokenFilterBuildingInfo( new LengthTokenFilter(),
                (TokenFiltersDescriptor d, ITokenFilter tokenFilter, string name) =>
                    d.Length(name, f => (LengthTokenFilter)tokenFilter) )
        },
        {
            "limit",
            new TokenFilterBuildingInfo( new LimitTokenCountTokenFilter(),
                (TokenFiltersDescriptor d, ITokenFilter tokenFilter, string name) =>
                    d.LimitTokenCount(name, f => (LimitTokenCountTokenFilter)tokenFilter) )
        },
        {
            "lowercase",
            new TokenFilterBuildingInfo( new LowercaseTokenFilter(),
                (TokenFiltersDescriptor d, ITokenFilter tokenFilter, string name) =>
                    d.Lowercase(name, f => (LowercaseTokenFilter)tokenFilter) )
        },
        {
            "multiplexer",
            new TokenFilterBuildingInfo( new MultiplexerTokenFilter(),
                (TokenFiltersDescriptor d, ITokenFilter tokenFilter, string name) =>
                    d.Multiplexer(name, f => (MultiplexerTokenFilter)tokenFilter) )
        },
        {
            "ngram",
            new TokenFilterBuildingInfo( new NGramTokenFilter(),
                (TokenFiltersDescriptor d, ITokenFilter tokenFilter, string name) =>
                    d.NGram(name, f => (NGramTokenFilter)tokenFilter) )
        },
        {
            "nori_part_of_speech",
            new TokenFilterBuildingInfo( new NoriPartOfSpeechTokenFilter(),
                (TokenFiltersDescriptor d, ITokenFilter tokenFilter, string name) =>
                    d.NoriPartOfSpeech(name, f => (NoriPartOfSpeechTokenFilter)tokenFilter) )
        },
        {
            "pattern_capture",
            new TokenFilterBuildingInfo( new PatternCaptureTokenFilter(),
                (TokenFiltersDescriptor d, ITokenFilter tokenFilter, string name) =>
                    d.PatternCapture(name, f => (PatternCaptureTokenFilter)tokenFilter) )
        },
        {
            "pattern_replace",
            new TokenFilterBuildingInfo( new PatternReplaceTokenFilter(),
                (TokenFiltersDescriptor d, ITokenFilter tokenFilter, string name) =>
                    d.PatternReplace(name, f => (PatternReplaceTokenFilter)tokenFilter) )
        },
        {
            "phonetic",
            new TokenFilterBuildingInfo( new PhoneticTokenFilter(),
                (TokenFiltersDescriptor d, ITokenFilter tokenFilter, string name) =>
                    d.Phonetic(name, f => (PhoneticTokenFilter)tokenFilter) )
        },
        {
            "porter_stem",
            new TokenFilterBuildingInfo( new PorterStemTokenFilter(),
                (TokenFiltersDescriptor d, ITokenFilter tokenFilter, string name) =>
                    d.PorterStem(name, f => (PorterStemTokenFilter)tokenFilter) )
        },
        {
            "predicate_token_filter",
            new TokenFilterBuildingInfo( new PredicateTokenFilter(),
                (TokenFiltersDescriptor d, ITokenFilter tokenFilter, string name) =>
                    d.Predicate(name, f => (PredicateTokenFilter)tokenFilter) )
        },
        {
            "remove_duplicates",
            new TokenFilterBuildingInfo( new RemoveDuplicatesTokenFilter(),
                (TokenFiltersDescriptor d, ITokenFilter tokenFilter, string name) =>
                    d.RemoveDuplicates(name, f => (RemoveDuplicatesTokenFilter)tokenFilter) )
        },
        {
            "reverse",
            new TokenFilterBuildingInfo( new ReverseTokenFilter(),
                (TokenFiltersDescriptor d, ITokenFilter tokenFilter, string name) =>
                    d.Reverse(name, f => (ReverseTokenFilter)tokenFilter) )
        },
        {
            "shingle",
            new TokenFilterBuildingInfo( new ShingleTokenFilter(),
                (TokenFiltersDescriptor d, ITokenFilter tokenFilter, string name) =>
                    d.Shingle(name, f => (ShingleTokenFilter)tokenFilter) )
        },
        {
            "snowball",
            new TokenFilterBuildingInfo( new SnowballTokenFilter(),
                (TokenFiltersDescriptor d, ITokenFilter tokenFilter, string name) =>
                    d.Snowball(name, f => (SnowballTokenFilter)tokenFilter) )
        },
        {
            "stemmer_override",
            new TokenFilterBuildingInfo( new StemmerOverrideTokenFilterDescriptor(),
                (TokenFiltersDescriptor d, ITokenFilter tokenFilter, string name) =>
                    d.StemmerOverride(name, f => (StemmerOverrideTokenFilterDescriptor)tokenFilter) )
        },
        {
            "stemmer",
            new TokenFilterBuildingInfo( new StemmerTokenFilter(),
                (TokenFiltersDescriptor d, ITokenFilter tokenFilter, string name) =>
                    d.Stemmer(name, f => (StemmerTokenFilter)tokenFilter) )
        },
        {
            "stop",
            new TokenFilterBuildingInfo( new StopTokenFilter(),
                (TokenFiltersDescriptor d, ITokenFilter tokenFilter, string name) =>
                    d.Stop(name, f => (StopTokenFilter)tokenFilter) )
        },
        {
            "synonym_graph",
            new TokenFilterBuildingInfo( new SynonymGraphTokenFilter(),
                (TokenFiltersDescriptor d, ITokenFilter tokenFilter, string name) =>
                    d.SynonymGraph(name, f => (SynonymGraphTokenFilter)tokenFilter) )
        },
        {
            "synonym",
            new TokenFilterBuildingInfo( new SynonymTokenFilter(),
                (TokenFiltersDescriptor d, ITokenFilter tokenFilter, string name) =>
                    d.Synonym(name, f => (SynonymTokenFilter)tokenFilter) )
        },
        {
            "trim",
            new TokenFilterBuildingInfo( new TrimTokenFilter(),
                (TokenFiltersDescriptor d, ITokenFilter tokenFilter, string name) =>
                    d.Trim(name, f => (TrimTokenFilter)tokenFilter) )
        },
        {
            "truncate",
            new TokenFilterBuildingInfo( new TruncateTokenFilter(),
                (TokenFiltersDescriptor d, ITokenFilter tokenFilter, string name) =>
                    d.Truncate(name, f => (TruncateTokenFilter)tokenFilter) )
        },
        {
            "unique",
            new TokenFilterBuildingInfo( new UniqueTokenFilter(),
                (TokenFiltersDescriptor d, ITokenFilter tokenFilter, string name) =>
                    d.Unique(name, f => (UniqueTokenFilter)tokenFilter) )
        },
        {
            "uppercase",
            new TokenFilterBuildingInfo( new UppercaseTokenFilter(),
                (TokenFiltersDescriptor d, ITokenFilter tokenFilter, string name) =>
                    d.Uppercase(name, f => (UppercaseTokenFilter)tokenFilter) )
        },
        {
            "word_delimiter_graph",
            new TokenFilterBuildingInfo( new WordDelimiterGraphTokenFilter(),
                (TokenFiltersDescriptor d, ITokenFilter tokenFilter, string name) =>
                    d.WordDelimiterGraph(name, f => (WordDelimiterGraphTokenFilter)tokenFilter) )
        },
        {
            "word_delimiter",
            new TokenFilterBuildingInfo( new WordDelimiterTokenFilter(),
                (TokenFiltersDescriptor d, ITokenFilter tokenFilter, string name) =>
                    d.WordDelimiter(name, f => (WordDelimiterTokenFilter)tokenFilter) )
        }
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
    /// <returns><see cref="bool"/>.</returns>
    public async Task<bool> CreateIndexAsync(ElasticIndexSettings elasticIndexSettings)
    {
        // Get Index name scoped by ShellName
        if (await ExistsAsync(elasticIndexSettings.IndexName))
        {
            return true;
        }

        var analysisDescriptor = new AnalysisDescriptor();
        var analyzersDescriptor = new AnalyzersDescriptor();
        var indexSettingsDescriptor = new IndexSettingsDescriptor();

        // The name "standardanalyzer" is a legacy used prior OC 1.6 release. It can be removed in future releases.
        var analyzerName = (elasticIndexSettings.AnalyzerName == "standardanalyzer" ? null : elasticIndexSettings.AnalyzerName) ?? "standard";

        if (_elasticsearchOptions.Analyzers.TryGetValue(analyzerName, out var analyzerProperties))
        {
            if (_elasticsearchOptions.Filter is not null)
            {
                var tokenFiltersDescriptor = GetTokenFilterDescriptor(_elasticsearchOptions.Filter);
                analysisDescriptor.TokenFilters(f => tokenFiltersDescriptor);
            }

            if (analyzerProperties.TryGetPropertyValue("filter", out var filterResult))
            {
                var filterCollection = JsonSerializer.Deserialize<IEnumerable<string>>(filterResult);

                var existingFilters = filterCollection.Where(f => _tokenFilterNames.Contains(f));
                analysisDescriptor.Analyzers(a => a.Custom("default", c => c.Tokenizer("standard").Filters(existingFilters)));
            }
            else
            {
                var analyzer = CreateAnalyzer(analyzerProperties);
                analysisDescriptor.Analyzers(a => a.UserDefined(analyzerName, analyzer));
            }               

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
                    .Excludes([IndexingConstants.DisplayTextAnalyzedKey]))
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

    private TokenFiltersDescriptor GetTokenFilterDescriptor(Dictionary<string, JsonObject> filter)
    {
        var descriptor = new TokenFiltersDescriptor();

        foreach (var filterName in filter.Keys)
        {
            var filterProps = filter[filterName];

            if (filterProps.TryGetPropertyValue("type", out var typeObject) is false
             || _tokenFilterBuildingInfoGetter.TryGetValue(typeObject.ToString(), out var tokenFilterBuildingInfo) is false)
            {
                continue;
            }

            var properties = tokenFilterBuildingInfo.TokenFilter.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (var filterProperty in filterProps)
            {
                if (filterProperty.Value == null || string.Equals(filterProperty.Key, "type", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                var key = filterProperty.Key.Replace("_", string.Empty);

                var property = properties.FirstOrDefault(p => p.Name.Equals(key, StringComparison.OrdinalIgnoreCase));
                var propertyType = property.PropertyType;

                if (property == null)
                {
                    continue;
                }

                try
                {
                    if (property.PropertyType == typeof(StopWords))
                    {
                        if (filterProperty.Value is JsonArray)
                        {
                            var propertyValue = JsonSerializer.Deserialize<IEnumerable<string>>(filterProperty.Value);
                            property.SetValue(tokenFilterBuildingInfo.TokenFilter, new StopWords(propertyValue));
                        }
                        else
                        {
                            var propertyValue = JsonSerializer.Deserialize<string>(filterProperty.Value);
                            property.SetValue(tokenFilterBuildingInfo.TokenFilter, new StopWords(propertyValue));
                        }

                        tokenFilterBuildingInfo.AddTokenFilter(descriptor, tokenFilterBuildingInfo.TokenFilter, filterName);
                        _tokenFilterNames.Add(filterName);

                        continue;
                    }

                    if (filterProperty.Value is JsonArray jsonArray)
                    {
                        var propertyValue = JsonSerializer.Deserialize(filterProperty.Value, propertyType);
                        property.SetValue(tokenFilterBuildingInfo.TokenFilter, propertyValue);
                    }
                    else
                    {
                        var propertyValue = JsonSerializer.Deserialize(filterProperty.Value, propertyType);
                        property.SetValue(tokenFilterBuildingInfo.TokenFilter, propertyValue);
                    }

                    tokenFilterBuildingInfo.AddTokenFilter(descriptor, tokenFilterBuildingInfo.TokenFilter, filterName);
                    _tokenFilterNames.Add(filterName);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }

        }

        return descriptor;
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
                if (analyzerProperty.Value == null || string.Equals(analyzerProperty.Key, "type", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                var key = analyzerProperty.Key.Replace(_separator, string.Empty);

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
                            var values = analyzerProperty.Value.Values<string>().ToArray();

                            property.SetValue(analyzer, new StopWords(values));
                        }

                        continue;
                    }

                    if (analyzerProperty.Value is JsonArray jsonArray)
                    {
                        var values = jsonArray.Values<string>().ToArray();

                        property.SetValue(analyzer, values);
                    }
                    else
                    {
                        var value = JNode.ToObject(analyzerProperty.Value, property.PropertyType);

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

        if (!response.Success)
        {
            _logger.LogWarning("There were issues retrieving index mappings from Elasticsearch. {OriginalException}", response.OriginalException);
        }

        return response.Body;
    }

    public async Task<string> GetIndexSettings(string indexName)
    {
        var response = await _elasticClient.LowLevel.Indices.GetSettingsAsync<StringResponse>(GetFullIndexName(indexName));

        if (!response.Success)
        {
            _logger.LogWarning("There were issues retrieving index settings from Elasticsearch. {OriginalException}", response.OriginalException);
        }

        return response.Body;
    }

    public async Task<string> GetIndexInfo(string indexName)
    {
        var response = await _elasticClient.LowLevel.Indices.GetAsync<StringResponse>(GetFullIndexName(indexName));

        if (!response.Success)
        {
            _logger.LogWarning("There were issues retrieving index info from Elasticsearch. {OriginalException}", response.OriginalException);
        }

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
            {
                descriptor.Delete<Dictionary<string, object>>(d => d
                    .Index(GetFullIndexName(indexName))
                    .Id(id)
                );
            }

            var response = await _elasticClient.BulkAsync(descriptor);

            if (response.Errors)
            {
                _logger.LogWarning("There were issues deleting documents from Elasticsearch. {OriginalException}", response.OriginalException);
            }

            success = response.IsValid;
        }

        return success;
    }

    /// <summary>
    /// Deletes all documents in an index in one request.
    /// <see href="https://www.elastic.co/guide/en/elasticsearch/reference/master/docs-delete-by-query.html"/>.
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

        _charsToRemove.ForEach(c => indexName = indexName.Replace(c.ToString(), string.Empty));

        return indexName;
    }

    public async Task StoreDocumentsAsync(string indexName, IEnumerable<DocumentIndex> indexDocuments)
    {
        var documents = new List<Dictionary<string, object>>();

        foreach (var indexDocument in indexDocuments)
        {
            documents.Add(CreateElasticDocument(indexDocument));
        }

        if (documents.Count > 0)
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
                _logger.LogWarning("There were issues reported indexing the documents. {ServerError}", result.ServerError);
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

        if (await ExistsAsync(indexName))
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
        if (await ExistsAsync(indexName))
        {
            await elasticClient(_elasticClient);

            _timestamps[GetFullIndexName(indexName)] = _clock.UtcNow;
        }
    }

    private static Dictionary<string, object> CreateElasticDocument(DocumentIndex documentIndex)
    {
        var entries = new Dictionary<string, object>
        {
            { IndexingConstants.ContentItemIdKey, documentIndex.ContentItemId },
            { IndexingConstants.ContentItemVersionIdKey, documentIndex.ContentItemVersionId }
        };

        foreach (var entry in documentIndex.Entries)
        {
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
                    if (entry.Value != null && long.TryParse(entry.Value.ToString(), out var value))
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

                        if (!string.IsNullOrEmpty(stringValue))
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
        ArgumentException.ThrowIfNullOrEmpty(indexName);

        return GetIndexPrefix() + _separator + indexName;
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

            if (!string.IsNullOrWhiteSpace(_elasticsearchOptions.IndexPrefix))
            {
                parts.Add(_elasticsearchOptions.IndexPrefix.ToLowerInvariant());
            }

            parts.Add(_shellSettings.Name.ToLowerInvariant());

            _indexPrefix = string.Join(_separator, parts);
        }

        return _indexPrefix;
    }
}
