using Azure;
using Azure.Search.Documents.Indexes.Models;
using Microsoft.Extensions.Logging;
using OrchardCore.AzureAI.Models;
using OrchardCore.Entities;
using OrchardCore.Indexing;
using OrchardCore.Indexing.Models;
using OrchardCore.Locking.Distributed;
using OrchardCore.Modules;
using static OrchardCore.Indexing.DocumentIndex;

namespace OrchardCore.AzureAI.Services;

public sealed class AzureAISearchIndexManager : IIndexManager
{
    public const string FullTextKey = "Content__ContentItem__FullText";
    public const string DisplayTextAnalyzedKey = "Content__ContentItem__DisplayText__Analyzed";

    private readonly AzureAIClientFactory _clientFactory;
    private readonly ILogger _logger;
    private readonly IEnumerable<IIndexEvents> _indexEvents;
    private readonly IDistributedLock _distributedLock;

    public AzureAISearchIndexManager(
        AzureAIClientFactory clientFactory,
        ILogger<AzureAISearchIndexManager> logger,
        IEnumerable<IIndexEvents> indexEvents,
        IDistributedLock distributedLock)
    {
        _clientFactory = clientFactory;
        _distributedLock = distributedLock;
        _logger = logger;
        _indexEvents = indexEvents;
    }

    public async Task<bool> CreateAsync(IndexProfile indexProfile)
    {
        if (await ExistsAsync(indexProfile.IndexFullName))
        {
            return false;
        }

        try
        {
            var context = new IndexCreateContext(indexProfile);

            await _indexEvents.InvokeAsync((handler, ctx) => handler.CreatingAsync(ctx), context, _logger);

            var searchIndex = GetSearchIndex(indexProfile);

            var client = _clientFactory.CreateSearchIndexClient();

            await client.CreateIndexAsync(searchIndex);

            await _indexEvents.InvokeAsync((handler, ctx) => handler.CreatedAsync(ctx), context, _logger);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unable to create index in Azure AI Search. Message: {Message}", ex.Message);
        }

        return false;
    }

    public async Task<bool> ExistsAsync(string indexFullName)
        => await GetAsync(indexFullName) != null;

