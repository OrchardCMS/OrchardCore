using OrchardCore.Deployment;
using OrchardCore.Localization;

namespace OrchardCore.Sitemaps.Deployment;

public sealed class AllSitemapsDeploymentStep : DeploymentStep, IDeploymentStep
{
    public override string Name => "AllSitemaps";

    public static LocalizationSource Category => CommonLocalizationSources.ContentManagement;
}
