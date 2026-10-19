using OrchardCore.Deployment;
using OrchardCore.Localization;

namespace OrchardCore.Sitemaps.Deployment;

public sealed class AllSitemapsDeploymentStep : DeploymentStep
{
    public AllSitemapsDeploymentStep()
    {
        Name = "AllSitemaps";
        Category = CommonLocalizationSources.ContentManagement;
    }
}
