using System.Text.Json.Nodes;
using OrchardCore.Deployment;
using OrchardCore.Indexing.Core.Recipes;

namespace OrchardCore.Indexing.Core.Deployments;

public sealed class ResetIndexEntityDeploymentSource
    : DeploymentSourceBase<ResetIndexEntityDeploymentStep>
{
    protected override Task ProcessAsync(ResetIndexEntityDeploymentStep step, DeploymentPlanResult result)
    {
        var indicesToReset = step.IncludeAll
            ? []
            : step.Indexes;

        result.Steps.Add(new JsonObject
        {
            ["name"] = ResetIndexEntityStep.Key,
            ["includeAll"] = step.IncludeAll,
            ["Indices"] = JArray.FromObject(indicesToReset),
        });

        return Task.CompletedTask;
    }
}
