using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Nest;
using OrchardCore.Entities;
using OrchardCore.Search.Abstractions;
using OrchardCore.Search.Elasticsearch.Core.Models;
using OrchardCore.Search.Elasticsearch.Core.Services;
using OrchardCore.Search.Elasticsearch.Providers;
using OrchardCore.Security.Permissions;
using OrchardCore.Settings;

namespace OrchardCore.Search.Elasticsearch.Services;

public class ElasticSearchService : ISearchService
{
    private readonly IEnumerable<IPermissionProvider> _permissionProviders;
    private readonly ISiteService _siteService;
    private readonly ElasticIndexManager _elasticIndexManager;
    private readonly ElasticIndexingService _elasticIndexingService;
    private readonly ElasticIndexSettingsService _elasticIndexSettingsService;
    private readonly ElasticAnalyzerManager _elasticAnalyzerManager;
    private readonly IElasticSearchQueryService _elasticSearchQueryService;
    private readonly ILogger _logger;
    private readonly SearchProvider _searchProvider = new ElasticSearchProvider();

    public ElasticSearchService(
        IEnumerable<IPermissionProvider> permissionProviders,
        ISiteService siteService,
        ElasticIndexManager elasticIndexManager,
        ElasticIndexingService elasticIndexingService,
        ElasticIndexSettingsService elasticIndexSettingsService,
        ElasticAnalyzerManager elasticAnalyzerManager,
        IElasticSearchQueryService elasticSearchQueryService,
        ILogger<ElasticSearchService> logger
        )
    {
        _permissionProviders = permissionProviders;
        _siteService = siteService;
        _elasticIndexManager = elasticIndexManager;
        _elasticIndexingService = elasticIndexingService;
        _elasticIndexSettingsService = elasticIndexSettingsService;
        _elasticAnalyzerManager = elasticAnalyzerManager;
        _elasticSearchQueryService = elasticSearchQueryService;
        _logger = logger;
    }

    public bool CanHandle(SearchProvider provider)
    {
        if (provider == null)
        {
            throw new ArgumentNullException(nameof(provider));
        }

        return String.Equals(provider.AreaName, _searchProvider.AreaName)
            && String.Equals(provider.Name, _searchProvider.Name);
    }

    public async Task<string> DefaultIndexAsync()
    {
        var siteSettings = await _siteService.GetSiteSettingsAsync();
        var searchSettings = siteSettings.As<ElasticSettings>();

        return searchSettings?.SearchIndex;
    }

    public Task<bool> ExistsAsync(string indexName)
    {
        return _elasticIndexManager.Exists(indexName);
    }

    public async Task<SearchResult> GetAsync(string indexName, string term, string[] defaultSearchFields, int start, int pageSize)
    {
        if (String.IsNullOrWhiteSpace(indexName))
        {
            throw new ArgumentException($"{nameof(indexName)} cannot be null or empty.");
        }

        var elasticIndexSettings = await _elasticIndexSettingsService.GetSettingsAsync(indexName);

        var analyzer = _elasticAnalyzerManager.CreateAnalyzer(await _elasticIndexSettingsService.GetIndexAnalyzerAsync(indexName));

        var siteSettings = await _siteService.GetSiteSettingsAsync();
        var searchSettings = siteSettings.As<ElasticSettings>();

        var result = new SearchResult()
        {
            Latest = elasticIndexSettings.IndexLatest,
        };

        try
        {
            QueryContainer query = null;

            if (searchSettings.AllowElasticQueryStringQueryInSearch)
            {
                query = new QueryStringQuery
                {
                    Fields = searchSettings.DefaultSearchFields,
                    Analyzer = analyzer.Type,
                    Query = term
                };
            }
            else
            {
                query = new MultiMatchQuery
                {
                    Fields = searchSettings.DefaultSearchFields,
                    Analyzer = analyzer.Type,
                    Query = term
                };
            }

            result.ContentItemIds = await _elasticSearchQueryService.ExecuteQueryAsync(searchSettings.SearchIndex, query, null, start, pageSize);
            result.Success = true;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Incorrect Elasticsearch search query syntax provided in search.");

            result.Error = e.Message;
        }

        return result;
    }

    public async Task<Permission> GetPermissionAsync(string indexName)
    {
        if (String.IsNullOrWhiteSpace(indexName))
        {
            throw new ArgumentException($"{nameof(indexName)} cannot be null or empty.");
        }

        var permissionsProvider = _permissionProviders.FirstOrDefault(x => x.GetType().FullName == "OrchardCore.Search.Elasticsearch.Permissions");

        if (permissionsProvider == null)
        {
            return null;
        }

        var permissions = await permissionsProvider.GetPermissionsAsync();

        return permissions?.FirstOrDefault(x => x.Name == $"QueryElasticsearch{indexName}Index");
    }

    public async Task<string[]> GetSearchFieldsAsync(string indexName)
    {
        if (String.IsNullOrWhiteSpace(indexName))
        {
            throw new ArgumentException($"{nameof(indexName)} cannot be null or empty.");
        }

        var elasticSettings = await _elasticIndexingService.GetElasticSettingsAsync();

        return elasticSettings?.DefaultSearchFields ?? Array.Empty<string>();
    }
}
