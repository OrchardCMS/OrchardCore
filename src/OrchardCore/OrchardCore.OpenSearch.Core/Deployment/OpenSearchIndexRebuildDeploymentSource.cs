using System.Text.Json.Nodes;
using OrchardCore.Deployment;

namespace OrchardCore.OpenSearch.Core.Deployment;

public sealed class OpenSearchIndexRebuildDeploymentSource
    : DeploymentSourceBase<OpenSearchIndexRebuildDeploymentStep>
{
    protected override Task ProcessAsync(OpenSearchIndexRebuildDeploymentStep step, DeploymentPlanResult result)
    {
        var indicesToRebuild = step.IncludeAll ? [] : step.Indices;

        result.Steps.Add(new JsonObject
        {
            ["name"] = "opensearch-index-rebuild",
            ["includeAll"] = step.IncludeAll,
            ["Indices"] = JArray.FromObject(indicesToRebuild),
        });

        return Task.CompletedTask;
    }
}
