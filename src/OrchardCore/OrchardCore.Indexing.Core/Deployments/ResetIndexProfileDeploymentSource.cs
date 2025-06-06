using System.Text.Json.Nodes;
using OrchardCore.Deployment;
using OrchardCore.Indexing.Core.Recipes;

namespace OrchardCore.Indexing.Core.Deployments;

public sealed class ResetIndexProfileDeploymentSource
    : DeploymentSourceBase<ResetIndexProfileDeploymentStep>
{
    protected override Task ProcessAsync(ResetIndexProfileDeploymentStep step, DeploymentPlanResult result)
    {
        var indicesToReset = step.IncludeAll
            ? []
            : step.Indexes;

        result.Steps.Add(new JsonObject
        {
            ["name"] = ResetIndexProfileStep.Key,
            ["includeAll"] = step.IncludeAll,
            ["Indices"] = JArray.FromObject(indicesToReset),
        });

        return Task.CompletedTask;
    }
}
