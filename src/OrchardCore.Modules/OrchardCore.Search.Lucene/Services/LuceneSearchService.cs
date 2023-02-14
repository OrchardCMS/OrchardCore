using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Lucene.Net.QueryParsers.Classic;
using Microsoft.Extensions.Logging;
using OrchardCore.Entities;
using OrchardCore.Search.Abstractions;
using OrchardCore.Search.Lucene.Model;
using OrchardCore.Security.Permissions;
using OrchardCore.Settings;

namespace OrchardCore.Search.Lucene.Services;

public class LuceneSearchService : ISearchService
{
    private readonly ISiteService _siteService;
    private readonly IEnumerable<IPermissionProvider> _permissionProviders;
    private readonly LuceneIndexManager _luceneIndexManager;
    private readonly LuceneIndexingService _luceneIndexingService;
    private readonly LuceneAnalyzerManager _luceneAnalyzerManager;
    private readonly LuceneIndexSettingsService _luceneIndexSettingsService;
    private readonly ILuceneSearchQueryService _luceneSearchQueryService;
    private readonly ILogger _logger;
    private readonly SearchProvider _searchProvider = new LuceneSearchProvider();

    public LuceneSearchService(
        ISiteService siteService,
        IEnumerable<IPermissionProvider> permissionProviders,
        LuceneIndexManager luceneIndexManager,
        LuceneIndexingService luceneIndexingService,
        LuceneAnalyzerManager luceneAnalyzerManager,
        LuceneIndexSettingsService luceneIndexSettingsService,
        ILuceneSearchQueryService luceneSearchQueryService,
        ILogger<LuceneSearchService> logger)
    {
        _siteService = siteService;
        _permissionProviders = permissionProviders;
        _luceneIndexManager = luceneIndexManager;
        _luceneIndexingService = luceneIndexingService;
        _luceneAnalyzerManager = luceneAnalyzerManager;
        _luceneIndexSettingsService = luceneIndexSettingsService;
        _luceneSearchQueryService = luceneSearchQueryService;
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
        var searchSettings = siteSettings.As<LuceneSettings>();

        return searchSettings?.SearchIndex;
    }

    public Task<bool> ExistsAsync(string indexName)
    {
        return Task.FromResult(_luceneIndexManager.Exists(indexName));
    }

    public async Task<SearchResult> GetAsync(string indexName, string term, string[] defaultSearchFields, int start, int size)
    {
        if (String.IsNullOrWhiteSpace(indexName))
        {
            throw new ArgumentException($"{nameof(indexName)} cannot be null or empty.");
        }

        var analyzer = _luceneAnalyzerManager.CreateAnalyzer(await _luceneIndexSettingsService.GetIndexAnalyzerAsync(indexName));
        var queryParser = new MultiFieldQueryParser(LuceneSettings.DefaultVersion, defaultSearchFields, analyzer);
        var result = new SearchResult();

        try
        {
            var query = queryParser.Parse(term);
            result.ContentItemIds = await _luceneSearchQueryService.ExecuteQueryAsync(query, indexName, start, size);
            result.Success = true;
        }
        catch (ParseException e)
        {
            _logger.LogError(e, "Incorrect Lucene search query syntax provided in search.");

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

        var permissionsProvider = _permissionProviders.FirstOrDefault(x => x.GetType().FullName == "OrchardCore.Search.Lucene.Permissions");

        if (permissionsProvider == null)
        {
            return null;
        }

        var permissions = await permissionsProvider.GetPermissionsAsync();

        return permissions?.FirstOrDefault(x => x.Name == $"QueryLucene{indexName}Index");
    }

    public async Task<string[]> GetSearchFieldsAsync(string indexName)
    {
        var luceneSettings = await _luceneIndexingService.GetLuceneSettingsAsync();

        return luceneSettings?.DefaultSearchFields ?? Array.Empty<string>();
    }
}
