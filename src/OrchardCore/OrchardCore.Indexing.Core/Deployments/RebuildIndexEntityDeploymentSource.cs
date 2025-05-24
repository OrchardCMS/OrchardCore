using System.Text.Json.Nodes;
using OrchardCore.Deployment;
using OrchardCore.Indexing.Core.Recipes;

namespace OrchardCore.Indexing.Core.Deployments;

public sealed class RebuildIndexEntityDeploymentSource
    : DeploymentSourceBase<RebuildIndexEntityDeploymentStep>
{
    protected override Task ProcessAsync(RebuildIndexEntityDeploymentStep step, DeploymentPlanResult result)
    {
        var indicesToRebuild = step.IncludeAll
            ? []
            : step.Indexes;

        result.Steps.Add(new JsonObject
        {
            ["name"] = RebuildIndexEntityStep.Key,
            ["includeAll"] = step.IncludeAll,
            ["Indices"] = JArray.FromObject(indicesToRebuild),
        });

        return Task.CompletedTask;
    }
}
