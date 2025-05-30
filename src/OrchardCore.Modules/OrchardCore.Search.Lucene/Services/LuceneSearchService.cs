using Lucene.Net.QueryParsers.Classic;
using Microsoft.Extensions.Logging;
using OrchardCore.Entities;
using OrchardCore.Indexing.Models;
using OrchardCore.Search.Abstractions;
using OrchardCore.Search.Lucene.Models;

namespace OrchardCore.Search.Lucene.Services;

public class LuceneSearchService : ISearchService
{
    private readonly LuceneIndexManager _luceneIndexManager;
    private readonly LuceneAnalyzerManager _luceneAnalyzerManager;
    private readonly ILuceneSearchQueryService _luceneSearchQueryService;
    private readonly ILogger _logger;

    public LuceneSearchService(
        LuceneIndexManager luceneIndexManager,
        LuceneAnalyzerManager luceneAnalyzerManager,
        ILuceneSearchQueryService luceneSearchQueryService,
        ILogger<LuceneSearchService> logger)
    {
        _luceneIndexManager = luceneIndexManager;
        _luceneAnalyzerManager = luceneAnalyzerManager;
        _luceneSearchQueryService = luceneSearchQueryService;
        _logger = logger;
    }

    public async Task<SearchResult> SearchAsync(IndexEntity index, string term, int start, int size)
    {
        var result = new SearchResult();

        if (index == null || !await _luceneIndexManager.ExistsAsync(index.IndexFullName))
        {
            _logger.LogWarning("Lucene: Couldn't execute search. Lucene has not been configured yet.");

            return result;
        }

        var queryMetadata = index.As<LuceneIndexDefaultQueryMetadata>();

        if (queryMetadata.DefaultSearchFields == null || queryMetadata.DefaultSearchFields.Length == 0)
        {
            _logger.LogWarning("Lucene: Couldn't execute search. No search provider settings was defined.");

            return result;
        }
        var metadata = index.As<LuceneIndexMetadata>();

        var analyzer = _luceneAnalyzerManager.CreateAnalyzer(metadata.AnalyzerName);
        var queryParser = new MultiFieldQueryParser(queryMetadata.DefaultVersion, queryMetadata.DefaultSearchFields, analyzer);

        try
        {
            var query = queryParser.Parse(term);
            result.ContentItemIds = await _luceneSearchQueryService.ExecuteQueryAsync(query, index.IndexName, start, size);
            result.Success = true;
        }
        catch (ParseException e)
        {
            _logger.LogError(e, "Incorrect Lucene search query syntax provided in search.");
        }

        return result;
    }
}
