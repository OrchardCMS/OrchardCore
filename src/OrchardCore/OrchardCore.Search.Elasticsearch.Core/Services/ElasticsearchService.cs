using System.Text;
using System.Text.Encodings.Web;
using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.QueryDsl;
using Fluid.Values;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OrchardCore.Liquid;
using OrchardCore.Search.Abstractions;
using OrchardCore.Search.Elasticsearch.Core.Models;
using OrchardCore.Search.Elasticsearch.Core.Services;
using OrchardCore.Settings;

namespace OrchardCore.Search.Elasticsearch.Services;

public class ElasticsearchService : ISearchService
{
    public const string Key = "Elasticsearch";

    private readonly ISiteService _siteService;
    private readonly ElasticsearchIndexManager _elasticIndexManager;
    private readonly ElasticsearchIndexSettingsService _elasticIndexSettingsService;
    private readonly ElasticsearchClient _elasticClient;
    private readonly JavaScriptEncoder _javaScriptEncoder;
    private readonly ElasticsearchConnectionOptions _elasticConnectionOptions;
    private readonly ILiquidTemplateManager _liquidTemplateManager;
    private readonly ElasticsearchQueryService _elasticsQueryService;
    private readonly ILogger _logger;

    public ElasticsearchService(
        ISiteService siteService,
        ElasticsearchIndexManager elasticIndexManager,
        ElasticsearchIndexSettingsService elasticIndexSettingsService,
        ElasticsearchClient elasticClient,
        JavaScriptEncoder javaScriptEncoder,
        IOptions<ElasticsearchConnectionOptions> elasticConnectionOptions,
        ILiquidTemplateManager liquidTemplateManager,
        ElasticsearchQueryService elasticQueryService,
        ILogger<ElasticsearchService> logger
        )
    {
        _siteService = siteService;
        _elasticIndexManager = elasticIndexManager;
        _elasticIndexSettingsService = elasticIndexSettingsService;
        _elasticClient = elasticClient;
        _javaScriptEncoder = javaScriptEncoder;
        _elasticConnectionOptions = elasticConnectionOptions.Value;
        _liquidTemplateManager = liquidTemplateManager;
        _elasticsQueryService = elasticQueryService;
        _logger = logger;
    }

    public string Name => Key;

    public async Task<SearchResult> SearchAsync(string indexName, string term, int start, int pageSize)
    {
        var result = new SearchResult();

        if (!_elasticConnectionOptions.ConfigurationExists())
        {
            _logger.LogWarning("Elasticsearch: Couldn't execute search. Elasticsearch has not been configured yet.");

            return result;
        }

        var searchSettings = await _siteService.GetSettingsAsync<ElasticSettings>();

        var index = !string.IsNullOrWhiteSpace(indexName)
            ? indexName.Trim()
            : searchSettings.SearchIndex ?? (await _elasticIndexSettingsService.GetSettingsAsync()).FirstOrDefault()?.IndexName;

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

        try
        {
            var searchType = searchSettings.GetSearchType();
            Query query = null;

            if (searchType == ElasticSettings.CustomSearchType && !string.IsNullOrWhiteSpace(searchSettings.DefaultQuery))
            {
                var tokenizedContent = await _liquidTemplateManager.RenderStringAsync(searchSettings.DefaultQuery, _javaScriptEncoder,
                    new Dictionary<string, FluidValue>()
                    {
                        ["term"] = new StringValue(term)
                    });

                try
                {
                    using var stream = new MemoryStream(Encoding.UTF8.GetBytes(tokenizedContent));

                    var searchRequest = await _elasticClient.RequestResponseSerializer.DeserializeAsync<SearchRequest>(stream);

                    query = searchRequest.Query;
                }
                catch { }
            }
            else if (searchType == ElasticSettings.QueryStringSearchType)
            {
                query = new QueryStringQuery
                {
                    Fields = searchSettings.DefaultSearchFields,
                    Analyzer = await _elasticIndexSettingsService.GetQueryAnalyzerAsync(index),
                    Query = term
                };
            }

            query ??= new MultiMatchQuery
            {
                Fields = searchSettings.DefaultSearchFields,
                Analyzer = await _elasticIndexSettingsService.GetQueryAnalyzerAsync(index),
                Query = term
            };

            var searchContext = new ElasticsearchSearchContext(index, query)
            {
                From = start,
                Size = pageSize,
            };

            result.ContentItemIds = await _elasticsQueryService.GetContentItemIdsAsync(searchContext);

            result.Success = true;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Incorrect Elasticsearch search query syntax provided in search.");
        }

        return result;
    }
}
