using OrchardCore.Deployment;
using OrchardCore.Indexing.Core.Recipes;
using OrchardCore.Localization;

namespace OrchardCore.Indexing.Core.Deployments;

public sealed class ResetIndexDeploymentStep : DeploymentStep
{
    private static readonly LocalizationSource s_category = new("Indexing", typeof(ResetIndexDeploymentStep));
    private static readonly LocalizationSource s_title = new("Reset Indexes", typeof(ResetIndexDeploymentStep));

    public ResetIndexDeploymentStep()
    {
        Name = ResetIndexStep.Key;
        Category = s_category;
        Title = s_title;
    }

    public bool IncludeAll { get; set; }

    public string[] IndexNames { get; set; }
}
