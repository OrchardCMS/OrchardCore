using System;
using System.Text;
using System.Threading.Tasks;
using Azure;
using Azure.Search.Documents.Indexes;
using Azure.Search.Documents.Indexes.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OrchardCore.Environment.Shell;
using OrchardCore.Search.Azure.CognitiveSearch.Models;

namespace OrchardCore.Search.Azure.CognitiveSearch.Services;

public class AzureCognitiveSearchIndexManager(
    SearchIndexClient client,
    ILogger<AzureCognitiveSearchIndexManager> logger,
    IOptions<AzureCognitiveSearchOptions> azureCognitiveSearchOptions,
    ShellSettings shellSettings)
{
    private readonly SearchIndexClient _client = client;
    private readonly ILogger _logger = logger;
    private readonly ShellSettings _shellSettings = shellSettings;
    private readonly AzureCognitiveSearchOptions _azureCognitiveSearchOptions = azureCognitiveSearchOptions.Value;
    private string _indexPrefix;


    public async Task<bool> CreateAsync(CognitiveSearchSettings settings)
    {
        if (await ExistsAsync(settings.IndexName))
        {
            return true;
        }

        try
        {
            var fullIndexName = GetFullIndexName(settings.IndexName);

            var searchIndex = new SearchIndex(fullIndexName)
            {
                Fields =
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
                    new SimpleField(IndexingConstants.OwnerKey, SearchFieldDataType.String)
                    {
                        IsFilterable = true,
                        IsSortable = true,
                    },
                    new SearchableField(IndexingConstants.FullTextKey)
                    {
                        AnalyzerName = settings.AnalyzerName,
                    },
                    new SearchableField(IndexingConstants.DisplayTextKey)
                    {
                        AnalyzerName = settings.AnalyzerName,
                    },
                    new SimpleField(IndexingConstants.ContentTypeKey, SearchFieldDataType.String)
                    {
                        IsFilterable = true,
                        IsSortable = true,
                    },
                },
                Suggesters =
                {
                    new SearchSuggester("sg", IndexingConstants.FullTextKey),
                },
            };

            var response = await _client.CreateIndexAsync(searchIndex);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unable to create index in Azure Cognitive Serach.");
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
                _logger.LogError(e, "Unable to get index from Azure Cognitive Serach.");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unable to get index from Azure Cognitive Serach.");
        }

        return null;
    }

    /// <summary>
    /// Makes sure that the index names are compliant with Azure Cognitive Search specifications.
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

    private bool TryGetSafePrefix(string indexName, out string safePrefix)
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

    public string GetFullIndexName(string indexName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(indexName);

        return GetIndexPrefix() + '-' + indexName;
    }

    private string GetIndexPrefix()
    {
        if (_indexPrefix == null)
        {
            var builder = new StringBuilder();

            if (!string.IsNullOrWhiteSpace(_azureCognitiveSearchOptions.IndexPrefix))
            {
                builder.Append(_azureCognitiveSearchOptions.IndexPrefix.ToLowerInvariant());
                builder.Append('-');
            }

            builder.Append(_shellSettings.Name.ToLowerInvariant());

            if (TryGetSafePrefix(builder.ToString(), out var safePrefix))
            {
                _indexPrefix = safePrefix;
            }
        }

        return _indexPrefix;
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
            _logger.LogError(ex, "Unable to delete index from Azure Cognitive Serach.");
        }

        return false;
    }
}
