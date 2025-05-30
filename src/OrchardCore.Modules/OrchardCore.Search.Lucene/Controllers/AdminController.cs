using System.Diagnostics;
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
using OrchardCore.Search.Lucene.Models;
using OrchardCore.Search.Lucene.Services;
using OrchardCore.Search.Lucene.ViewModels;

namespace OrchardCore.Search.Lucene.Controllers;

public sealed class AdminController : Controller
{
    private readonly LuceneIndexManager _luceneIndexManager;
    private readonly IAuthorizationService _authorizationService;
    private readonly LuceneAnalyzerManager _luceneAnalyzerManager;
    private readonly ILuceneQueryService _queryService;
    private readonly ILiquidTemplateManager _liquidTemplateManager;
    private readonly IIndexEntityStore _indexEntityStore;
    private readonly JavaScriptEncoder _javaScriptEncoder;
    private readonly ILogger _logger;
    private readonly IOptions<TemplateOptions> _templateOptions;

    internal readonly IStringLocalizer S;
    internal readonly IHtmlLocalizer H;

    public AdminController(
        IIndexEntityStore indexEntityStore,
        LuceneIndexManager luceneIndexManager,
        IAuthorizationService authorizationService,
        LuceneAnalyzerManager luceneAnalyzerManager,
        ILuceneQueryService queryService,
        ILiquidTemplateManager liquidTemplateManager,
        JavaScriptEncoder javaScriptEncoder,
        IStringLocalizer<AdminController> stringLocalizer,
        IHtmlLocalizer<AdminController> htmlLocalizer,
        ILogger<AdminController> logger,
        IOptions<TemplateOptions> templateOptions)
    {
        _luceneIndexManager = luceneIndexManager;
        _authorizationService = authorizationService;
        _luceneAnalyzerManager = luceneAnalyzerManager;
        _queryService = queryService;
        _liquidTemplateManager = liquidTemplateManager;
        _indexEntityStore = indexEntityStore;
        _javaScriptEncoder = javaScriptEncoder;
        S = stringLocalizer;
        H = htmlLocalizer;
        _logger = logger;
        _templateOptions = templateOptions;
    }

    [Admin("Lucene/{Query}/{id?}", "LuceneQuery")]
    public Task<IActionResult> Query(string id, string query)
    {
        var matchAllQuery = GetMatchAllQuery();

        if (string.IsNullOrEmpty(query))
        {
            query = matchAllQuery;
        }

        query = string.IsNullOrWhiteSpace(query)
            ? string.Empty
            : Base64.FromUTF8Base64String(query);

        return Query(new AdminQueryViewModel
        {
            Id = id,
            MatchAllQuery = matchAllQuery,
            DecodedQuery = query,
        });
    }

    [HttpPost]
    public async Task<IActionResult> Query(AdminQueryViewModel model)
    {
        if (!await _authorizationService.AuthorizeAsync(User, LuceneSearchPermissions.ManageLuceneIndexes))
        {
            return Forbid();
        }

        if (string.IsNullOrEmpty(model.Id))
        {
            return NotFound();
        }

        var index = await _indexEntityStore.FindByIdAsync(model.Id);

        if (index is null)
        {
            return NotFound();
        }

        if (index.ProviderName != LuceneConstants.ProviderName)
        {
            return BadRequest();
        }

        model.Indexes = (await _indexEntityStore.GetAsync(LuceneConstants.ProviderName))
            .Select(x => new SelectListItem(x.DisplayText, x.Id))
            .OrderBy(x => x.Text);

        if (!await _luceneIndexManager.ExistsAsync(index.IndexFullName))
        {
            return NotFound();
        }

        model.MatchAllQuery = GetMatchAllQuery();

        if (string.IsNullOrEmpty(model.Parameters))
        {
            model.Parameters = "{ }";
        }

        if (string.IsNullOrWhiteSpace(model.DecodedQuery))
        {
            return View(model);
        }

        var stopwatch = new Stopwatch();
        stopwatch.Start();

        await _luceneIndexManager.SearchAsync(index, async searcher =>
        {
            var queryMetadata = index.As<LuceneIndexDefaultQueryMetadata>();
            var analyzer = _luceneAnalyzerManager.CreateAnalyzer(index.As<LuceneIndexMetadata>().AnalyzerName);
            var context = new LuceneQueryContext(searcher, queryMetadata.DefaultVersion, analyzer);

            var parameters = JConvert.DeserializeObject<Dictionary<string, object>>(model.Parameters);

            var tokenizedContent = await _liquidTemplateManager.RenderStringAsync(model.DecodedQuery, _javaScriptEncoder, parameters.Select(x => new KeyValuePair<string, FluidValue>(x.Key, FluidValue.Create(x.Value, _templateOptions.Value))));

            try
            {
                var parameterizedQuery = JsonNode.Parse(tokenizedContent, JOptions.Node, JOptions.Document).AsObject();
                var luceneTopDocs = await _queryService.SearchAsync(context, parameterizedQuery);

                if (luceneTopDocs != null)
                {
                    model.Documents = luceneTopDocs.TopDocs.ScoreDocs.Select(hit => searcher.Doc(hit.Doc)).ToList();
                    model.Count = luceneTopDocs.Count;
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error while executing query");
                ModelState.AddModelError(nameof(model.DecodedQuery), S["Invalid query : {0}", e.Message]);
            }

            stopwatch.Stop();
            model.Elapsed = stopwatch.Elapsed;
        });

        return View(model);
    }

    private static string GetMatchAllQuery()
    {
        return Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(
            """
            {
            
              "query": {
                "match_all": { }
              },
              "size": 10
            }
            """));
    }
}
