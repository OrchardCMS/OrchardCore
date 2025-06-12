using System.Text.Json.Nodes;
using OrchardCore.Deployment;
using OrchardCore.Indexing.Core.Recipes;

namespace OrchardCore.Indexing.Core.Deployments;

public sealed class RebuildIndexProfileDeploymentSource
    : DeploymentSourceBase<RebuildIndexProfileDeploymentStep>
{
    protected override Task ProcessAsync(RebuildIndexProfileDeploymentStep step, DeploymentPlanResult result)
    {
        var indicesToRebuild = step.IncludeAll
            ? []
            : step.Indexes;

        result.Steps.Add(new JsonObject
        {
            ["name"] = RebuildIndexStep.Key,
            ["includeAll"] = step.IncludeAll,
            ["Indices"] = JArray.FromObject(indicesToRebuild),
        });

        return Task.CompletedTask;
    }
}
