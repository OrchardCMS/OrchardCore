using System.Collections.Concurrent;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Nodes;
using Elasticsearch.Net;
using Json.Path;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Nest;
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
public sealed class ElasticIndexManager
{
    private const string _separator = "_";

    private readonly IElasticClient _elasticClient;
    private readonly ShellSettings _shellSettings;
    private readonly IClock _clock;
    private readonly ILogger _logger;
    private readonly ElasticsearchOptions _elasticSearchOptions;
    private readonly ConcurrentDictionary<string, DateTime> _timestamps = new(StringComparer.OrdinalIgnoreCase);
    private readonly string _lastTaskId = "last_task_id";

    private static readonly Dictionary<string, Func<IAnalyzer>> _analyzerGetter = new(StringComparer.OrdinalIgnoreCase)
    {
        { ElasticsearchConstants.DefaultAnalyzer, () => new StandardAnalyzer() },
        { ElasticsearchConstants.SimpleAnalyzer, () => new SimpleAnalyzer() },
        { ElasticsearchConstants.KeywordAnalyzer, () => new KeywordAnalyzer() },
        { ElasticsearchConstants.WhitespaceAnalyzer, () => new WhitespaceAnalyzer() },
        { ElasticsearchConstants.PatternAnalyzer, () => new PatternAnalyzer() },
        { ElasticsearchConstants.LanguageAnalyzer, () => new LanguageAnalyzer() },
        { ElasticsearchConstants.FingerprintAnalyzer, () => new FingerprintAnalyzer() },
        { ElasticsearchConstants.CustomAnalyzer, () => new CustomAnalyzer() },
        { ElasticsearchConstants.StopAnalyzer, () => new StopAnalyzer() },
    };

