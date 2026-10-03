using OrchardCore.Deployment;
using OrchardCore.Localization;

namespace OrchardCore.Sitemaps.Deployment;

public sealed class AllSitemapsDeploymentStep : DeploymentStep
{
    private static readonly LocalizationSource s_category = new("Content Management", typeof(AllSitemapsDeploymentStep));

    public AllSitemapsDeploymentStep()
    {
        Name = "AllSitemaps";
        Category = s_category;
    }
}
