using OrchardCore.Deployment;
using OrchardCore.Indexing.Core.Recipes;
using OrchardCore.Localization;

namespace OrchardCore.Indexing.Core.Deployments;

public sealed class IndexProfileDeploymentStep : DeploymentStep
{
    public IndexProfileDeploymentStep()
    {
        Name = CreateOrUpdateIndexProfileStep.StepKey;
        Category = CommonLocalizationSources.Indexing;
        Title = IndexingLocalizationSources.IndexProfiles;
    }

    public bool IncludeAll { get; set; }

    public string[] IndexNames { get; set; }
}
