using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OrchardCore.Entities;
using OrchardCore.Queries;
using OrchardCore.Search.Lucene.Model;

namespace OrchardCore.Search.Lucene.Controllers;

[Route("api/lucene")]
[ApiController]
[Authorize(AuthenticationSchemes = "Api")]
[IgnoreAntiforgeryToken]
[AllowAnonymous]
public sealed class LuceneApiController : ControllerBase
{
    private readonly IAuthorizationService _authorizationService;
    private readonly IQueryManager _queryManager;

    public LuceneApiController(
        IAuthorizationService authorizationService,
        IQueryManager queryManager)
    {
        _authorizationService = authorizationService;
        _queryManager = queryManager;
    }

    [HttpGet]
    [Route("content")]
    public async Task<IActionResult> Content([FromQuery] LuceneQueryModel queryModel)
    {
        if (!await _authorizationService.AuthorizeAsync(User, Permissions.QueryLuceneApi))
        {
            return this.ChallengeOrForbid("Api");
        }

        var result = await LuceneQueryApiAsync(queryModel, returnContentItems: true);

        return new ObjectResult(result);
    }

    [HttpPost]
    [Route("content")]
    public async Task<IActionResult> ContentPost(LuceneQueryModel queryModel)
    {
        if (!await _authorizationService.AuthorizeAsync(User, Permissions.QueryLuceneApi))
        {
            return this.ChallengeOrForbid();
        }

        var result = await LuceneQueryApiAsync(queryModel, returnContentItems: true);

        return new ObjectResult(result);
    }

    [HttpGet]
    [Route("documents")]
    public async Task<IActionResult> Documents([FromQuery] LuceneQueryModel queryModel)
    {
        if (!await _authorizationService.AuthorizeAsync(User, Permissions.QueryLuceneApi))
        {
            return this.ChallengeOrForbid();
        }

        var result = await LuceneQueryApiAsync(queryModel);

        return new ObjectResult(result);
    }

    [HttpPost]
    [Route("documents")]
    public async Task<IActionResult> DocumentsPost(LuceneQueryModel queryModel)
    {
        if (!await _authorizationService.AuthorizeAsync(User, Permissions.QueryLuceneApi))
        {
            return this.ChallengeOrForbid("Api");
        }

        var result = await LuceneQueryApiAsync(queryModel);

        return new ObjectResult(result);
    }

    private async Task<IQueryResults> LuceneQueryApiAsync(LuceneQueryModel queryModel, bool returnContentItems = false)
    {
        var luceneQuery = await _queryManager.NewAsync(LuceneQuerySource.SourceName);
        luceneQuery.ReturnContentItems = returnContentItems;

        luceneQuery.Put(new LuceneQueryMetadata()
        {
            Index = queryModel.IndexName,
            Template = queryModel.Query,
        });

        var queryParameters = queryModel.Parameters != null
            ? JConvert.DeserializeObject<Dictionary<string, object>>(queryModel.Parameters)
            : [];

        return await _queryManager.ExecuteQueryAsync(luceneQuery, queryParameters);
    }
}
