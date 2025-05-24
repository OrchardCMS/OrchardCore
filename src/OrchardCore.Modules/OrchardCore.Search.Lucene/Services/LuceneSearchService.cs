using Lucene.Net.QueryParsers.Classic;
using Microsoft.Extensions.Logging;
using OrchardCore.Entities;
using OrchardCore.Indexing.Models;
using OrchardCore.Search.Abstractions;
using OrchardCore.Search.Lucene.Model;
using OrchardCore.Settings;

namespace OrchardCore.Search.Lucene.Services;

public class LuceneSearchService : ISearchService
{
    public const string Key = "Lucene";

    private readonly ISiteService _siteService;
    private readonly LuceneIndexManager _luceneIndexManager;
    private readonly LuceneIndexingService _luceneIndexingService;
    private readonly LuceneAnalyzerManager _luceneAnalyzerManager;
    private readonly LuceneIndexSettingsService _luceneIndexSettingsService;
    private readonly ILuceneSearchQueryService _luceneSearchQueryService;
    private readonly ILogger _logger;

    public LuceneSearchService(
        ISiteService siteService,
        LuceneIndexManager luceneIndexManager,
        LuceneIndexingService luceneIndexingService,
        LuceneAnalyzerManager luceneAnalyzerManager,
        LuceneIndexSettingsService luceneIndexSettingsService,
        ILuceneSearchQueryService luceneSearchQueryService,
        ILogger<LuceneSearchService> logger)
    {
        _siteService = siteService;
        _luceneIndexManager = luceneIndexManager;
        _luceneIndexingService = luceneIndexingService;
        _luceneAnalyzerManager = luceneAnalyzerManager;
        _luceneIndexSettingsService = luceneIndexSettingsService;
        _luceneSearchQueryService = luceneSearchQueryService;
        _logger = logger;
    }

    public string Name => Key;

    public async Task<SearchResult> SearchAsync(IndexEntity index, string term, int start, int size)
    {
        var result = new SearchResult();

        if (index == null || !_luceneIndexManager.Exists(index.IndexFullName))
        {
            _logger.LogWarning("Lucene: Couldn't execute search. Lucene has not been configured yet.");

            return result;
        }

        var defaultSearchFields = index.As<LuceneDefaultQueryMetadata>()?.DefaultSearchFields ?? [];

        if (defaultSearchFields == null || defaultSearchFields.Length == 0)
        {
            _logger.LogWarning("Lucene: Couldn't execute search. No search provider settings was defined.");

            return result;
        }

        var analyzer = _luceneAnalyzerManager.CreateAnalyzer(await _luceneIndexSettingsService.GetIndexAnalyzerAsync(index.IndexName));
        var queryParser = new MultiFieldQueryParser(LuceneSettings.DefaultVersion, defaultSearchFields, analyzer);

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
