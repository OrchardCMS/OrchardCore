using OrchardCore.Deployment;
using OrchardCore.Indexing.Recipes;

namespace OrchardCore.Indexing.Deployments;

internal sealed class IndexEntityDeploymentStep : DeploymentStep
{
    public IndexEntityDeploymentStep()
    {
        Name = IndexEntityStep.StepKey;
    }

    public bool IncludeAll { get; set; }

    public string[] SourceIds { get; set; }
}
