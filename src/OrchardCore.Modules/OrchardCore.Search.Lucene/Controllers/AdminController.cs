using System.Diagnostics;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Nodes;
using Fluid;
using Fluid.Values;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OrchardCore.Admin;
using OrchardCore.Entities;
using OrchardCore.Indexing;
using OrchardCore.Liquid;
using OrchardCore.Lucene.Core;
using OrchardCore.Search.Lucene.Models;
using OrchardCore.Search.Lucene.Services;
using OrchardCore.Search.Lucene.ViewModels;

namespace OrchardCore.Search.Lucene.Controllers;

public sealed class AdminController : Controller
{
    private readonly LuceneIndexManager _indexManager;
    private readonly ILuceneIndexStore _indexStore;
    private readonly IAuthorizationService _authorizationService;
    private readonly LuceneAnalyzerManager _analyzerManager;
    private readonly ILuceneQueryService _queryService;
    private readonly ILiquidTemplateManager _liquidTemplateManager;
    private readonly IIndexProfileStore _indexProfileStore;
    private readonly JavaScriptEncoder _javaScriptEncoder;
    private readonly ILogger _logger;
    private readonly IOptions<TemplateOptions> _templateOptions;

    internal readonly IStringLocalizer S;
    internal readonly IHtmlLocalizer H;

    public AdminController(
        IIndexProfileStore indexProfileStore,
        LuceneIndexManager indexManager,
        ILuceneIndexStore indexStore,
        IAuthorizationService authorizationService,
        LuceneAnalyzerManager analyzerManager,
        ILuceneQueryService queryService,
        ILiquidTemplateManager liquidTemplateManager,
        JavaScriptEncoder javaScriptEncoder,
        IStringLocalizer<AdminController> stringLocalizer,
        IHtmlLocalizer<AdminController> htmlLocalizer,
        ILogger<AdminController> logger,
        IOptions<TemplateOptions> templateOptions)
    {
        _indexManager = indexManager;
        _indexStore = indexStore;
        _authorizationService = authorizationService;
        _analyzerManager = analyzerManager;
        _queryService = queryService;
        _liquidTemplateManager = liquidTemplateManager;
        _indexProfileStore = indexProfileStore;
        _javaScriptEncoder = javaScriptEncoder;
        S = stringLocalizer;
        H = htmlLocalizer;
        _logger = logger;
        _templateOptions = templateOptions;
    }

    [Admin("Lucene/query/{indexName}", "LuceneQuery")]
    public async Task<IActionResult> QueryIndex(string indexName, string query = null, string parameters = null)
    {
        var index = await _indexProfileStore.FindByNameAsync(indexName);

        if (index is null)
        {
            return NotFound();
        }

        return await Query(new AdminQueryViewModel()
        {
            Id = index.Id,
            DecodedQuery = string.IsNullOrWhiteSpace(query)
                ? null
                : Base64.FromUTF8Base64String(query),
            Parameters = parameters,
        });
    }

    [Admin("Lucene/query/{id?}", "LuceneQuery")]
    public async Task<IActionResult> Query()
    {
        if (!await _authorizationService.AuthorizeAsync(User, LuceneSearchPermissions.ManageLuceneIndexes))
        {
            return Forbid();
        }

        var model = new AdminQueryViewModel
        {
            MatchAllQuery = GetMatchAllQuery(),
            DecodedQuery = GetDecodedMatchAllQuery(),
            Indexes = (await _indexProfileStore.GetByProviderAsync(LuceneConstants.ProviderName))
                .Select(x => new SelectListItem(x.Name, x.Id))
                .OrderBy(x => x.Text),
        };

        // Keep the view name here since QueryIndex calls this method directly.
        // Otherwise the view name would be "QueryIndex" instead of "Query" which will be incorrect.
        return View("Query", model);
    }

    [HttpPost]
    [Admin("Lucene/query/{id?}", "LuceneQuery")]
    public async Task<IActionResult> Query(AdminQueryViewModel model)
    {
        if (!await _authorizationService.AuthorizeAsync(User, LuceneSearchPermissions.ManageLuceneIndexes))
        {
            return Forbid();
        }

        model.MatchAllQuery = GetMatchAllQuery();
        model.Indexes = (await _indexProfileStore.GetByProviderAsync(LuceneConstants.ProviderName))
            .Select(x => new SelectListItem(x.Name, x.Id))
            .OrderBy(x => x.Text);

        if (!ModelState.IsValid)
        {
            return View("Query", model);
        }

        var index = await _indexProfileStore.FindByIdAsync(model.Id);

        if (index is null)
        {
            return NotFound();
        }

        if (index.ProviderName != LuceneConstants.ProviderName)
        {
            return BadRequest();
        }

        if (!await _indexManager.ExistsAsync(index.IndexFullName))
        {
            return NotFound();
        }

        if (string.IsNullOrEmpty(model.Parameters))
        {
            model.Parameters = "{ }";
        }

        if (string.IsNullOrEmpty(model.DecodedQuery))
        {
            model.DecodedQuery = GetDecodedMatchAllQuery();
        }

        var stopwatch = new Stopwatch();
        stopwatch.Start();

        try
        {
            await _indexStore.SearchAsync(index, async searcher =>
            {
                var queryMetadata = index.As<LuceneIndexDefaultQueryMetadata>();
                var analyzer = _analyzerManager.CreateAnalyzer(index.As<LuceneIndexMetadata>().AnalyzerName);
                var context = new LuceneQueryContext(searcher, queryMetadata.DefaultVersion, analyzer);

                var parameters = JConvert.DeserializeObject<Dictionary<string, object>>(model.Parameters);

                var tokenizedContent = await _liquidTemplateManager.RenderStringAsync(model.DecodedQuery, _javaScriptEncoder, parameters.Select(x => new KeyValuePair<string, FluidValue>(x.Key, FluidValue.Create(x.Value, _templateOptions.Value))));

                if (!string.IsNullOrEmpty(tokenizedContent))
                {
                    var parameterizedQuery = JsonNode.Parse(tokenizedContent, JOptions.Node, JOptions.Document).AsObject();
                    var topDocs = await _queryService.SearchAsync(context, parameterizedQuery);

                    if (topDocs != null)
                    {
                        model.Documents = topDocs.TopDocs.ScoreDocs.Select(hit => searcher.Doc(hit.Doc)).ToList();
                        model.Count = topDocs.Count;
                    }
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while executing Lucene query");
            ModelState.AddModelError(nameof(model.DecodedQuery), S["Invalid query: {0}", ex.Message]);
        }

        stopwatch.Stop();
        model.Elapsed = stopwatch.Elapsed;

        // Keep the view name here since QueryIndex calls this method directly.
        // Otherwise the view name would be "QueryIndex" instead of "Query" which will be incorrect.
        return View("Query", model);
    }

    private static string GetMatchAllQuery()
    {
        return Convert.ToBase64String(Encoding.UTF8.GetBytes(GetDecodedMatchAllQuery()));
    }

    private static string GetDecodedMatchAllQuery()
    {
        return
            """
            {
              "query": {
                "match_all": { }
              },
              "size": 10
            }
            """;
    }
}
