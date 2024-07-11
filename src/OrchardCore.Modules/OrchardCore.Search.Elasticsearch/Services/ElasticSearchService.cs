using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Fluid.Values;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Nest;
using OrchardCore.Liquid;
using OrchardCore.Search.Abstractions;
using OrchardCore.Search.Elasticsearch.Core.Models;
using OrchardCore.Search.Elasticsearch.Core.Services;
using OrchardCore.Settings;

namespace OrchardCore.Search.Elasticsearch.Services;

using ElasticSearchDescriptor = SearchDescriptor<Dictionary<string, object>>;

public class ElasticsearchService : ISearchService
{
    public const string Key = "Elasticsearch";

    private readonly ISiteService _siteService;
    private readonly IElasticIndexManager _elasticIndexManager;
    private readonly ElasticIndexSettingsService _elasticIndexSettingsService;
    private readonly JavaScriptEncoder _javaScriptEncoder;
    private readonly ElasticConnectionOptions _elasticConnectionOptions;
    private readonly ILiquidTemplateManager _liquidTemplateManager;
    private readonly ILogger _logger;

    public ElasticsearchService(
        ISiteService siteService,
        IElasticIndexManager elasticIndexManager,
        ElasticIndexSettingsService elasticIndexSettingsService,
        JavaScriptEncoder javaScriptEncoder,
        IOptions<ElasticConnectionOptions> elasticConnectionOptions,
        ILiquidTemplateManager liquidTemplateManager,
        ILogger<ElasticsearchService> logger
        )
    {
        _siteService = siteService;
        _elasticIndexManager = elasticIndexManager;
        _elasticIndexSettingsService = elasticIndexSettingsService;
        _javaScriptEncoder = javaScriptEncoder;
        _elasticConnectionOptions = elasticConnectionOptions.Value;
        _liquidTemplateManager = liquidTemplateManager;
        _logger = logger;
    }

    public string Name => Key;

    public async Task<SearchResult> SearchAsync(string indexName, string term, int start, int pageSize)
    {
        var result = new SearchResult();

        if (!_elasticConnectionOptions.FileConfigurationExists())
        {
            _logger.LogWarning("Elasticsearch: Couldn't execute search. Elasticsearch has not been configured yet.");

            return result;
        }

        var searchSettings = await _siteService.GetSettingsAsync<ElasticSettings>();

        var index = !string.IsNullOrWhiteSpace(indexName) ? indexName.Trim() : searchSettings.SearchIndex;

        if (index == null || !await _elasticIndexManager.ExistsAsync(index))
        {
            _logger.LogWarning("Elasticsearch: Couldn't execute search. The search index doesn't exist.");

            return result;
        }

        var elasticIndexSettings = await _elasticIndexSettingsService.GetSettingsAsync(index);
        result.Latest = elasticIndexSettings.IndexLatest;

        if (searchSettings.DefaultSearchFields == null || searchSettings.DefaultSearchFields.Length == 0)
        {
            _logger.LogWarning("Elasticsearch: Couldn't execute search. No search provider settings was defined.");

            return result;
        }

        var searchType = searchSettings.GetSearchType();
        Func<ElasticSearchDescriptor, ElasticSearchDescriptor> formatSearch = null;
        if (searchType == ElasticSettings.CustomSearchType && !string.IsNullOrWhiteSpace(searchSettings.DefaultQuery))
        {
            var tokenizedContent = await _liquidTemplateManager.RenderStringAsync(searchSettings.DefaultQuery, _javaScriptEncoder,
                new Dictionary<string, FluidValue>()
                {
                    ["term"] = new StringValue(term)
                });

            try
            {
                var searchDescriptor = await _elasticIndexManager.DeserializeSearchDescriptor(tokenizedContent);
                formatSearch = _ => searchDescriptor;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Incorrect Elasticsearch search query syntax provided in search.");
            }
        }
        else if (searchType == ElasticSettings.QueryStringSearchType)
        {
            var analyzer = await _elasticIndexSettingsService.GetQueryAnalyzerAsync(index);
            formatSearch = descriptor =>
                descriptor.Query(q => q.QueryString(qs => qs
                   .Fields(searchSettings.DefaultSearchFields)
                   .Analyzer(analyzer)
                   .Query(term)
                ));
        }

        if (formatSearch == null)
        {
            var analyzer = await _elasticIndexSettingsService.GetQueryAnalyzerAsync(index);
            formatSearch = descriptor =>
               descriptor.Query(q => q.MultiMatch(qs => qs
                  .Fields(searchSettings.DefaultSearchFields)
                  .Analyzer(analyzer)
                  .Query(term)
               ));
        }

        var elasticTopDocs = await _elasticIndexManager.SearchAsync(index, descriptor =>
            formatSearch(descriptor)
            .Size(pageSize)
            .From(start));

        result.ContentItemIds = elasticTopDocs.TopDocs.Select(item => item.GetValueOrDefault("ContentItemId").ToString()).ToList();
        result.Success = true;
        return result;
    }
}
