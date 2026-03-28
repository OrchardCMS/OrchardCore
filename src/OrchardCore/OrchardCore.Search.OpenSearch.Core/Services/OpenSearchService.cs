using System.Text;
using System.Text.Encodings.Web;
using Fluid.Values;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OpenSearch.Client;
using OrchardCore.Entities;
using OrchardCore.Indexing.Core.Models;
using OrchardCore.Indexing.Models;
using OrchardCore.Liquid;
using OrchardCore.Search.Abstractions;
using OrchardCore.Search.OpenSearch.Core.Models;
using OrchardCore.Search.OpenSearch.Core.Services;

namespace OrchardCore.Search.OpenSearch.Services;

public class OpenSearchService : ISearchService
{
    private readonly OpenSearchIndexManager _openSearchIndexManager;
    private readonly OpenSearchClient _openSearchClient;
    private readonly JavaScriptEncoder _javaScriptEncoder;
    private readonly OpenSearchConnectionOptions _openSearchConnectionOptions;
    private readonly ILiquidTemplateManager _liquidTemplateManager;
    private readonly OpenSearchQueryService _openSearchQueryService;
    private readonly ILogger _logger;

    public OpenSearchService(
        OpenSearchIndexManager openSearchIndexManager,
        OpenSearchClient openSearchClient,
        JavaScriptEncoder javaScriptEncoder,
        IOptions<OpenSearchConnectionOptions> openSearchConnectionOptions,
        ILiquidTemplateManager liquidTemplateManager,
        OpenSearchQueryService openSearchQueryService,
        ILogger<OpenSearchService> logger)
    {
        _openSearchIndexManager = openSearchIndexManager;
        _openSearchClient = openSearchClient;
        _javaScriptEncoder = javaScriptEncoder;
        _openSearchConnectionOptions = openSearchConnectionOptions.Value;
        _liquidTemplateManager = liquidTemplateManager;
        _openSearchQueryService = openSearchQueryService;
        _logger = logger;
    }

    public string Name
        => OpenSearchConstants.ProviderName;

    public async Task<SearchResult> SearchAsync(IndexProfile index, string term, int start, int pageSize)
    {
        ArgumentNullException.ThrowIfNull(index);

        var result = new SearchResult();

        if (!_openSearchConnectionOptions.ConfigurationExists())
        {
            _logger.LogWarning("OpenSearch: Couldn't execute search. OpenSearch has not been configured yet.");

            return result;
        }

        var metadata = index.As<ContentIndexMetadata>();

        var queryMetadata = index.As<OpenSearchDefaultQueryMetadata>();

        if (index == null || !await _openSearchIndexManager.ExistsAsync(index.IndexFullName))
        {
            _logger.LogWarning("OpenSearch: Couldn't execute search. The search index doesn't exist.");

            return result;
        }

        result.Latest = metadata.IndexLatest;

        if (queryMetadata.DefaultSearchFields == null || queryMetadata.DefaultSearchFields.Length == 0)
        {
            _logger.LogWarning("OpenSearch: Couldn't execute search. No default query settings were configured.");

            return result;
        }

        try
        {
            var searchType = queryMetadata.GetSearchType();

            SearchRequest searchRequest;

            if (searchType == OpenSearchConstants.CustomSearchType)
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

                    searchRequest = _openSearchClient.RequestResponseSerializer.Deserialize<SearchRequest>(stream);
                    searchRequest ??= BuildDefaultSearchRequest(index.IndexFullName, queryMetadata, index.As<OpenSearchIndexMetadata>(), term);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Incorrect OpenSearch search query syntax provided in custom query.");

                    searchRequest = BuildDefaultSearchRequest(index.IndexFullName, queryMetadata, index.As<OpenSearchIndexMetadata>(), term);
                }
            }
            else if (searchType == OpenSearchConstants.QueryStringSearchType)
            {
                var metadataIndex = index.As<OpenSearchIndexMetadata>();

                searchRequest = new SearchRequest(index.IndexFullName)
                {
                    Query = new QueryStringQuery
                    {
                        Fields = queryMetadata.DefaultSearchFields,
                        Analyzer = metadataIndex.GetQueryAnalyzerName(),
                        Query = term,
                    },
                    From = start,
                    Size = pageSize,
                };
            }
            else
            {
                searchRequest = BuildDefaultSearchRequest(index.IndexFullName, queryMetadata, index.As<OpenSearchIndexMetadata>(), term);
            }

            searchRequest.From = start;
            searchRequest.Size = pageSize;

            var searchContext = new OpenSearchSearchContext(index, searchRequest);

            await _openSearchQueryService.PopulateResultAsync(searchContext, result);

            result.Success = true;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Incorrect OpenSearch search query syntax provided in search.");
        }

        return result;
    }

    private static SearchRequest BuildDefaultSearchRequest(
        string indexFullName,
        OpenSearchDefaultQueryMetadata queryMetadata,
        OpenSearchIndexMetadata metadataIndex,
        string term)
    {
        return new SearchRequest(indexFullName)
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
