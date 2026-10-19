using OrchardCore.Deployment;
using OrchardCore.Indexing.Core.Recipes;
using OrchardCore.Localization;

namespace OrchardCore.Indexing.Core.Deployments;

public sealed class RebuildIndexDeploymentStep : DeploymentStep
{
    public RebuildIndexDeploymentStep()
    {
        Name = RebuildIndexStep.Key;
        Category = CommonLocalizationSources.Indexing;
        Title = IndexingLocalizationSources.RebuildIndexes;
    }

    public bool IncludeAll { get; set; }

    public string[] IndexNames { get; set; }
}
