using OrchardCore.Deployment;
using OrchardCore.Indexing.Core.Recipes;
using OrchardCore.Localization;

namespace OrchardCore.Indexing.Core.Deployments;

public sealed class IndexProfileDeploymentStep : DeploymentStep
{
    private static readonly LocalizationSource s_category = LocalizationSource.Create<IndexProfileDeploymentStep>("Indexing");
    private static readonly LocalizationSource s_title = LocalizationSource.Create<IndexProfileDeploymentStep>("Index Profiles");

    public IndexProfileDeploymentStep()
    {
        Name = CreateOrUpdateIndexProfileStep.StepKey;
        Category = s_category;
        Title = s_title;
    }

    public bool IncludeAll { get; set; }

    public string[] IndexNames { get; set; }
}
