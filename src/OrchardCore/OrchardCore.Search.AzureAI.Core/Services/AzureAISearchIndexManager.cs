using Azure;
using Azure.Search.Documents.Indexes.Models;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OrchardCore.Environment.Shell;
using OrchardCore.Modules;
using OrchardCore.Search.AzureAI.Models;
using static OrchardCore.Indexing.DocumentIndexBase;

namespace OrchardCore.Search.AzureAI.Services;

public class AzureAISearchIndexManager
{
    public const string OwnerKey = "Content__ContentItem__Owner";
    public const string AuthorKey = "Content__ContentItem__Author";
    public const string FullTextKey = "Content__ContentItem__FullText";
    public const string DisplayTextAnalyzedKey = "Content__ContentItem__DisplayText__Analyzed";

    private readonly AzureAIClientFactory _clientFactory;
    private readonly ILogger _logger;
    private readonly IEnumerable<IAzureAISearchIndexEvents> _indexEvents;
    private readonly IMemoryCache _memoryCache;
    private readonly ShellSettings _shellSettings;
    private readonly AzureAISearchDefaultOptions _azureAIOptions;
    private readonly string _prefixCacheKey;

    public AzureAISearchIndexManager(
        AzureAIClientFactory clientFactory,
        ILogger<AzureAISearchIndexManager> logger,
        IEnumerable<IAzureAISearchIndexEvents> indexEvents,
        IMemoryCache memoryCache,
        ShellSettings shellSettings,
        IOptions<AzureAISearchDefaultOptions> azureAIOptions)
    {
        _clientFactory = clientFactory;
        _logger = logger;
        _indexEvents = indexEvents;
        _memoryCache = memoryCache;
        _shellSettings = shellSettings;
        _azureAIOptions = azureAIOptions.Value;
        _prefixCacheKey = $"AzureAISearchIndexesPrefix_{shellSettings.Name}";
    }

    public async Task<bool> CreateAsync(AzureAISearchIndexSettings settings)
    {
        if (await ExistsAsync(settings.IndexName))
        {
            return true;
        }

        try
        {
            var context = new AzureAISearchIndexCreateContext(settings, GetFullIndexName(settings.IndexName));

            await _indexEvents.InvokeAsync((handler, ctx) => handler.CreatingAsync(ctx), context, _logger);

            var searchIndex = GetSearchIndex(context.IndexFullName, settings);

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

    public async Task<bool> ExistsAsync(string indexName)
        => await GetAsync(indexName) != null;

    public async Task<SearchIndex> GetAsync(string indexName)
    {
        try
        {
            var indexFullName = GetFullIndexName(indexName);

            var client = _clientFactory.CreateSearchIndexClient();

            var response = await client.GetIndexAsync(indexFullName);

            return response?.Value;
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

    public async Task<bool> DeleteAsync(string indexName)
    {
        if (!await ExistsAsync(indexName))
        {
            return false;
        }

        try
        {
            var context = new AzureAISearchIndexRemoveContext(indexName, GetFullIndexName(indexName));

            await _indexEvents.InvokeAsync((handler, ctx) => handler.RemovingAsync(ctx), context, _logger);

            var client = _clientFactory.CreateSearchIndexClient();

            await client.DeleteIndexAsync(context.IndexFullName);

            await _indexEvents.InvokeAsync((handler, ctx) => handler.RemovedAsync(ctx), context, _logger);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unable to delete index from Azure AI Search.");
        }

        return false;
    }

    public async Task RebuildAsync(AzureAISearchIndexSettings settings)
    {
        try
        {
            var context = new AzureAISearchIndexRebuildContext(settings, GetFullIndexName(settings.IndexName));

            await _indexEvents.InvokeAsync((handler, ctx) => handler.RebuildingAsync(ctx), context, _logger);

            var client = _clientFactory.CreateSearchIndexClient();

            if (await ExistsAsync(settings.IndexName))
            {
                await client.DeleteIndexAsync(context.IndexFullName);
            }

            var searchIndex = GetSearchIndex(context.IndexFullName, settings);

            await client.CreateIndexAsync(searchIndex);

            await _indexEvents.InvokeAsync((handler, ctx) => handler.RebuiltAsync(ctx), context, _logger);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unable to update Azure AI Search index.");
        }
    }

    public string GetFullIndexName(string indexName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(indexName);

        return GetIndexPrefix() + '-' + indexName;
    }

    private string GetIndexPrefix()
    {
        if (!_memoryCache.TryGetValue<string>(_prefixCacheKey, out var value))
        {
            var prefix = _shellSettings.Name.ToLowerInvariant();

            if (!string.IsNullOrWhiteSpace(_azureAIOptions.IndexesPrefix))
            {
                prefix = $"{_azureAIOptions.IndexesPrefix.ToLowerInvariant()}-{prefix}";
            }

            if (AzureAISearchIndexNamingHelper.TryGetSafePrefix(prefix, out var safePrefix))
            {
                value = safePrefix;
                _memoryCache.Set(_prefixCacheKey, safePrefix);
            }
            else
            {
                throw new InvalidOperationException($"Unable to create a safe index prefix for AI Search. Attempted to created a safe name using '{safePrefix}'.");
            }
        }

        return value ?? string.Empty;
    }

    private static SearchIndex GetSearchIndex(string fullIndexName, AzureAISearchIndexSettings settings)
    {
        var searchFields = new List<SearchField>();

        var suggesterFieldNames = new List<string>();

        foreach (var indexMap in settings.IndexMappings)
        {
            if (searchFields.Exists(x => x.Name.EqualsOrdinalIgnoreCase(indexMap.AzureFieldKey)))
            {
                continue;
            }

            if (indexMap.IsSuggester && !suggesterFieldNames.Contains(indexMap.AzureFieldKey))
            {
                suggesterFieldNames.Add(indexMap.AzureFieldKey);
            }

            var fieldType = GetFieldType(indexMap.Type);

            if (indexMap.IsSearchable)
            {
                searchFields.Add(new SearchableField(indexMap.AzureFieldKey, collection: indexMap.IsCollection)
                {
                    AnalyzerName = settings.AnalyzerName,
                    IsKey = indexMap.IsKey,
                    IsFilterable = indexMap.IsFilterable,
                    IsSortable = indexMap.IsSortable,
                    IsHidden = indexMap.IsHidden,
                    IsFacetable = indexMap.IsFacetable,
                });
            }
            else
            {
                searchFields.Add(new SimpleField(indexMap.AzureFieldKey, fieldType)
                {
                    IsKey = indexMap.IsKey,
                    IsFilterable = indexMap.IsFilterable,
                    IsSortable = indexMap.IsSortable,
                    IsHidden = indexMap.IsHidden,
                    IsFacetable = indexMap.IsFacetable,
                });
            }
        }

        var searchIndex = new SearchIndex(fullIndexName)
        {
            Fields = searchFields,
            Suggesters =
            {
                new SearchSuggester("sg", suggesterFieldNames),
            },
        };

        return searchIndex;
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
            _ => throw new ArgumentOutOfRangeException(nameof(type), $"The type '{type}' is not support by Azure AI Search")
        };
}
