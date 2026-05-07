using Microsoft.Extensions.Localization;
using OrchardCore.Deployment;
using OrchardCore.Indexing.Core.Recipes;

namespace OrchardCore.Indexing.Core.Deployments;

public sealed class RebuildIndexDeploymentStep : DeploymentStep
{
    public RebuildIndexDeploymentStep()
    {
        Name = RebuildIndexStep.Key;
    }

    public RebuildIndexDeploymentStep(IStringLocalizer<RebuildIndexDeploymentStep> S)
        : this()
    {
        Category = S["Indexing"];
    }

    public bool IncludeAll { get; set; }

    public string[] IndexNames { get; set; }
}
