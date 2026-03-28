using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OrchardCore.Entities;
using OrchardCore.Queries;
using OrchardCore.Search.OpenSearch.Core.Services;
using OrchardCore.Search.OpenSearch.Models;
using OrchardCore.Search.OpenSearch.ViewModels;

namespace OrchardCore.Search.OpenSearch;

[Route("api/opensearch")]
[ApiController]
[Authorize(AuthenticationSchemes = "Api"), IgnoreAntiforgeryToken, AllowAnonymous]
public sealed class OpenSearchApiController : ControllerBase
{
    private readonly IAuthorizationService _authorizationService;
    private readonly IQueryManager _queryManager;

    public OpenSearchApiController(
        IAuthorizationService authorizationService,
        IQueryManager queryManager)
    {
        _authorizationService = authorizationService;
        _queryManager = queryManager;
    }

    [HttpGet]
    [Route("content")]
    public async Task<IActionResult> Content([FromQuery] OpenSearchApiQueryViewModel queryModel)
    {
        if (!await _authorizationService.AuthorizeAsync(User, OpenSearchPermissions.QueryOpenSearchApi))
        {
            return this.ChallengeOrForbid("Api");
        }

        var result = await OpenSearchQueryApiAsync(queryModel, returnContentItems: true);

        return new ObjectResult(result);
    }

    [HttpPost]
    [Route("content")]
    public async Task<IActionResult> ContentPost(OpenSearchApiQueryViewModel queryModel)
    {
        if (!await _authorizationService.AuthorizeAsync(User, OpenSearchPermissions.QueryOpenSearchApi))
        {
            return this.ChallengeOrForbid();
        }

        var result = await OpenSearchQueryApiAsync(queryModel, returnContentItems: true);

        return new ObjectResult(result);
    }

    [HttpGet]
    [Route("documents")]
    public async Task<IActionResult> Documents([FromQuery] OpenSearchApiQueryViewModel queryModel)
    {
        if (!await _authorizationService.AuthorizeAsync(User, OpenSearchPermissions.QueryOpenSearchApi))
        {
            return this.ChallengeOrForbid();
        }

        var result = await OpenSearchQueryApiAsync(queryModel);

        return new ObjectResult(result);
    }

    [HttpPost]
    [Route("documents")]
    public async Task<IActionResult> DocumentsPost(OpenSearchApiQueryViewModel queryModel)
    {
        if (!await _authorizationService.AuthorizeAsync(User, OpenSearchPermissions.QueryOpenSearchApi))
        {
            return this.ChallengeOrForbid("Api");
        }

        var result = await OpenSearchQueryApiAsync(queryModel);

        return new ObjectResult(result);
    }

    private async Task<IQueryResults> OpenSearchQueryApiAsync(OpenSearchApiQueryViewModel queryModel, bool returnContentItems = false)
    {
        var openSearchQuery = await _queryManager.NewAsync(OpenSearchQuerySource.SourceName);
        openSearchQuery.ReturnContentItems = returnContentItems;

        openSearchQuery.Put(new OpenSearchQueryMetadata
        {
            Index = queryModel.IndexName,
            Template = queryModel.Query,
        });

        var queryParameters = queryModel.Parameters != null
            ? JConvert.DeserializeObject<Dictionary<string, object>>(queryModel.Parameters)
            : [];

        var result = await _queryManager.ExecuteQueryAsync(openSearchQuery, queryParameters);

        return result;
    }
}
