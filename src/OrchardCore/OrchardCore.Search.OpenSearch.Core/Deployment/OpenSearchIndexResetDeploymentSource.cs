using System.Text.Json.Nodes;
using OrchardCore.Deployment;

namespace OrchardCore.Search.OpenSearch.Core.Deployment;

public sealed class OpenSearchIndexResetDeploymentSource
    : DeploymentSourceBase<OpenSearchIndexResetDeploymentStep>
{
    protected override Task ProcessAsync(OpenSearchIndexResetDeploymentStep step, DeploymentPlanResult result)
    {
        var indicesToReset = step.IncludeAll ? [] : step.Indices;

        result.Steps.Add(new JsonObject
        {
            ["name"] = "opensearch-index-reset",
            ["includeAll"] = step.IncludeAll,
            ["Indices"] = JArray.FromObject(indicesToReset),
        });

        return Task.CompletedTask;
    }
}