    public async Task<SearchIndex> GetAsync(string indexFullName)
    {
        try
        {
            var client = _clientFactory.CreateSearchIndexClient();

            var response = await client.GetIndexAsync(indexFullName);

            return response.Value;
        }
        catch (RequestFailedException e)
        {
            if (e.Status != 404)
            {
                _logger.LogError(e, "Unable to get index from Azure AI Search.");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unable to get index from Azure AI Search.");
        }

        return null;
    }

    public async Task<bool> DeleteAsync(IndexProfile indexProfile)
    {
        ArgumentNullException.ThrowIfNull(indexProfile);

        if (!await ExistsAsync(indexProfile.IndexFullName))
        {
            return false;
        }

        try
        {
            var context = new IndexRemoveContext(indexProfile.IndexFullName);

            await _indexEvents.InvokeAsync((handler, ctx) => handler.RemovingAsync(ctx), context, _logger);

            var client = _clientFactory.CreateSearchIndexClient();

            await client.DeleteIndexAsync(context.IndexFullName, default(MatchConditions));

            await _indexEvents.InvokeAsync((handler, ctx) => handler.RemovedAsync(ctx), context, _logger);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unable to delete index from Azure AI Search.");
        }

        return false;
    }

    public async Task<bool> RebuildAsync(IndexProfile indexProfile)
    {
        ArgumentNullException.ThrowIfNull(indexProfile);

        // Acquire a distributed lock to prevent concurrent rebuild operations that could cause
        // the index to be temporarily unavailable during the delete and create window.
        (var locker, var isLocked) = await _distributedLock.TryAcquireLockAsync(
            $"AzureAIRebuild-{indexProfile.Id}",
            TimeSpan.FromSeconds(3),
            TimeSpan.FromMinutes(15));

        if (!isLocked)
        {
            _logger.LogWarning("Unable to acquire lock for rebuilding index {IndexName}. Another rebuild may be in progress.", indexProfile.Name);
            return false;
        }

        try
        {
            var context = new IndexRebuildContext(indexProfile);

            await _indexEvents.InvokeAsync((handler, ctx) => handler.RebuildingAsync(ctx), context, _logger);

            var client = _clientFactory.CreateSearchIndexClient();

            if (await ExistsAsync(indexProfile.IndexFullName))
            {
                await client.DeleteIndexAsync(indexProfile.IndexFullName, default(MatchConditions));
            }

            var searchIndex = GetSearchIndex(indexProfile);

            await client.CreateIndexAsync(searchIndex);

            await _indexEvents.InvokeAsync((handler, ctx) => handler.RebuiltAsync(ctx), context, _logger);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unable to update Azure AI Search index.");
            return false;
        }
        finally
        {
            await locker.DisposeAsync();
        }
    }

    private static SearchIndex GetSearchIndex(IndexProfile indexProfile)
    {
        var searchFields = new List<SearchField>();
        List<AzureAISearchIndexMap> vectorFields = null;
        var suggesterFieldNames = new List<string>();
        var metadata = indexProfile.GetOrCreate<AzureAISearchIndexMetadata>();

        foreach (var indexMap in metadata.IndexMappings)
        {
            if (searchFields.Exists(x => x.Name.EqualsOrdinalIgnoreCase(indexMap.AzureFieldKey)))
            {
                continue;
            }

            if (indexMap.IsSuggester && !suggesterFieldNames.Contains(indexMap.AzureFieldKey))
            {
                suggesterFieldNames.Add(indexMap.AzureFieldKey);
            }

            var field = GetSearchFieldTemplate(indexMap, metadata, metadata.AnalyzerName);

            if (field is null)
            {
                continue;
            }

            searchFields.Add(field);

            if (indexMap.Type == Types.Vector)
            {
                vectorFields ??= [];
                vectorFields.Add(indexMap);
            }
        }

        var searchIndex = new SearchIndex(indexProfile.IndexFullName)
        {
            Fields = searchFields,
        };

        if (suggesterFieldNames.Count > 0)
        {
            searchIndex.Suggesters.Add(new SearchSuggester("sg", suggesterFieldNames));
        }

        if (BuildVectorSearch(metadata, vectorFields) is { } vectorSearch)
        {
            searchIndex.VectorSearch = vectorSearch;
        }

        return searchIndex;
    }

    private static VectorSearch BuildVectorSearch(AzureAISearchIndexMetadata metadata, List<AzureAISearchIndexMap> vectorFields)
    {
        if (vectorFields is null || vectorFields.Count == 0)
        {
            return null;
        }

        var vectorSearch = new VectorSearch();
        var configuredVectorSearch = metadata.VectorSearch;
        var configuredProfiles = GetConfiguredProfiles(configuredVectorSearch);
        var configuredAlgorithms = GetConfiguredAlgorithms(configuredVectorSearch);
        var addedProfiles = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var addedAlgorithms = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var profile in configuredProfiles)
        {
            var algorithmName = string.IsNullOrWhiteSpace(profile.AlgorithmConfigurationName)
                ? CreateDefaultVectorSearchAlgorithmName(profile.Name)
                : profile.AlgorithmConfigurationName;

            EnsureAlgorithm(vectorSearch, configuredAlgorithms, addedAlgorithms, algorithmName);
            vectorSearch.Profiles.Add(CreateVectorSearchProfile(profile, algorithmName));
            addedProfiles.Add(profile.Name);
        }

        foreach (var vectorField in vectorFields)
        {
            var profileName = GetVectorSearchProfileName(metadata, vectorField.VectorInfo);

            if (string.IsNullOrEmpty(profileName) || !addedProfiles.Add(profileName))
            {
                continue;
            }

            var algorithmName = CreateDefaultVectorSearchAlgorithmName(profileName);

            EnsureAlgorithm(vectorSearch, configuredAlgorithms, addedAlgorithms, algorithmName);
            vectorSearch.Profiles.Add(new VectorSearchProfile(profileName, algorithmName));
        }

        return vectorSearch.Profiles.Count > 0 ? vectorSearch : null;
    }

