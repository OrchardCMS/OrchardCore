using OrchardCore.Deployment;
using OrchardCore.Indexing.Core.Recipes;
using OrchardCore.Localization;

namespace OrchardCore.Indexing.Core.Deployments;

public sealed class RebuildIndexDeploymentStep : DeploymentStep
{
    private static readonly LocalizationSource s_category = new("Indexing", typeof(RebuildIndexDeploymentStep));
    private static readonly LocalizationSource s_title = new("Rebuild Indexes", typeof(RebuildIndexDeploymentStep));

    public RebuildIndexDeploymentStep()
    {
        Name = RebuildIndexStep.Key;
        Category = s_category;
        Title = s_title;
    }

    public bool IncludeAll { get; set; }

    public string[] IndexNames { get; set; }
}
