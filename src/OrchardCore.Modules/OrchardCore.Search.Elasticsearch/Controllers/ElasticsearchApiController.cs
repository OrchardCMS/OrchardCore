using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OrchardCore.Entities;
using OrchardCore.Queries;
using OrchardCore.Search.Elasticsearch.Core.Services;
using OrchardCore.Search.Elasticsearch.Models;
using OrchardCore.Search.Elasticsearch.ViewModels;

namespace OrchardCore.Search.Elasticsearch;

[Route("api/elasticsearch")]
[ApiController]
[Authorize(AuthenticationSchemes = "Api"), IgnoreAntiforgeryToken, AllowAnonymous]
public sealed class ElasticsearchApiController : ControllerBase
{
    private readonly IAuthorizationService _authorizationService;
    private readonly IQueryManager _queryManager;

    public ElasticsearchApiController(
        IAuthorizationService authorizationService,
        IQueryManager queryManager)
    {
        _authorizationService = authorizationService;
        _queryManager = queryManager;
    }

    [HttpGet]
    [Route("content")]
    public async Task<IActionResult> Content([FromQuery] ElasticApiQueryViewModel queryModel)
    {
        if (!await _authorizationService.AuthorizeAsync(User, Permissions.QueryElasticApi))
        {
            return this.ChallengeOrForbid("Api");
        }

        var result = await ElasticQueryApiAsync(queryModel, returnContentItems: true);

        return new ObjectResult(result);
    }

    [HttpPost]
    [Route("content")]
    public async Task<IActionResult> ContentPost(ElasticApiQueryViewModel queryModel)
    {
        if (!await _authorizationService.AuthorizeAsync(User, Permissions.QueryElasticApi))
        {
            return this.ChallengeOrForbid();
        }

        var result = await ElasticQueryApiAsync(queryModel, returnContentItems: true);

        return new ObjectResult(result);
    }

    [HttpGet]
    [Route("documents")]
    public async Task<IActionResult> Documents([FromQuery] ElasticApiQueryViewModel queryModel)
    {
        if (!await _authorizationService.AuthorizeAsync(User, Permissions.QueryElasticApi))
        {
            return this.ChallengeOrForbid();
        }

        var result = await ElasticQueryApiAsync(queryModel);

        return new ObjectResult(result);
    }

    [HttpPost]
    [Route("documents")]
    public async Task<IActionResult> DocumentsPost(ElasticApiQueryViewModel queryModel)
    {
        if (!await _authorizationService.AuthorizeAsync(User, Permissions.QueryElasticApi))
        {
            return this.ChallengeOrForbid("Api");
        }

        var result = await ElasticQueryApiAsync(queryModel);

        return new ObjectResult(result);
    }

    private async Task<IQueryResults> ElasticQueryApiAsync(ElasticApiQueryViewModel queryModel, bool returnContentItems = false)
    {
        var elasticQuery = await _queryManager.NewAsync(ElasticQuerySource.SourceName);
        elasticQuery.ReturnContentItems = returnContentItems;

        elasticQuery.Put(new ElasticsearchQueryMetadata
        {
            Index = queryModel.IndexName,
            Template = queryModel.Query,
        });

        var queryParameters = queryModel.Parameters != null
            ? JConvert.DeserializeObject<Dictionary<string, object>>(queryModel.Parameters)
            : [];

        var result = await _queryManager.ExecuteQueryAsync(elasticQuery, queryParameters);

        return result;
    }
}
