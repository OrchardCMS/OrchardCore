using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Nest;
using OrchardCore.Entities;
using OrchardCore.Search.Abstractions;
using OrchardCore.Search.Elasticsearch.Core.Models;
using OrchardCore.Search.Elasticsearch.Core.Services;
using OrchardCore.Settings;

namespace OrchardCore.Search.Elasticsearch.Services;

public class ElasticsearchService : ISearchService
{
    private readonly ISiteService _siteService;
    private readonly ElasticIndexManager _elasticIndexManager;
    private readonly ElasticIndexSettingsService _elasticIndexSettingsService;
    private readonly ElasticAnalyzerManager _elasticAnalyzerManager;
    private readonly IElasticSearchQueryService _elasticsearchQueryService;
    private readonly ILogger _logger;

    public ElasticsearchService(
        ISiteService siteService,
        ElasticIndexManager elasticIndexManager,
        ElasticIndexSettingsService elasticIndexSettingsService,
        ElasticAnalyzerManager elasticAnalyzerManager,
        IElasticSearchQueryService elasticsearchQueryService,
        ILogger<ElasticsearchService> logger
        )
    {
        _siteService = siteService;
        _elasticIndexManager = elasticIndexManager;
        _elasticIndexSettingsService = elasticIndexSettingsService;
        _elasticAnalyzerManager = elasticAnalyzerManager;
        _elasticsearchQueryService = elasticsearchQueryService;
        _logger = logger;
    }

    public string Name => "Elasticsearch";

    public async Task<SearchResult> SearchAsync(string indexName, string term, int start, int pageSize)
    {
        var index = !String.IsNullOrWhiteSpace(indexName) ? indexName.Trim() : await DefaultIndexAsync();

        var result = new SearchResult();

        if (index == null || !await _elasticIndexManager.Exists(index))
        {
            _logger.LogWarning("Elasticsearch: Couldn't execute search. The search index doesn't exist.");

            return result;
        }

        var elasticIndexSettings = await _elasticIndexSettingsService.GetSettingsAsync(index);
        result.Latest = elasticIndexSettings.IndexLatest;

        var analyzer = _elasticAnalyzerManager.CreateAnalyzer(await _elasticIndexSettingsService.GetIndexAnalyzerAsync(index));

        var siteSettings = await _siteService.GetSiteSettingsAsync();
        var searchSettings = siteSettings.As<ElasticSettings>();

        if (searchSettings.DefaultSearchFields == null || searchSettings.DefaultSearchFields.Length == 0)
        {
            _logger.LogWarning("Elasticsearch: Couldn't execute search. No serach provider settings was defined.");

            return result;
        }

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

            result.ContentItemIds = await _elasticsearchQueryService.ExecuteQueryAsync(index, query, null, start, pageSize);
            result.Success = true;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Incorrect Elasticsearch search query syntax provided in search.");
        }

        return result;
    }

    private async Task<string> DefaultIndexAsync()
    {
        var siteSettings = await _siteService.GetSiteSettingsAsync();

        return siteSettings.As<ElasticSettings>().SearchIndex;
    }
}
