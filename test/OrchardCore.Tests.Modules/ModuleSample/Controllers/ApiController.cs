using System.Collections.Generic;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OrchardCore.Queries;

namespace ModuleSample.Controllers;


[ApiController]
[Authorize(AuthenticationSchemes = "Api"), IgnoreAntiforgeryToken, AllowAnonymous]
public class ApiController : Controller
{
    [HttpPost, HttpGet]
    [Route("api/moduleSample/tests/testCustomQuery")]
    public async Task<IActionResult> TestCustomQueryAsync()
    {
        var result = new CustomQueryResult { Data = new { user = "Tom", age = 5 } };
        return await Task.FromResult(new ObjectResult(result));
    }
}

public class CustomQueryResult : IQueryResults
{
    [JsonPropertyName("items")]
    public IEnumerable<object> Items { get; set; }
    [JsonPropertyName("total")]
    public long? Total { get; set; }
    [JsonPropertyName("data")]
    public object Data { get; set; }
    [JsonPropertyName("success")]
    public bool? Success { get; set; } = true;
    [JsonPropertyName("msg")]
    public string Msg { get; set; }
}
