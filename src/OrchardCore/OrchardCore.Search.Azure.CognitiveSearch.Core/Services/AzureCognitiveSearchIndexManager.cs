using System;
using System.Text;
using System.Threading.Tasks;
using Azure.Search.Documents.Indexes;
using Azure.Search.Documents.Indexes.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OrchardCore.Contents.Indexing;
using OrchardCore.Environment.Shell;
using OrchardCore.Search.Azure.CognitiveSearch.Models;
using OrchardCore.Settings;

namespace OrchardCore.Search.Azure.CognitiveSearch.Services;

public class AzureCognitiveSearchIndexManager(
    SearchIndexClient client,
    ISiteService siteService,
    ILogger<AzureCognitiveSearchIndexManager> logger,
    IOptions<AzureCognitiveSearchOptions> azureCognitiveSearchOptions,
    ShellSettings shellSettings)
{
    private readonly SearchIndexClient _client = client;
    private readonly ISiteService _siteService = siteService;
    private readonly ILogger _logger = logger;
    private readonly ShellSettings _shellSettings = shellSettings;
    private readonly AzureCognitiveSearchOptions _azureCognitiveSearchOptions = azureCognitiveSearchOptions.Value;
    private string _indexPrefix;

    public async Task<bool> CreateAsync(string indexName)
    {
        if (await ExistsAsync(indexName))
        {
            return true;
        }

        var site = await _siteService.GetSiteSettingsAsync();
        var searchSettings = site.As<AzureCognitiveSearchSettings>();

        var searchIndex = new SearchIndex(GetFullIndexName(indexName))
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
                    IsKey = true,
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
                    AnalyzerName = searchSettings.IndexAnalyzerName,
                },
                new SearchableField(IndexingConstants.DisplayTextKey)
                {
                    AnalyzerName = searchSettings.IndexAnalyzerName,
                },
                new SimpleField(IndexingConstants.ContentTypeKey, SearchFieldDataType.String)
                {
                    IsKey = true,
                    IsFilterable = true,
                    IsSortable = true,
                },
            },
            Suggesters =
            {
                new SearchSuggester("term", IndexingConstants.FullTextKey),
            },
        };

        try
        {
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
            var response = await _client.GetIndexAsync(GetFullIndexName(indexName));

            return response?.Value;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unable to get index from Azure Cognitive Serach.");
        }

        return null;
    }

    public string GetFullIndexName(string indexName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(indexName);

        return GetIndexPrefix() + '_' + indexName;
    }

    private string GetIndexPrefix()
    {
        if (_indexPrefix == null)
        {
            var builder = new StringBuilder();

            if (!string.IsNullOrWhiteSpace(_azureCognitiveSearchOptions.IndexPrefix))
            {
                builder.Append(_azureCognitiveSearchOptions.IndexPrefix.ToLowerInvariant());
                builder.Append('_');
            }

            builder.Append(_shellSettings.Name.ToLowerInvariant());
            builder.Append('_');

            _indexPrefix = builder.ToString();
        }

        return _indexPrefix;
    }
}
