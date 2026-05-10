using System.Collections.Concurrent;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OpenSearch.Client;
using OpenSearch.Net;
using OrchardCore.ContentManagement;
using OrchardCore.Contents.Indexing;
using OrchardCore.Entities;
using OrchardCore.Indexing;
using OrchardCore.Indexing.Models;
using OrchardCore.Locking.Distributed;
using OrchardCore.Modules;
using OrchardCore.OpenSearch.Core.Models;
using OrchardCore.OpenSearch.Models;

namespace OrchardCore.OpenSearch.Core.Services;

public sealed class OpenSearchIndexManager : IIndexManager
{
    private static readonly Dictionary<string, Type> _analyzerGetter = new(StringComparer.OrdinalIgnoreCase)
    {
        { OpenSearchConstants.DefaultAnalyzer, typeof(StandardAnalyzer) },
        { OpenSearchConstants.SimpleAnalyzer, typeof(SimpleAnalyzer) },
        { OpenSearchConstants.KeywordAnalyzer, typeof(KeywordAnalyzer) },
        { OpenSearchConstants.WhitespaceAnalyzer, typeof(WhitespaceAnalyzer) },
        { OpenSearchConstants.PatternAnalyzer, typeof(PatternAnalyzer) },
        { OpenSearchConstants.FingerprintAnalyzer, typeof(FingerprintAnalyzer) },
        { OpenSearchConstants.CustomAnalyzer, typeof(CustomAnalyzer) },
        { OpenSearchConstants.StopAnalyzer, typeof(StopAnalyzer) },
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
        { "keep_types", typeof(KeepTypesTokenFilter) },
        { "keep", typeof(KeepWordsTokenFilter) },
        { "keyword_marker", typeof(KeywordMarkerTokenFilter) },
        { "kstem", typeof(KStemTokenFilter) },
        { "length", typeof(LengthTokenFilter) },
        { "limit", typeof(LimitTokenCountTokenFilter) },
        { "lowercase", typeof(LowercaseTokenFilter) },
        { "multiplexer", typeof(MultiplexerTokenFilter) },
        { "ngram", typeof(NGramTokenFilter) },
        { "pattern_capture", typeof(PatternCaptureTokenFilter) },
        { "pattern_replace", typeof(PatternReplaceTokenFilter) },
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

    private readonly OpenSearchClient _openSearchClient;
    private readonly IEnumerable<IIndexEvents> _indexEvents;
    private readonly IClock _clock;
    private readonly ILogger _logger;
    private readonly OpenSearchOptions _openSearchOptions;
    private readonly IDistributedLock _distributedLock;
    private readonly ConcurrentDictionary<string, DateTime> _timestamps = new(StringComparer.OrdinalIgnoreCase);

    public OpenSearchIndexManager(
        OpenSearchClient openSearchClient,
        IOptions<OpenSearchOptions> openSearchOptions,
        IEnumerable<IIndexEvents> indexEvents,
        IClock clock,
        ILogger<OpenSearchIndexManager> logger,
        IDistributedLock distributedLock)
    {
        _openSearchClient = openSearchClient;
        _openSearchOptions = openSearchOptions.Value;
        _indexEvents = indexEvents;
        _clock = clock;
        _logger = logger;
        _distributedLock = distributedLock;
    }

    public async Task<bool> CreateAsync(IndexProfile index)
    {
        if (await ExistsAsync(index.IndexFullName))
        {
            return false;
        }

        var context = new IndexCreateContext(index);

        await _indexEvents.InvokeAsync((handler, ctx) => handler.CreatingAsync(ctx), context, _logger);

        var createIndexDescriptor = BuildCreateIndexDescriptor(index);
        var response = await _openSearchClient.Indices.CreateAsync(index.IndexFullName, createIndexDescriptor);

        if (!response.IsValid)
        {
            if (response.OriginalException != null)
            {
                _logger.LogError(response.OriginalException, "There were issues creating an index in OpenSearch");
            }
            else
            {
                _logger.LogWarning("There were issues creating an index in OpenSearch");
            }

            return false;
        }

        await _indexEvents.InvokeAsync((handler, ctx) => handler.CreatedAsync(ctx), context, _logger);

        return true;
    }

    public async Task<string> GetIndexSettingsAsync(string indexFullName)
    {
        ArgumentNullException.ThrowIfNull(indexFullName);

        var response = await _openSearchClient.Indices.GetAsync(new GetIndexRequest(indexFullName));

        if (!response.IsValid)
        {
            if (response.OriginalException != null)
            {
                _logger.LogError(response.OriginalException, "There were issues retrieving index settings from OpenSearch");
            }
            else
            {
                _logger.LogWarning("There were issues retrieving index settings from OpenSearch");
            }

            return null;
        }

        var setting = response.Indices[indexFullName].Settings;

        return SerializeToString(setting);
    }

    public async Task<string> GetIndexInfoAsync(string indexFullName)
    {
        ArgumentNullException.ThrowIfNull(indexFullName);

        var response = await _openSearchClient.Indices.GetAsync(new GetIndexRequest(indexFullName));

        if (!response.IsValid)
        {
            if (response.OriginalException != null)
            {
                _logger.LogError(response.OriginalException, "There were issues retrieving index info from OpenSearch");
            }
            else
            {
                _logger.LogWarning("There were issues retrieving index info from OpenSearch");
            }

            return null;
        }

        var info = response.Indices[indexFullName];

        return SerializeToString(info);
    }

    public async Task<bool> DeleteAsync(IndexProfile index)
    {
        ArgumentNullException.ThrowIfNull(index);

        var context = new IndexRemoveContext(index.IndexFullName);

        await _indexEvents.InvokeAsync((handler, ctx) => handler.RemovingAsync(ctx), context, _logger);

        var response = await _openSearchClient.Indices.DeleteAsync(index.IndexFullName);

        if (!response.IsValid)
        {
            if (response.OriginalException != null)
            {
                _logger.LogError(response.OriginalException, "There were issues deleting an index in OpenSearch");
            }
            else if (response.ApiCall?.HttpStatusCode != 404)
            {
                _logger.LogWarning("There were issues deleting an index in OpenSearch");
            }

            return false;
        }

        await _indexEvents.InvokeAsync((handler, ctx) => handler.RemovedAsync(ctx), context, _logger);

        return response.Acknowledged;
    }

    public async Task<bool> RebuildAsync(IndexProfile index)
    {
        ArgumentNullException.ThrowIfNull(index);

        (var locker, var isLocked) = await _distributedLock.TryAcquireLockAsync(
            $"OpenSearchRebuild-{index.Id}",
            TimeSpan.FromSeconds(3),
            TimeSpan.FromMinutes(15));

        if (!isLocked)
        {
            _logger.LogWarning("Unable to acquire lock for rebuilding index {IndexName}. Another rebuild may be in progress.", index.Name);
            return false;
        }

        try
        {
            var context = new IndexRebuildContext(index);

            await _indexEvents.InvokeAsync((handler, ctx) => handler.RebuildingAsync(ctx), context, _logger);

            if (await ExistsAsync(index.IndexFullName))
            {
                var deleteResponse = await _openSearchClient.Indices.DeleteAsync(index.IndexFullName);

                if (!deleteResponse.IsValid)
                {
                    if (deleteResponse.OriginalException != null)
                    {
                        _logger.LogError(deleteResponse.OriginalException, "There were issues removing an index in OpenSearch");
                    }
                    else
                    {
                        _logger.LogWarning("There were issues removing an index in OpenSearch");
                    }
                }
            }

            var createIndexDescriptor = BuildCreateIndexDescriptor(index);
            var response = await _openSearchClient.Indices.CreateAsync(index.IndexFullName, createIndexDescriptor);

            if (!response.IsValid)
            {
                if (response.OriginalException != null)
                {
                    _logger.LogError(response.OriginalException, "There were issues creating an index in OpenSearch");
                }
                else
                {
                    _logger.LogWarning("There were issues creating an index in OpenSearch");
                }

                return false;
            }

            await _indexEvents.InvokeAsync((handler, ctx) => handler.RebuiltAsync(ctx), context, _logger);

            return response.Acknowledged;
        }
        finally
        {
            await locker.DisposeAsync();
        }
    }

    public async Task<bool> ExistsAsync(string indexFullName)
    {
        ArgumentException.ThrowIfNullOrEmpty(indexFullName);

        var response = await _openSearchClient.Indices.ExistsAsync(indexFullName);

        if (!response.IsValid && response.ApiCall?.HttpStatusCode != 404)
        {
            if (response.OriginalException != null)
            {
                _logger.LogError(response.OriginalException, "There were issues checking if an index in OpenSearch exists");
            }
            else
            {
                _logger.LogWarning("There were issues checking if an index in OpenSearch exists");
            }
        }

        return response.Exists;
    }

    public async Task<OpenSearchResult> SearchAsync(OpenSearchSearchContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        var result = new OpenSearchResult()
        {
            TopDocs = [],
        };

        if (await ExistsAsync(context.IndexProfile.IndexFullName))
        {
            var searchResponse = await _openSearchClient.SearchAsync<Dictionary<string, object>>(context.SearchRequest);

            if (!searchResponse.IsValid)
            {
                if (searchResponse.OriginalException != null)
                {
                    _logger.LogError(searchResponse.OriginalException, "There were issues executing a search in OpenSearch");
                }
                else
                {
                    _logger.LogWarning("There were issues executing a search in OpenSearch");
                }
            }
            else
            {
                ProcessSuccessfulSearchResponse(context.IndexProfile, result, searchResponse);
            }

            _timestamps[context.IndexProfile.IndexFullName] = _clock.UtcNow;
        }

        return result;
    }

    /// <summary>
    /// Returns results from a search made using the fluent OpenSearch client.
    /// </summary>
    public async Task SearchAsync(IndexProfile index, Func<OpenSearchClient, Task> openSearchClient)
    {
        ArgumentNullException.ThrowIfNull(index);
        ArgumentNullException.ThrowIfNull(openSearchClient);

        if (await ExistsAsync(index.IndexFullName))
        {
            await openSearchClient(_openSearchClient);

            _timestamps[index.IndexFullName] = _clock.UtcNow;
        }
    }

    internal async Task<OpenSearchResult> SearchAsync(IndexProfile indexProfile, string query)
    {
        ArgumentNullException.ThrowIfNull(indexProfile);
        ArgumentException.ThrowIfNullOrEmpty(query);

        var result = new OpenSearchResult()
        {
            TopDocs = [],
        };

        if (await ExistsAsync(indexProfile.IndexFullName))
        {
            var lowLevelResponse = await _openSearchClient.LowLevel.SearchAsync<StringResponse>(
                indexProfile.IndexFullName,
                PostData.String(query));

            if (!lowLevelResponse.Success)
            {
                if (lowLevelResponse.OriginalException != null)
                {
                    _logger.LogError(lowLevelResponse.OriginalException, "There were issues executing a raw search in OpenSearch");
                }
                else
                {
                    _logger.LogWarning("There were issues executing a raw search in OpenSearch");
                }
            }
            else if (!string.IsNullOrEmpty(lowLevelResponse.Body))
            {
                try
                {
                    var jsonRoot = JsonNode.Parse(lowLevelResponse.Body) as JsonObject;
                    var hitsObj = jsonRoot?["hits"] as JsonObject;
                    var hitsArray = hitsObj?["hits"]?.AsArray();

                    if (hitsArray != null)
                    {
                        result.Count = hitsArray.Count;
                        result.TotalCount = hitsObj?["total"]?["value"]?.GetValue<long>() ?? 0;

                        var metadata = indexProfile.As<OpenSearchIndexMetadata>();

                        foreach (var hitNode in hitsArray)
                        {
                            var hitObj = hitNode as JsonObject;
                            var source = hitObj?["_source"] as JsonObject;
                            var id = hitObj?["_id"]?.GetValue<string>();
                            var score = hitObj?["_score"]?.GetValue<double?>();

                            JsonObject document;

                            if (source != null)
                            {
                                document = source;
                            }
                            else
                            {
                                document = new JsonObject
                                {
                                    { metadata.IndexMappings?.KeyFieldName ?? ContentIndexingConstants.ContentItemIdKey, id },
                                };
                            }

                            result.TopDocs.Add(new OpenSearchRecord(document)
                            {
                                Score = score,
                            });
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error parsing raw OpenSearch search response");
                }
            }

            _timestamps[indexProfile.IndexFullName] = _clock.UtcNow;
        }

        return result;
    }

    private Func<CreateIndexDescriptor, ICreateIndexRequest> BuildCreateIndexDescriptor(IndexProfile indexProfile)
    {
        var metadata = indexProfile.As<OpenSearchIndexMetadata>();
        var analyzerName = metadata.GetAnalyzerName();

        return descriptor =>
        {
            descriptor.Settings(s =>
            {
                s.Analysis(a =>
                {
                    if (_openSearchOptions.Analyzers != null && _openSearchOptions.Analyzers.TryGetValue(analyzerName, out var analyzerProperties))
                    {
                        var analyzer = GetAnalyzer(analyzerProperties);
                        if (analyzer != null)
                        {
                            a.Analyzers(az => az.UserDefined(analyzerName, analyzer));
                        }
                    }

                    if (_openSearchOptions.TokenFilters != null && _openSearchOptions.TokenFilters.Count > 0)
                    {
                        a.TokenFilters(tf =>
                        {
                            foreach (var filter in _openSearchOptions.TokenFilters)
                            {
                                if (!filter.Value.TryGetPropertyValue("type", out var typeObject) ||
                                    !_tokenFilterGetter.TryGetValue(typeObject.ToString(), out var tokenFilterType))
                                {
                                    continue;
                                }

                                var filterProps = filter.Value.ToJsonString();
                                var tokenFilter = System.Text.Json.JsonSerializer.Deserialize(filterProps, tokenFilterType) as ITokenFilter;

                                if (tokenFilter != null)
                                {
                                    tf.UserDefined(filter.Key, tokenFilter);
                                }
                            }

                            return tf;
                        });
                    }

                    return a;
                });

                return s;
            });

            if (metadata.IndexMappings?.Mapping != null)
            {
                var typeMapping = metadata.IndexMappings.Mapping;

                // Store last task ID in meta.
                typeMapping.Meta ??= new FluentDictionary<string, object>();
                typeMapping.Meta[OpenSearchConstants.LastTaskIdMetadataKey] = 0;

                descriptor.Map(m =>
                {
                    var iMapping = (ITypeMapping)m;
                    iMapping.Properties = typeMapping.Properties;
                    iMapping.DynamicTemplates = typeMapping.DynamicTemplates;
                    iMapping.Meta = typeMapping.Meta;
                    iMapping.SourceField = new SourceField { Enabled = metadata.StoreSourceData };
                    return m;
                });
            }

            return descriptor;
        };
    }

    private static IAnalyzer GetAnalyzer(System.Text.Json.Nodes.JsonObject analyzerProperties)
    {
        IAnalyzer analyzer = null;

        if (analyzerProperties.TryGetPropertyValue("type", out var typeObject)
            && _analyzerGetter.TryGetValue(typeObject.ToString(), out var analyzerType))
        {
            var json = analyzerProperties.ToJsonString();
            analyzer = System.Text.Json.JsonSerializer.Deserialize(json, analyzerType) as IAnalyzer;
        }

        if (analyzer == null && _analyzerGetter.TryGetValue(OpenSearchConstants.DefaultAnalyzer, out var defaultType))
        {
            var json = analyzerProperties.ToJsonString();
            analyzer = System.Text.Json.JsonSerializer.Deserialize(json, defaultType) as IAnalyzer;
        }

        return analyzer;
    }

    private static void ProcessSuccessfulSearchResponse(
        IndexProfile indexProfile,
        OpenSearchResult result,
        ISearchResponse<Dictionary<string, object>> searchResponse)
    {
        result.Count = searchResponse.Hits.Count;
        result.TotalCount = searchResponse.HitsMetadata?.Total?.Value ?? 0;

        var metadata = indexProfile.As<OpenSearchIndexMetadata>();

        foreach (var hit in searchResponse.Hits)
        {
            JsonObject jsonObject;

            if (hit.Source != null)
            {
                var json = System.Text.Json.JsonSerializer.Serialize(hit.Source);
                jsonObject = JsonNode.Parse(json) as JsonObject ?? new JsonObject();
            }
            else
            {
                jsonObject = new JsonObject
                {
                    { metadata.IndexMappings?.KeyFieldName ?? ContentIndexingConstants.ContentItemIdKey, hit.Id },
                };
            }

            IReadOnlyDictionary<string, IReadOnlyCollection<string>> highlights = null;

            if (hit.Highlight != null && hit.Highlight.Count > 0)
            {
                highlights = hit.Highlight.ToDictionary(
                    k => k.Key,
                    v => (IReadOnlyCollection<string>)v.Value.ToList());
            }

            result.TopDocs.Add(new OpenSearchRecord(jsonObject)
            {
                Score = hit.Score,
                Highlights = highlights,
            });
        }
    }

    private string SerializeToString(object value)
    {
        using var stream = new MemoryStream();
        _openSearchClient.RequestResponseSerializer.Serialize(value, stream);
        stream.Position = 0;
        return Encoding.UTF8.GetString(stream.ToArray());
    }
}
