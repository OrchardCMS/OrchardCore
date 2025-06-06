using Azure;
using Azure.Search.Documents.Indexes.Models;
using Microsoft.Extensions.Logging;
using OrchardCore.Entities;
using OrchardCore.Indexing;
using OrchardCore.Indexing.Models;
using OrchardCore.Modules;
using OrchardCore.Search.AzureAI.Models;
using static OrchardCore.Indexing.DocumentIndex;

namespace OrchardCore.Search.AzureAI.Services;

public class AzureAISearchIndexManager : IIndexManager
{
    public const string OwnerKey = "Content__ContentItem__Owner";
    public const string AuthorKey = "Content__ContentItem__Author";
    public const string FullTextKey = "Content__ContentItem__FullText";
    public const string DisplayTextAnalyzedKey = "Content__ContentItem__DisplayText__Analyzed";

    private readonly AzureAIClientFactory _clientFactory;
    private readonly ILogger _logger;
    private readonly IEnumerable<IIndexEvents> _indexEvents;

    public AzureAISearchIndexManager(
        AzureAIClientFactory clientFactory,
        ILogger<AzureAISearchIndexManager> logger,
        IEnumerable<IIndexEvents> indexEvents)
    {
        _clientFactory = clientFactory;
        _logger = logger;
        _indexEvents = indexEvents;
    }

    public async Task<bool> CreateAsync(IndexProfile index)
    {
        if (await ExistsAsync(index.IndexFullName))
        {
            return true;
        }

        try
        {
            var context = new IndexCreateContext(index);

            await _indexEvents.InvokeAsync((handler, ctx) => handler.CreatingAsync(ctx), context, _logger);

            var searchIndex = GetSearchIndex(index);

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

    public async Task<bool> DeleteAsync(IndexProfile index)
    {
        ArgumentNullException.ThrowIfNull(index);

        if (!await ExistsAsync(index.IndexFullName))
        {
            return false;
        }

        try
        {
            var context = new IndexRemoveContext(index.IndexFullName);

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

    public async Task<bool> RebuildAsync(IndexProfile index)
    {
        try
        {
            var context = new IndexRebuildContext(index);

            await _indexEvents.InvokeAsync((handler, ctx) => handler.RebuildingAsync(ctx), context, _logger);

            var client = _clientFactory.CreateSearchIndexClient();

            if (await ExistsAsync(index.IndexName))
            {
                await client.DeleteIndexAsync(index.IndexFullName);
            }

            var searchIndex = GetSearchIndex(index);

            await client.CreateIndexAsync(searchIndex);

            await _indexEvents.InvokeAsync((handler, ctx) => handler.RebuiltAsync(ctx), context, _logger);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unable to update Azure AI Search index.");
        }

        return false;
    }

    private static SearchIndex GetSearchIndex(IndexProfile index)
    {
        var searchFields = new List<SearchField>();

        var suggesterFieldNames = new List<string>();
        var metadata = index.As<AzureAISearchIndexMetadata>();

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

            var fieldType = GetFieldType(indexMap.Type);

            if (indexMap.IsSearchable)
            {
                searchFields.Add(new SearchableField(indexMap.AzureFieldKey, collection: indexMap.IsCollection)
                {
                    AnalyzerName = metadata.AnalyzerName,
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

        var searchIndex = new SearchIndex(index.IndexFullName)
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
