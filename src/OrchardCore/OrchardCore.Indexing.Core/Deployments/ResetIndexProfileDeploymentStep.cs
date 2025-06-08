using OrchardCore.Deployment;
using OrchardCore.Indexing.Core.Recipes;

namespace OrchardCore.Indexing.Core.Deployments;

public sealed class ResetIndexProfileDeploymentStep : DeploymentStep
{
    public ResetIndexProfileDeploymentStep()
    {
        Name = ResetIndexProfileStep.Key;
    }

    public bool IncludeAll { get; set; }

    public string[] Indexes { get; set; }
}