    private static readonly Dictionary<string, Func<ITokenFilter>> _tokenFilterGetter = new(StringComparer.OrdinalIgnoreCase)
    {
        { "asciifolding", () => new AsciiFoldingTokenFilter() },
        { "common_grams", () => new CommonGramsTokenFilter() },
        { "condition", () => new ConditionTokenFilter() },
        { "delimited_payload", () => new DelimitedPayloadTokenFilter() },
        { "dictionary_decompounder", () => new DictionaryDecompounderTokenFilter() },
        { "edge_ngram", () => new EdgeNGramTokenFilter() },
        { "elision", () => new ElisionTokenFilter() },
        { "fingerprint", () => new FingerprintTokenFilter() },
        { "hunspell", () => new HunspellTokenFilter() },
        { "hyphenation_decompounder", () => new HyphenationDecompounderTokenFilter() },
        { "icu_collation", () => new IcuCollationTokenFilter() },
        { "icu_folding", () => new IcuFoldingTokenFilter() },
        { "icu_normalizer", () => new IcuNormalizationTokenFilter() },
        { "icu_transform", () => new IcuTransformTokenFilter() },
        { "keep_types", () => new KeepTypesTokenFilter() },
        { "keep", () => new KeepWordsTokenFilter() },
        { "keyword_marker", () => new KeywordMarkerTokenFilter() },
        { "kstem", () => new KStemTokenFilter() },
        { "kuromoji_part_of_speech", () => new KuromojiPartOfSpeechTokenFilter() },
        { "kuromoji_readingform", () => new KuromojiReadingFormTokenFilter() },
        { "kuromoji_stemmer", () => new KuromojiStemmerTokenFilter() },
        { "length", () => new LengthTokenFilter() },
        { "limit", () => new LimitTokenCountTokenFilter() },
        { "lowercase", () => new LowercaseTokenFilter() },
        { "multiplexer", () => new MultiplexerTokenFilter() },
        { "ngram", () => new NGramTokenFilter() },
        { "nori_part_of_speech", () => new NoriPartOfSpeechTokenFilter() },
        { "pattern_capture", () => new PatternCaptureTokenFilter() },
        { "pattern_replace", () => new PatternReplaceTokenFilter() },
        { "phonetic", () => new PhoneticTokenFilter() },
        { "porter_stem", () => new PorterStemTokenFilter() },
        { "remove_duplicates", () => new RemoveDuplicatesTokenFilter() },
        { "reverse", () => new ReverseTokenFilter() },
        { "shingle", () => new ShingleTokenFilter() },
        { "snowball", () => new SnowballTokenFilter() },
        { "stemmer_override", () => new StemmerOverrideTokenFilterDescriptor() },
        { "stemmer", () => new StemmerTokenFilter() },
        { "stop", () => new StopTokenFilter() },
        { "synonym_graph", () => new SynonymGraphTokenFilter() },
        { "synonym", () => new SynonymTokenFilter() },
        { "trim", () => new TrimTokenFilter() },
        { "truncate", () => new TruncateTokenFilter() },
        { "unique", () => new UniqueTokenFilter() },
        { "uppercase", () => new UppercaseTokenFilter() },
        { "word_delimiter_graph", () => new WordDelimiterGraphTokenFilter() },
        { "word_delimiter", () => new WordDelimiterTokenFilter() }
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

        var analysisDescriptor = new AnalysisDescriptor();
        var indexSettingsDescriptor = new IndexSettingsDescriptor();

        // The name "standardanalyzer" is a legacy used prior OC 1.6 release. It can be removed in future releases.
        var analyzerName = (elasticIndexSettings.AnalyzerName == "standardanalyzer"
        ? null
        : elasticIndexSettings.AnalyzerName) ?? ElasticsearchConstants.DefaultAnalyzer;

        if (_elasticSearchOptions.Analyzers is not null && _elasticSearchOptions.Analyzers.TryGetValue(analyzerName, out var analyzerProperties))
        {
            var analyzer = GetAnalyzer(analyzerProperties);

            analysisDescriptor.Analyzers(a => a.UserDefined(analyzerName, analyzer));
        }

        if (_elasticSearchOptions.TokenFilters is not null && _elasticSearchOptions.TokenFilters.Count > 0)
        {
            var tokenFiltersDescriptor = GetTokenFiltersDescriptor(_elasticSearchOptions.TokenFilters);

            analysisDescriptor.TokenFilters(d => tokenFiltersDescriptor);
        }

        indexSettingsDescriptor.Analysis(a => analysisDescriptor);

        // Custom metadata to store the last indexing task id.
        var IndexingState = new FluentDictionary<string, object>()
        {
            { _lastTaskId, 0 },
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
        // DynamicTemplates mapping for Geo fields, the GeoPointFieldIndexHandler adds a Location index by default.
        await _elasticClient.MapAsync<object>(p => p
            .Index(fullIndexName)
            .DynamicTemplates(d => d
                .DynamicTemplate("*.Location", dyn => dyn
                    .MatchMappingType("object")
                    .PathMatch("*" + ".Location")
                    .Mapping(m => m.GeoPoint(g => g))
                    )
                )
            );

        return response.Acknowledged;
    }

    private TokenFiltersDescriptor GetTokenFiltersDescriptor(Dictionary<string, JsonObject> filters)
    {
        var descriptor = new TokenFiltersDescriptor();

        foreach (var filter in filters)
        {
            if (!filter.Value.TryGetPropertyValue("type", out var typeObject) ||
                !_tokenFilterGetter.TryGetValue(typeObject.ToString(), out var tokenFilterBuildingInfo))
            {
                continue;
            }

            var tokenFilter = tokenFilterBuildingInfo.Invoke();

            var properties = tokenFilter.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (var filterProperty in filter.Value)
            {
                if (filterProperty.Value == null || string.Equals(filterProperty.Key, "type", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                var key = filterProperty.Key.Replace(_separator, string.Empty);
                var property = properties.FirstOrDefault(p => p.Name.Equals(key, StringComparison.OrdinalIgnoreCase));

                if (property == null)
                {
                    continue;
                }

                try
                {
                    PopulateValue(tokenFilter, property, filterProperty.Value);

                    descriptor.UserDefined(filter.Key, tokenFilter);
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "Unable to parse token filter for Elasticsearch (Filter: {Key}).", filter.Key);
                }
            }
        }

        return descriptor;
    }

    private IAnalyzer GetAnalyzer(JsonObject analyzerProperties)
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
                    PopulateValue(analyzer, property, analyzerProperty.Value);
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "Unable to parse an analyzer for Elasticsearch.");
                }
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
        var IndexingState = new FluentDictionary<string, object>()
        {
            { _lastTaskId, lastTaskId },
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
        ArgumentException.ThrowIfNullOrEmpty(indexName);
        ArgumentNullException.ThrowIfNull(query);

        var elasticTopDocs = new ElasticTopDocs()
        {
            TopDocs = [],
        };

        if (await ExistsAsync(indexName))
        {
            var fullIndexName = GetFullIndexName(indexName);

            var searchRequest = new SearchRequest(fullIndexName)
            {
                Query = query,
                From = from,
                Size = size,
                Sort = sort ?? [],
            };

            var searchResponse = await _elasticClient.SearchAsync<Dictionary<string, object>>(searchRequest);

            if (searchResponse.IsValid)
            {
                elasticTopDocs.Count = searchResponse.Hits.Count;

                var documents = searchResponse.Documents.GetEnumerator();
                var hits = searchResponse.Hits.GetEnumerator();

                while (documents.MoveNext() && hits.MoveNext())
                {
                    var document = documents.Current;

                    if (document != null)
                    {
                        elasticTopDocs.TopDocs.Add(document);

                        continue;
                    }

                    var hit = hits.Current;

                    var topDoc = new Dictionary<string, object>
                    {
                        { nameof(ContentItem.ContentItemId), hit.Id },
                    };

                    elasticTopDocs.TopDocs.Add(topDoc);
                }
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
        ArgumentException.ThrowIfNullOrEmpty(indexName);

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
                case DocumentIndexBase.Types.Boolean:
                    if (entry.Value is bool boolValue)
                    {
                        AddValue(entries, entry.Name, boolValue);
                    }
                    break;

                case DocumentIndexBase.Types.DateTime:

                    if (entry.Value is DateTimeOffset offsetValue)
                    {
                        AddValue(entries, entry.Name, offsetValue);
                    }
                    else if (entry.Value is DateTime dateTimeValue)
                    {
                        AddValue(entries, entry.Name, dateTimeValue.ToUniversalTime());
                    }

                    break;

                case DocumentIndexBase.Types.Integer:
                    if (entry.Value != null && long.TryParse(entry.Value.ToString(), out var value))
                    {
                        AddValue(entries, entry.Name, value);
                    }

                    break;

                case DocumentIndexBase.Types.Number:
                    if (entry.Value != null)
                    {
                        AddValue(entries, entry.Name, Convert.ToDouble(entry.Value));
                    }
                    break;

                case DocumentIndexBase.Types.Text:
                    if (entry.Value != null)
                    {
                        var stringValue = Convert.ToString(entry.Value);

                        if (!string.IsNullOrEmpty(stringValue))
                        {
                            AddValue(entries, entry.Name, stringValue);
                        }
                    }
                    break;
                case DocumentIndexBase.Types.GeoPoint:
                    if (entry.Value is DocumentIndexBase.GeoPoint point)
                    {
                        AddValue(entries, entry.Name, new GeoLocation((double)point.Latitude, (double)point.Longitude));
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

    private static void PopulateValue(object destination, PropertyInfo property, JsonNode value)
    {
        if (property.PropertyType == typeof(StopWords))
        {
            if (value is JsonArray)
            {
                var values = value.Values<string>().ToArray();

                property.SetValue(destination, new StopWords(values));
            }
            else if (value.TryGetValue<string>(out var saveValue))
            {
                property.SetValue(destination, new StopWords(saveValue));
            }

            return;
        }

        if (value is JsonArray jsonArray)
        {
            var values = jsonArray.Values<string>().ToArray();

            property.SetValue(destination, values);
        }
        else
        {
            var safeValue = JNode.ToObject(value, property.PropertyType);

            property.SetValue(destination, safeValue);
        }
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

            if (!string.IsNullOrWhiteSpace(_elasticSearchOptions.IndexPrefix))
            {
                parts.Add(_elasticSearchOptions.IndexPrefix.ToLowerInvariant());
            }

            parts.Add(_shellSettings.Name.ToLowerInvariant());

            _indexPrefix = string.Join(_separator, parts);
        }

        return _indexPrefix;
    }
}
