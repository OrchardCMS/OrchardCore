using OrchardCore.Deployment;
using OrchardCore.Indexing.Core.Recipes;

namespace OrchardCore.Indexing.Core.Deployments;

public sealed class IndexEntityDeploymentStep : DeploymentStep
{
    public IndexEntityDeploymentStep()
    {
        Name = IndexEntityStep.StepKey;
    }

    public bool IncludeAll { get; set; }

    public string[] Indexes { get; set; }
}
