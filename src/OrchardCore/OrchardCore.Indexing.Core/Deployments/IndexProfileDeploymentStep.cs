using Microsoft.Extensions.Localization;
using OrchardCore.Deployment;
using OrchardCore.Indexing.Core.Recipes;

namespace OrchardCore.Indexing.Core.Deployments;

public sealed class IndexProfileDeploymentStep : DeploymentStep
{
    public IndexProfileDeploymentStep()
    {
        Name = CreateOrUpdateIndexProfileStep.StepKey;
        Category = LocalizedString.Create("Indexing");
        Title = LocalizedString.Create("Index Profiles");
    }

    public bool IncludeAll { get; set; }

    public string[] IndexNames { get; set; }
}
