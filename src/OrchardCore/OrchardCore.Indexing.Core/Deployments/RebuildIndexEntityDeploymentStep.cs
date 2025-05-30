using OrchardCore.Deployment;
using OrchardCore.Indexing.Core.Recipes;

namespace OrchardCore.Indexing.Core.Deployments;

public sealed class RebuildIndexEntityDeploymentStep : DeploymentStep
{
    public RebuildIndexEntityDeploymentStep()
    {
        Name = RebuildIndexEntityStep.Key;
    }

    public bool IncludeAll { get; set; }

    public string[] Indexes { get; set; }
}
