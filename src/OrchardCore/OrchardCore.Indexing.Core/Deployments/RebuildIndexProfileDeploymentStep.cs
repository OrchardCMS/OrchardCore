using OrchardCore.Deployment;
using OrchardCore.Indexing.Core.Recipes;

namespace OrchardCore.Indexing.Core.Deployments;

public sealed class RebuildIndexProfileDeploymentStep : DeploymentStep
{
    public RebuildIndexProfileDeploymentStep()
    {
        Name = RebuildIndexProfileStep.Key;
    }

    public bool IncludeAll { get; set; }

    public string[] Indexes { get; set; }
}
