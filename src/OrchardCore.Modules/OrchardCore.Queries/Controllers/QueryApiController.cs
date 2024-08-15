using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace OrchardCore.Queries.Controllers;

[Route("api/queries")]
[ApiController]
[Authorize(AuthenticationSchemes = "Api")]
[IgnoreAntiforgeryToken]
[AllowAnonymous]
public sealed class QueryApiController : ControllerBase
{
    private readonly IAuthorizationService _authorizationService;
    private readonly IQueryManager _queryManager;

    public QueryApiController(
        IAuthorizationService authorizationService,
        IQueryManager queryManager
        )
    {
        _authorizationService = authorizationService;
        _queryManager = queryManager;
    }

    [HttpPost, HttpGet]
    [Route("{name}")]
    public async Task<IActionResult> Query(
        string name,
        string parameters)
    {
        var query = await _queryManager.GetQueryAsync(name);

        if (query == null)
        {
            return NotFound();
        }

        if (!await _authorizationService.AuthorizeAsync(User, Permissions.CreatePermissionForQuery(query.Name)))
        {
            // Intentionally not returning Unauthorized as it is not usable from APIs and would
            // expose the existence of a named query (not a concern per se).
            return NotFound();
        }

        if (Request.Method == HttpMethods.Post && string.IsNullOrEmpty(parameters))
        {
            using var reader = new StreamReader(Request.Body, Encoding.UTF8);
            parameters = await reader.ReadToEndAsync();
        }

        var queryParameters = !string.IsNullOrEmpty(parameters) ?
            JConvert.DeserializeObject<Dictionary<string, object>>(parameters)
            : [];

        var result = await _queryManager.ExecuteQueryAsync(query, queryParameters);

        return new ObjectResult(result);
    }
}
