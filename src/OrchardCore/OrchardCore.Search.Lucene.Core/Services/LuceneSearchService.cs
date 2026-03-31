using Lucene.Net.QueryParsers.Classic;
using Microsoft.Extensions.Logging;
using OrchardCore.Entities;
using OrchardCore.Indexing.Models;
using OrchardCore.Search.Abstractions;
using OrchardCore.Search.Lucene.Models;

namespace OrchardCore.Search.Lucene.Services;

public class LuceneSearchService : ISearchService
{
    private readonly LuceneIndexManager _indexManager;
    private readonly LuceneAnalyzerManager _analyzerManager;
    private readonly ILuceneSearchQueryService _searchQueryService;
    private readonly ILogger _logger;

    public LuceneSearchService(
        LuceneIndexManager indexManager,
        LuceneAnalyzerManager analyzerManager,
        ILuceneSearchQueryService searchQueryService,
        ILogger<LuceneSearchService> logger)
    {
        _indexManager = indexManager;
        _analyzerManager = analyzerManager;
        _searchQueryService = searchQueryService;
        _logger = logger;
    }

    public string Name
        => LuceneConstants.ProviderName;

    public async Task<SearchResult> SearchAsync(IndexProfile index, string term, int start, int size)
    {
        var result = new SearchResult();

        if (index == null || !await _indexManager.ExistsAsync(index.IndexFullName))
        {
            _logger.LogWarning("Lucene: Couldn't execute search. Lucene has not been configured yet.");

            return result;
        }

        var defaultVersion = LuceneConstants.DefaultVersion;
        string[] defaultSearchFields = null;
        string analyzerName = null;

        if (index.TryGet<LuceneIndexDefaultQueryMetadata>(out var queryMetadata))
        {
            defaultVersion = queryMetadata.DefaultVersion;
            defaultSearchFields = queryMetadata.DefaultSearchFields;
        }

        if (index.TryGet<LuceneIndexMetadata>(out var indexMetadata))
        {
            analyzerName = indexMetadata.AnalyzerName;

            if (defaultSearchFields is null || defaultSearchFields.Length == 0)
            {
                defaultSearchFields = indexMetadata.IndexMappings?.Fields;
            }
        }

        if (defaultSearchFields == null || defaultSearchFields.Length == 0)
        {
            _logger.LogWarning("Lucene: Couldn't execute search. No search provider settings was defined.");

            return result;
        }

        var analyzer = _analyzerManager.CreateAnalyzer(analyzerName);
        var queryParser = new MultiFieldQueryParser(defaultVersion, defaultSearchFields, analyzer);

        try
        {
            var query = queryParser.Parse(term);
            result.ContentItemIds = await _searchQueryService.ExecuteQueryAsync(query, index.IndexName, start, size);
            result.Success = true;
        }
        catch (ParseException e)
        {
            _logger.LogError(e, "Incorrect Lucene search query syntax provided in search.");
        }

        return result;
    }
}
