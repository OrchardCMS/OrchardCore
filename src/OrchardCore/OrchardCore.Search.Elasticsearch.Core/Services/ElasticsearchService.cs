using System.Text;
using System.Text.Encodings.Web;
using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.QueryDsl;
using Fluid.Values;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OrchardCore.Entities;
using OrchardCore.Indexing.Core.Models;
using OrchardCore.Indexing.Models;
using OrchardCore.Liquid;
using OrchardCore.Search.Abstractions;
using OrchardCore.Search.Elasticsearch.Core.Models;
using OrchardCore.Search.Elasticsearch.Core.Services;

namespace OrchardCore.Search.Elasticsearch.Services;

public class ElasticsearchService : ISearchService
{
    private readonly ElasticsearchIndexManager _elasticIndexManager;
    private readonly ElasticsearchClient _elasticsearchClient;
    private readonly JavaScriptEncoder _javaScriptEncoder;
    private readonly ElasticsearchConnectionOptions _elasticConnectionOptions;
    private readonly ILiquidTemplateManager _liquidTemplateManager;
    private readonly ElasticsearchQueryService _elasticsQueryService;
    private readonly ILogger _logger;

    public ElasticsearchService(
        ElasticsearchIndexManager elasticIndexManager,
        ElasticsearchClient elasticsearchClient,
        JavaScriptEncoder javaScriptEncoder,
        IOptions<ElasticsearchConnectionOptions> elasticConnectionOptions,
        ILiquidTemplateManager liquidTemplateManager,
        ElasticsearchQueryService elasticQueryService,
        ILogger<ElasticsearchService> logger
        )
    {
        _elasticIndexManager = elasticIndexManager;
        _elasticsearchClient = elasticsearchClient;
        _javaScriptEncoder = javaScriptEncoder;
        _elasticConnectionOptions = elasticConnectionOptions.Value;
        _liquidTemplateManager = liquidTemplateManager;
        _elasticsQueryService = elasticQueryService;
        _logger = logger;
    }

    public string Name
        => ElasticsearchConstants.ProviderName;

    public async Task<SearchResult> SearchAsync(IndexProfile index, string term, int start, int pageSize)
    {
        ArgumentNullException.ThrowIfNull(index);

        var result = new SearchResult();

        if (!_elasticConnectionOptions.ConfigurationExists())
        {
            _logger.LogWarning("Elasticsearch: Couldn't execute search. Elasticsearch has not been configured yet.");

            return result;
        }

        var metadata = index.As<ContentIndexMetadata>();

        var queryMetadata = index.As<ElasticsearchDefaultQueryMetadata>();

        if (index == null || !await _elasticIndexManager.ExistsAsync(index.IndexFullName))
        {
            _logger.LogWarning("Elasticsearch: Couldn't execute search. The search index doesn't exist.");

            return result;
        }

        result.Latest = metadata.IndexLatest;

        if (queryMetadata.DefaultSearchFields == null || queryMetadata.DefaultSearchFields.Length == 0)
        {
            _logger.LogWarning("Elasticsearch: Couldn't execute search. No default query settings were configured.");

            return result;
        }

        try
        {
            var searchType = queryMetadata.GetSearchType();

            SearchRequest searchRequest;

            if (searchType == ElasticsearchConstants.CustomSearchType)
            {
                var tokenizedContent = string.IsNullOrWhiteSpace(queryMetadata.DefaultQuery)
                    ? "{}"
                    : await _liquidTemplateManager.RenderStringAsync(queryMetadata.DefaultQuery, _javaScriptEncoder,
                    new Dictionary<string, FluidValue>()
                    {
                        ["term"] = new StringValue(term),
                    });

                try
                {
                    using var stream = new MemoryStream(Encoding.UTF8.GetBytes(tokenizedContent));

                    searchRequest = await _elasticsearchClient.RequestResponseSerializer.DeserializeAsync<SearchRequest>(stream);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Incorrect Elasticsearch search query syntax provided in custom query.");

                    var metadataIndex = index.As<ElasticsearchIndexMetadata>();

                    searchRequest = new()
                    {
                        Query = new MultiMatchQuery
                        {
                            Fields = queryMetadata.DefaultSearchFields,
                            Analyzer = metadataIndex.GetQueryAnalyzerName(),
                            Query = term,
                        },
                    };
                }
            }
            else if (searchType == ElasticsearchConstants.QueryStringSearchType)
            {
                var metadataIndex = index.As<ElasticsearchIndexMetadata>();

                searchRequest = new()
                {
                    Query = new QueryStringQuery
                    {
                        Fields = queryMetadata.DefaultSearchFields,
                        Analyzer = metadataIndex.GetQueryAnalyzerName(),
                        Query = term,
                    },
                };
            }
            else
            {
                var metadataIndex = index.As<ElasticsearchIndexMetadata>();

                searchRequest = new()
                {
                    Query = new MultiMatchQuery
                    {
                        Fields = queryMetadata.DefaultSearchFields,
                        Analyzer = metadataIndex.GetQueryAnalyzerName(),
                        Query = term,
                    },
                };
            }

            searchRequest.Indices = index.IndexFullName;
            searchRequest.From = start;
            searchRequest.Size = pageSize;

            var searchContext = new ElasticsearchSearchContext(index, searchRequest);

            await _elasticsQueryService.PopulateResultAsync(searchContext, result);

            result.Success = true;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Incorrect Elasticsearch search query syntax provided in search.");
        }

        return result;
    }
}
