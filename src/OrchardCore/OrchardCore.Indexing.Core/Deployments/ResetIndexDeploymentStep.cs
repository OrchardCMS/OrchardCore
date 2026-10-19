using OrchardCore.Deployment;
using OrchardCore.Indexing.Core.Recipes;
using OrchardCore.Localization;

namespace OrchardCore.Indexing.Core.Deployments;

public sealed class ResetIndexDeploymentStep : DeploymentStep
{
    public ResetIndexDeploymentStep()
    {
        Name = ResetIndexStep.Key;
        Category = CommonLocalizationSources.Indexing;
        Title = IndexingLocalizationSources.ResetIndexes;
    }

    public bool IncludeAll { get; set; }

    public string[] IndexNames { get; set; }
}