    private static void EnsureAlgorithm(
        VectorSearch vectorSearch,
        Dictionary<string, AzureAISearchVectorSearchAlgorithm> configuredAlgorithms,
        HashSet<string> addedAlgorithms,
        string algorithmName)
    {
        if (string.IsNullOrWhiteSpace(algorithmName) || !addedAlgorithms.Add(algorithmName))
        {
            return;
        }

        if (configuredAlgorithms.TryGetValue(algorithmName, out var configuredAlgorithm))
        {
            vectorSearch.Algorithms.Add(CreateVectorSearchAlgorithm(configuredAlgorithm));
            return;
        }

        vectorSearch.Algorithms.Add(new HnswAlgorithmConfiguration(algorithmName));
    }

    private static VectorSearchProfile CreateVectorSearchProfile(AzureAISearchVectorSearchProfile profile, string algorithmName)
    {
        var vectorSearchProfile = new VectorSearchProfile(profile.Name, algorithmName);

        if (!string.IsNullOrWhiteSpace(profile.VectorizerName))
        {
            vectorSearchProfile.VectorizerName = profile.VectorizerName;
        }

        if (!string.IsNullOrWhiteSpace(profile.CompressionName))
        {
            vectorSearchProfile.CompressionName = profile.CompressionName;
        }

        return vectorSearchProfile;
    }

    private static VectorSearchAlgorithmConfiguration CreateVectorSearchAlgorithm(AzureAISearchVectorSearchAlgorithm algorithm)
    {
        if (string.IsNullOrWhiteSpace(algorithm.Kind) || algorithm.Kind.EqualsOrdinalIgnoreCase(AzureAISearchVectorSearchAlgorithm.HnswKind))
        {
            var hnswConfiguration = new HnswAlgorithmConfiguration(algorithm.Name);

            if (algorithm.HnswParameters is not null)
            {
                hnswConfiguration.Parameters = new HnswParameters
                {
                    M = algorithm.HnswParameters.M,
                    EfConstruction = algorithm.HnswParameters.EfConstruction,
                    EfSearch = algorithm.HnswParameters.EfSearch,
                    Metric = ToVectorSearchAlgorithmMetric(algorithm.HnswParameters.Metric),
                };
            }

            return hnswConfiguration;
        }

        if (algorithm.Kind.EqualsOrdinalIgnoreCase(AzureAISearchVectorSearchAlgorithm.ExhaustiveKnnKind))
        {
            var exhaustiveKnnConfiguration = new ExhaustiveKnnAlgorithmConfiguration(algorithm.Name);

            if (algorithm.ExhaustiveKnnParameters is not null)
            {
                exhaustiveKnnConfiguration.Parameters = new ExhaustiveKnnParameters
                {
                    Metric = ToVectorSearchAlgorithmMetric(algorithm.ExhaustiveKnnParameters.Metric),
                };
            }

            return exhaustiveKnnConfiguration;
        }

        throw new ArgumentOutOfRangeException(nameof(algorithm), algorithm.Kind, "Unsupported Azure AI Search vector algorithm kind.");
    }

    private static VectorSearchAlgorithmMetric? ToVectorSearchAlgorithmMetric(string metric)
        => string.IsNullOrWhiteSpace(metric) ? null : new VectorSearchAlgorithmMetric(metric);

    private static string GetVectorSearchProfileName(AzureAISearchIndexMetadata metadata, AzureAISearchIndexMapVectorInfo vectorInfo)
    {
        if (!string.IsNullOrWhiteSpace(vectorInfo?.VectorSearchConfiguration))
        {
            return vectorInfo.VectorSearchConfiguration;
        }

        var configuredProfileName = GetConfiguredProfiles(metadata.VectorSearch).FirstOrDefault()?.Name;

        return string.IsNullOrWhiteSpace(configuredProfileName)
            ? AzureAISearchVectorSearchOptions.DefaultProfileName
            : configuredProfileName;
    }

