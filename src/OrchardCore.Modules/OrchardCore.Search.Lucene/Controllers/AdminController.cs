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
    private readonly LuceneIndexManager _indexManager;
    private readonly IAuthorizationService _authorizationService;
    private readonly LuceneAnalyzerManager _analyzerManager;
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
        LuceneIndexManager indexManager,
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
        _authorizationService = authorizationService;
        _analyzerManager = analyzerManager;
        _queryService = queryService;
        _liquidTemplateManager = liquidTemplateManager;
        _indexEntityStore = indexEntityStore;
        _javaScriptEncoder = javaScriptEncoder;
        S = stringLocalizer;
        H = htmlLocalizer;
        _logger = logger;
        _templateOptions = templateOptions;
    }

    [Admin("Lucene/Query/{id?}", "LuceneQuery")]
    public async Task<IActionResult> Query(string id, string query = null)
    {
        if (!await _authorizationService.AuthorizeAsync(User, LuceneSearchPermissions.ManageLuceneIndexes))
        {
            return Forbid();
        }

        var matchAllQuery = GetMatchAllQuery();

        if (string.IsNullOrEmpty(query))
        {
            query = matchAllQuery;
        }

        var model = new AdminQueryViewModel
        {
            Id = id,
            MatchAllQuery = matchAllQuery,
            DecodedQuery = string.IsNullOrWhiteSpace(query)
                ? string.Empty
                : Base64.FromUTF8Base64String(query),
        };

        if (!string.IsNullOrEmpty(id))
        {
            return await Query(model);
        }

        model.Indexes = (await _indexEntityStore.GetAsync(LuceneConstants.ProviderName))
            .Select(x => new SelectListItem(x.DisplayText, x.Id))
            .OrderBy(x => x.Text);

        return View(model);
    }

    [HttpPost]
    [Admin("Lucene/Query/{id?}", "LuceneQuery")]
    public async Task<IActionResult> Query(AdminQueryViewModel model)
    {
        if (!await _authorizationService.AuthorizeAsync(User, LuceneSearchPermissions.ManageLuceneIndexes))
        {
            return Forbid();
        }

        model.MatchAllQuery = GetMatchAllQuery();
        model.Indexes = (await _indexEntityStore.GetAsync(LuceneConstants.ProviderName))
            .Select(x => new SelectListItem(x.DisplayText, x.Id))
            .OrderBy(x => x.Text);

        if (!ModelState.IsValid)
        {
            return View(model);
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

        if (!await _indexManager.ExistsAsync(index.IndexFullName))
        {
            return NotFound();
        }

        if (string.IsNullOrEmpty(model.Parameters))
        {
            model.Parameters = "{ }";
        }

        var stopwatch = new Stopwatch();
        stopwatch.Start();

        await _indexManager.SearchAsync(index, async searcher =>
        {
            var queryMetadata = index.As<LuceneIndexDefaultQueryMetadata>();
            var analyzer = _analyzerManager.CreateAnalyzer(index.As<LuceneIndexMetadata>().AnalyzerName);
            var context = new LuceneQueryContext(searcher, queryMetadata.DefaultVersion, analyzer);

            var parameters = JConvert.DeserializeObject<Dictionary<string, object>>(model.Parameters);

            var tokenizedContent = await _liquidTemplateManager.RenderStringAsync(model.DecodedQuery, _javaScriptEncoder, parameters.Select(x => new KeyValuePair<string, FluidValue>(x.Key, FluidValue.Create(x.Value, _templateOptions.Value))));

            try
            {
                var parameterizedQuery = JsonNode.Parse(tokenizedContent, JOptions.Node, JOptions.Document).AsObject();
                var topDocs = await _queryService.SearchAsync(context, parameterizedQuery);

                if (topDocs != null)
                {
                    model.Documents = topDocs.TopDocs.ScoreDocs.Select(hit => searcher.Doc(hit.Doc)).ToList();
                    model.Count = topDocs.Count;
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error while executing Lucene query");
                ModelState.AddModelError(nameof(model.DecodedQuery), S["Invalid query: {0}", e.Message]);
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
