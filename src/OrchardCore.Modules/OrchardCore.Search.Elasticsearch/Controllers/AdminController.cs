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
using OrchardCore.Indexing;
using OrchardCore.Liquid;
using OrchardCore.Search.Elasticsearch.Core.Models;
using OrchardCore.Search.Elasticsearch.Core.Services;
using OrchardCore.Search.Elasticsearch.ViewModels;

namespace OrchardCore.Search.Elasticsearch;

public sealed class AdminController : Controller
{
    private readonly IIndexEntityStore _indexEntityStore;
    private readonly ILiquidTemplateManager _liquidTemplateManager;
    private readonly IAuthorizationService _authorizationService;
    private readonly ElasticsearchIndexManager _elasticIndexManager;
    private readonly JavaScriptEncoder _javaScriptEncoder;
    private readonly ILogger _logger;
    private readonly IOptions<TemplateOptions> _templateOptions;
    private readonly ElasticsearchQueryService _elasticQueryService;
    private readonly ElasticsearchConnectionOptions _elasticConnectionOptions;

    internal readonly IStringLocalizer S;
    internal readonly IHtmlLocalizer H;

    public AdminController(
        IIndexEntityStore indexEntityStore,
        ILiquidTemplateManager liquidTemplateManager,
        IAuthorizationService authorizationService,
        ElasticsearchIndexManager elasticIndexManager,
        JavaScriptEncoder javaScriptEncoder,
        ILogger<AdminController> logger,
        IOptions<TemplateOptions> templateOptions,
        IOptions<ElasticsearchConnectionOptions> elasticConnectionOptions,
        ElasticsearchQueryService elasticQueryService,
        IStringLocalizer<AdminController> stringLocalizer,
        IHtmlLocalizer<AdminController> htmlLocalizer)
    {
        _indexEntityStore = indexEntityStore;
        _liquidTemplateManager = liquidTemplateManager;
        _authorizationService = authorizationService;
        _elasticIndexManager = elasticIndexManager;
        _javaScriptEncoder = javaScriptEncoder;
        _logger = logger;
        _templateOptions = templateOptions;
        _elasticQueryService = elasticQueryService;
        _elasticConnectionOptions = elasticConnectionOptions.Value;
        S = stringLocalizer;
        H = htmlLocalizer;
    }

    [Admin("elasticsearch/indexinfo/{id}", "Elasticsearch.IndexInfo")]
    public async Task<IActionResult> IndexInfo(string id)
    {
        var index = await _indexEntityStore.FindByIdAsync(id);

        if (index is null)
        {
            return NotFound();
        }

        var info = await _elasticIndexManager.GetIndexInfoAsync(index.IndexFullName);

        var formattedJson = JNode.Parse(info).ToJsonString(JOptions.Indented);
        return View(new IndexInfoViewModel
        {
            IndexDisplayText = index.Name,
            Id = id,
            IndexInfo = formattedJson,
        });
    }

    [Admin("Elasticsearch/query/{indexName}", "Elasticsearch.Query")]
    public async Task<IActionResult> QueryIndex(string indexName, string query = null, string parameters = null)
    {
        var index = await _indexEntityStore.FindByNameAsync(indexName);

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

    [Admin("Elasticsearch/query", "Elasticsearch.RunQuery")]
    public async Task<IActionResult> Query()
    {
        if (!await _authorizationService.AuthorizeAsync(User, ElasticsearchPermissions.ManageElasticIndexes))
        {
            return Forbid();
        }

        if (!_elasticConnectionOptions.ConfigurationExists())
        {
            return NotConfigured();
        }

        var model = new AdminQueryViewModel
        {
            MatchAllQuery = GetMatchAllQuery(),
            DecodedQuery = GetDecodedMatchAllQuery(),
            Indexes = (await _indexEntityStore.GetByProviderAsync(ElasticsearchConstants.ProviderName))
                .Select(x => new SelectListItem(x.Name, x.Id))
                .OrderBy(x => x.Text),
        };

        return View(model);
    }

    [HttpPost]
    [Admin("Elasticsearch/query", "Elasticsearch.RunQuery")]
    public async Task<IActionResult> Query(AdminQueryViewModel model)
    {
        if (!await _authorizationService.AuthorizeAsync(User, ElasticsearchPermissions.ManageElasticIndexes))
        {
            return Forbid();
        }

        if (!_elasticConnectionOptions.ConfigurationExists())
        {
            return BadRequest();
        }

        model.MatchAllQuery = GetMatchAllQuery();
        model.Indexes = (await _indexEntityStore.GetByProviderAsync(ElasticsearchConstants.ProviderName))
            .Select(x => new SelectListItem(x.Name, x.Id))
            .OrderBy(x => x.Text);

        if (!ModelState.IsValid)
        {
            return View("Query", model);
        }

        var index = await _indexEntityStore.FindByIdAsync(model.Id);

        if (index is null)
        {
            return NotFound();
        }

        if (index.ProviderName != ElasticsearchConstants.ProviderName)
        {
            return BadRequest();
        }

        if (!await _elasticIndexManager.ExistsAsync(index.IndexFullName))
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

        var parameters = JConvert.DeserializeObject<Dictionary<string, object>>(model.Parameters);
        var tokenizedContent = await _liquidTemplateManager.RenderStringAsync(model.DecodedQuery, _javaScriptEncoder,
            parameters.Select(x => new KeyValuePair<string, FluidValue>(x.Key, FluidValue.Create(x.Value, _templateOptions.Value))));

        try
        {
            if (!string.IsNullOrEmpty(tokenizedContent))
            {
                var results = await _elasticQueryService.SearchAsync(index, tokenizedContent);

                if (results != null)
                {
                    model.Documents = results.TopDocs;
                    model.Fields = results.Fields;
                    model.Count = results.Count;
                }
            }
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error while executing Elasticsearch query");
            ModelState.AddModelError(nameof(model.DecodedQuery), S["Invalid query: {0}", e.Message]);
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

    private ViewResult NotConfigured()
        => View("NotConfigured");
}