    private static AzureAISearchVectorSearchProfile[] GetConfiguredProfiles(AzureAISearchVectorSearchOptions vectorSearch)
        => vectorSearch?.Profiles?
            .Where(x => x is not null && !string.IsNullOrWhiteSpace(x.Name))
            .ToArray()
            ?? [];

    private static Dictionary<string, AzureAISearchVectorSearchAlgorithm> GetConfiguredAlgorithms(AzureAISearchVectorSearchOptions vectorSearch)
        => vectorSearch?.Algorithms?
            .Where(x => x is not null && !string.IsNullOrWhiteSpace(x.Name))
            .ToDictionary(x => x.Name, StringComparer.OrdinalIgnoreCase)
            ?? [];

    private static string CreateDefaultVectorSearchAlgorithmName(string profileName)
        => profileName.EqualsOrdinalIgnoreCase(AzureAISearchVectorSearchOptions.DefaultProfileName)
            ? AzureAISearchVectorSearchOptions.DefaultAlgorithmName
            : $"{profileName}-algorithm";

    private static SearchFieldTemplate GetSearchFieldTemplate(AzureAISearchIndexMap indexMap, AzureAISearchIndexMetadata metadata, string analyzerName)
    {
        if (indexMap.Type == Types.Vector)
        {
            if (indexMap.VectorInfo is null || indexMap.VectorInfo.Dimensions <= 0)
            {
                return null;
            }

            return new VectorSearchField(indexMap.AzureFieldKey, indexMap.VectorInfo.Dimensions, GetVectorSearchProfileName(metadata, indexMap.VectorInfo));
        }

        if (indexMap.IsSearchable)
        {
            return new SearchableField(indexMap.AzureFieldKey, collection: indexMap.IsCollection)
            {
                AnalyzerName = !string.IsNullOrEmpty(analyzerName)
                ? analyzerName
                : AzureAISearchDefaultOptions.DefaultAnalyzer,
                IsKey = indexMap.IsKey,
                IsFilterable = indexMap.IsFilterable,
                IsSortable = indexMap.IsSortable,
                IsHidden = indexMap.IsHidden,
                IsFacetable = indexMap.IsFacetable,
            };
        }

        var fieldType = GetFieldType(indexMap.Type);

        if (fieldType == SearchFieldDataType.Complex)
        {
            if (indexMap.SubFields is null || indexMap.SubFields.Count == 0)
            {
                return null;
            }

            var field = new ComplexField(indexMap.AzureFieldKey, collection: indexMap.IsCollection);

            foreach (var subField in indexMap.SubFields)
            {
                var subFieldTemplate = GetSearchFieldTemplate(subField, metadata, analyzerName);

                if (subFieldTemplate is null)
                {
                    continue;
                }

                field.Fields.Add(subFieldTemplate);
            }

            if (field.Fields.Count == 0)
            {
                // Complex fields must have at least one sub-field defined.
                return null;
            }

            return field;
        }

        return new SimpleField(indexMap.AzureFieldKey, fieldType)
        {
            IsKey = indexMap.IsKey,
            IsFilterable = indexMap.IsFilterable,
            IsSortable = indexMap.IsSortable,
            IsHidden = indexMap.IsHidden,
            IsFacetable = indexMap.IsFacetable,
        };
    }

    private static SearchFieldDataType GetFieldType(Types type)
        => type switch
        {
            Types.Boolean => SearchFieldDataType.Boolean,
            Types.DateTime => SearchFieldDataType.DateTimeOffset,
            Types.Number => SearchFieldDataType.Double,
            Types.Integer => SearchFieldDataType.Int64,
            Types.GeoPoint => SearchFieldDataType.GeographyPoint,
            Types.Text => SearchFieldDataType.String,
            Types.Complex => SearchFieldDataType.Complex,
            Types.Vector => SearchFieldDataType.Single,
            _ => throw new ArgumentOutOfRangeException(nameof(type), $"The type '{type}' is not support by Azure AI Search")
        };
}
