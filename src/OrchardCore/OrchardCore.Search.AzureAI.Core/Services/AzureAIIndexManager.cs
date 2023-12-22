using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Azure;
using Azure.Search.Documents.Indexes;
using Azure.Search.Documents.Indexes.Models;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OrchardCore.Contents.Indexing;
using OrchardCore.Environment.Shell;
using OrchardCore.Modules;
using OrchardCore.Search.AzureAI.Models;
using static OrchardCore.Indexing.DocumentIndex;

namespace OrchardCore.Search.AzureAI.Services;

public class AzureAIIndexManager(
    SearchIndexClient client,
    ILogger<AzureAIIndexManager> logger,
    IOptions<AzureAIOptions> azureAIOptions,
    IMemoryCache memoryCache,
    ShellSettings shellSettings)
{
    public const string OwnerKey = "Content__ContentItem__Owner";
    public const string AuthorKey = "Content__ContentItem__Author";
    public const string FullTextKey = "Content__ContentItem__FullText";

    private readonly SearchIndexClient _client = client;
    private readonly ILogger _logger = logger;
    private readonly IMemoryCache _memoryCache = memoryCache;
    private readonly ShellSettings _shellSettings = shellSettings;
    private readonly AzureAIOptions _azureAIOptions = azureAIOptions.Value;

    public async Task<bool> CreateAsync(AzureAIIndexSettings settings)
    {
        if (await ExistsAsync(settings.IndexName))
        {
            return true;
        }

        try
        {
            var fullIndexName = GetFullIndexName(settings.IndexName);

            var searchIndex = GetSearchIndex(fullIndexName, settings);

            var response = await _client.CreateIndexAsync(searchIndex);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unable to create index in Azure AI Search.");
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
            var response = await _client.GetIndexAsync(indexFullName);

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

    /// <summary>
    /// Makes sure that the index names are compliant with Azure AI Search specifications.
    /// <see href="https://learn.microsoft.com/en-us/rest/api/searchservice/naming-rules"/>.
    /// </summary>
    public bool TryGetSafeName(string indexName, out string safeName)
    {
        if (!TryGetSafePrefix(indexName, out var safePrefix) || safePrefix.Length < 2)
        {
            safeName = null;

            return false;
        }

        safeName = safePrefix;

        return true;
    }

    public string GetFullIndexName(string indexName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(indexName);

        return GetIndexPrefix() + '-' + indexName;
    }

    public bool TryGetSafeFieldName(string fieldName, out string safeName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(fieldName, nameof(fieldName));

        while (!char.IsLetter(fieldName[0]))
        {
            fieldName = fieldName.Remove(0, 1);
        }

        if (fieldName.StartsWith("azureSearch"))
        {
            fieldName = fieldName[11..];
        }

        fieldName = fieldName.Replace(".", "__");

        var validChars = Array.FindAll(fieldName.ToCharArray(), c => char.IsLetterOrDigit(c) || c == '_');

        if (validChars.Length > 0)
        {
            safeName = new string(validChars);

            return true;
        }

        safeName = null;

        return false;
    }
    private const string _prefixCacheKey = "AzureAISearchIndexesPrefix";

    private string GetIndexPrefix()
    {
        if (!_memoryCache.TryGetValue<string>(_prefixCacheKey, out var value))
        {
            var builder = new StringBuilder();

            if (!string.IsNullOrWhiteSpace(_azureAIOptions.IndexesPrefix))
            {
                builder.Append(_azureAIOptions.IndexesPrefix.ToLowerInvariant());
                builder.Append('-');
            }

            builder.Append(_shellSettings.Name.ToLowerInvariant());

            if (TryGetSafePrefix(builder.ToString(), out var safePrefix))
            {
                value = safePrefix;
                _memoryCache.Set(_prefixCacheKey, safePrefix);
            }
        }

        return value ?? string.Empty;
    }

    public async Task<bool> DeleteAsync(string indexName)
    {
        if (!await ExistsAsync(indexName))
        {
            return false;
        }

        try
        {
            var response = await _client.DeleteIndexAsync(GetFullIndexName(indexName));

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unable to delete index from Azure AI Search.");
        }

        return false;
    }

    public async Task RebuildIndexAsync(AzureAIIndexSettings settings)
    {
        try
        {
            await DeleteAsync(settings.IndexName);
            await CreateAsync(settings);

            // CreateOrUpdateIndexAsync does not allow you to update existing index.
            // var fullIndexName = GetFullIndexName(settings.IndexName);
            // var indexOptions = GetSearchIndex(fullIndexName, settings);
            // await _client.CreateOrUpdateIndexAsync(indexOptions, allowIndexDowntime: true, onlyIfUnchanged: true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unable to update Azure AI Search index.");
        }
    }

    private SearchIndex GetSearchIndex(string fullIndexName, AzureAIIndexSettings settings)
    {
        var searchFields = new List<SearchField>()
        {
            new SimpleField(IndexingConstants.ContentItemIdKey, SearchFieldDataType.String)
            {
                IsKey = true,
                IsFilterable = true,
                IsSortable = true,
            },
            new SimpleField(IndexingConstants.ContentItemVersionIdKey, SearchFieldDataType.String)
            {
                IsFilterable = true,
                IsSortable = true,
            },
            new SimpleField(OwnerKey, SearchFieldDataType.String)
            {
                IsFilterable = true,
                IsSortable = true,
            },
            new SearchableField(FullTextKey)
            {
                AnalyzerName = settings.AnalyzerName,
            },
        };

        foreach (var indexMap in settings.IndexMappings)
        {
            if (!TryGetSafeFieldName(indexMap.Key, out var safeFieldName))
            {
                continue;
            }

            if (searchFields.Exists(x => x.Name.EqualsOrdinalIgnoreCase(safeFieldName)))
            {
                continue;
            }

            if (indexMap.Options.HasFlag(Indexing.DocumentIndexOptions.Keyword))
            {
                searchFields.Add(new SimpleField(safeFieldName, GetFieldType(indexMap.Type))
                {
                    IsFilterable = true,
                    IsSortable = true,
                });

                continue;
            }

            searchFields.Add(new SearchableField(safeFieldName, true)
            {
                AnalyzerName = settings.AnalyzerName,
            });
        }

        var searchIndex = new SearchIndex(fullIndexName)
        {
            Fields = searchFields,
            Suggesters =
            {
                new SearchSuggester("sg", FullTextKey),
            },
        };

        return searchIndex;
    }

    private static bool TryGetSafePrefix(string indexName, out string safePrefix)
    {
        if (string.IsNullOrWhiteSpace(indexName))
        {
            safePrefix = null;

            return false;
        }

        indexName = indexName.ToLowerInvariant();

        while (!char.IsLetterOrDigit(indexName[0]))
        {
            indexName = indexName.Remove(0, 1);
        }

        var validChars = Array.FindAll(indexName.ToCharArray(), c => char.IsLetterOrDigit(c) || c == '-');

        safePrefix = new string(validChars);

        while (safePrefix.Contains("--"))
        {
            safePrefix = safePrefix.Replace("--", "-");
        }

        return true;
    }

    private static SearchFieldDataType GetFieldType(Types type)
        => type switch
        {
            Types.Boolean => SearchFieldDataType.Boolean,
            Types.DateTime => SearchFieldDataType.DateTimeOffset,
            Types.Number => SearchFieldDataType.Double,
            Types.Integer => SearchFieldDataType.Int64,
            Types.GeoPoint => SearchFieldDataType.GeographyPoint,
            _ => SearchFieldDataType.String,
        };
}
