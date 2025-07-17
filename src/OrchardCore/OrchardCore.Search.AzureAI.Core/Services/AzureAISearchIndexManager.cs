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

public sealed class AzureAISearchIndexManager : IIndexManager
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

    public async Task<bool> RebuildAsync(IndexProfile indexProfile)
    {
        try
        {
            var context = new IndexRebuildContext(indexProfile);

            await _indexEvents.InvokeAsync((handler, ctx) => handler.RebuildingAsync(ctx), context, _logger);

            var client = _clientFactory.CreateSearchIndexClient();

            if (await ExistsAsync(indexProfile.IndexFullName))
            {
                await client.DeleteIndexAsync(indexProfile.IndexFullName);
            }

            var searchIndex = GetSearchIndex(indexProfile);

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

    private static SearchIndex GetSearchIndex(IndexProfile indexProfile)
    {
        var searchFields = new List<SearchField>();

        var suggesterFieldNames = new List<string>();
        var metadata = indexProfile.As<AzureAISearchIndexMetadata>();

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

            var field = GetSearchFieldTemplate(indexMap, metadata.AnalyzerName);

            if (field is null)
            {
                continue;
            }

            searchFields.Add(field);
        }

        var searchIndex = new SearchIndex(indexProfile.IndexFullName)
        {
            Fields = searchFields,
        };

        if (suggesterFieldNames.Count > 0)
        {
            searchIndex.Suggesters.Add(new SearchSuggester("sg", suggesterFieldNames));
        }

        return searchIndex;
    }

    private static SearchFieldTemplate GetSearchFieldTemplate(AzureAISearchIndexMap indexMap, string analyzerName)
    {
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
                var subFieldTemplate = GetSearchFieldTemplate(subField, analyzerName);

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
            _ => throw new ArgumentOutOfRangeException(nameof(type), $"The type '{type}' is not support by Azure AI Search")
        };
}
