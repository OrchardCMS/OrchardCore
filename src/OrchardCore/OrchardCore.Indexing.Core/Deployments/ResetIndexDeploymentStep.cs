using OrchardCore.Deployment;
using OrchardCore.Indexing.Core.Recipes;

namespace OrchardCore.Indexing.Core.Deployments;

public sealed class ResetIndexDeploymentStep : DeploymentStep
{
    public ResetIndexDeploymentStep()
    {
        Name = ResetIndexStep.Key;
    }

    public bool IncludeAll { get; set; }

    public string[] IndexNames { get; set; }
}
