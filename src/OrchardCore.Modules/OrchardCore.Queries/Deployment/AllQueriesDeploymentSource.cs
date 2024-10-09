using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.Extensions.Options;
using OrchardCore.Deployment;
using OrchardCore.Json;

namespace OrchardCore.Queries.Deployment;

public class AllQueriesDeploymentSource
    : DeploymentSourceBase<AllQueriesDeploymentStep>
{
    private readonly IQueryManager _queryManager;
    private readonly JsonSerializerOptions _jsonSerializerOptions;

    public AllQueriesDeploymentSource(
        IQueryManager queryManager,
        IOptions<DocumentJsonSerializerOptions> jsonSerializerOptions)
    {
        _queryManager = queryManager;
        _jsonSerializerOptions = jsonSerializerOptions.Value.SerializerOptions;
    }

    protected override async Task ProcessAsync(AllQueriesDeploymentStep step, DeploymentPlanResult result)
    {
        var queries = await _queryManager.ListQueriesAsync();

        result.Steps.Add(new JsonObject
        {
            ["name"] = "Queries",
            ["Queries"] = JArray.FromObject(queries, _jsonSerializerOptions),
        });
    }
}
