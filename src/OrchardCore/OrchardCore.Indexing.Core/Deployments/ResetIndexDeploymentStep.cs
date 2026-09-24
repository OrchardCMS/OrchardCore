using Microsoft.Extensions.Localization;
using OrchardCore.Deployment;
using OrchardCore.Indexing.Core.Recipes;

namespace OrchardCore.Indexing.Core.Deployments;

public sealed class ResetIndexDeploymentStep : DeploymentStep
{
    public ResetIndexDeploymentStep()
    {
        Name = ResetIndexStep.Key;
        Category = LocalizedString.Create("Indexing");
        Title = LocalizedString.Create("Reset Indexes");
    }

    public bool IncludeAll { get; set; }

    public string[] IndexNames { get; set; }
}
