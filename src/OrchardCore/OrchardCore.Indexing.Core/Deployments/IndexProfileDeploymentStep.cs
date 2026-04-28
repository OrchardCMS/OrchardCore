using Microsoft.Extensions.Localization;
using OrchardCore.Deployment;
using OrchardCore.Indexing.Core.Recipes;

namespace OrchardCore.Indexing.Core.Deployments;

public sealed class IndexProfileDeploymentStep : DeploymentStep
{
    public IndexProfileDeploymentStep()
    {
        Name = CreateOrUpdateIndexProfileStep.StepKey;
    }

    public IndexProfileDeploymentStep(IStringLocalizer<IndexProfileDeploymentStep> S)
        : this()
    {
        Category = S["Indexing"];
    }

    public bool IncludeAll { get; set; }

    public string[] IndexNames { get; set; }
}
