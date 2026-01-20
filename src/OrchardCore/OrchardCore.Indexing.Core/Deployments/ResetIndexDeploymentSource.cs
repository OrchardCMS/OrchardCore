using System.Text.Json.Nodes;
using OrchardCore.Deployment;
using OrchardCore.Indexing.Core.Recipes;

namespace OrchardCore.Indexing.Core.Deployments;

public sealed class ResetIndexDeploymentSource
    : DeploymentSourceBase<ResetIndexDeploymentStep>
{
    protected override Task ProcessAsync(ResetIndexDeploymentStep step, DeploymentPlanResult result)
    {
        var indicesToReset = step.IncludeAll
            ? []
            : step.IndexNames ?? [];

        result.Steps.Add(new JsonObject
        {
            ["name"] = ResetIndexStep.Key,
            ["includeAll"] = step.IncludeAll,
            ["indexNames"] = JArray.FromObject(indicesToReset),
        });

        return Task.CompletedTask;
    }
}
