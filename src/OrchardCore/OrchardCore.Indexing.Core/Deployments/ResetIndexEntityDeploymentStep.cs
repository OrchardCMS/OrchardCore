using OrchardCore.Deployment;
using OrchardCore.Indexing.Core.Recipes;

namespace OrchardCore.Indexing.Core.Deployments;

public sealed class ResetIndexEntityDeploymentStep : DeploymentStep
{
    public ResetIndexEntityDeploymentStep()
    {
        Name = ResetIndexEntityStep.Key;
    }

    public bool IncludeAll { get; set; }

    public string[] Indexes { get; set; }
}
