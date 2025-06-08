using OrchardCore.Deployment;
using OrchardCore.Indexing.Core.Recipes;

namespace OrchardCore.Indexing.Core.Deployments;

public sealed class IndexProfileDeploymentStep : DeploymentStep
{
    public IndexProfileDeploymentStep()
    {
        Name = IndexingProfileStep.StepKey;
    }

    public bool IncludeAll { get; set; }

    public string[] IndexeIds { get; set; }
}
