using OrchardCore.Deployment;
using OrchardCore.Localization;

namespace OrchardCore.Features.Deployment;

/// <summary>
/// Adds enabled and disabled features to a <see cref="DeploymentPlanResult"/>.
/// </summary>
public class AllFeaturesDeploymentStep : DeploymentStep
{
    public AllFeaturesDeploymentStep()
    {
        Name = "AllFeatures";
        Category = CommonLocalizationSources.Infrastructure;
        Title = FeaturesLocalizationSources.AllFeatures;
    }

    public bool IgnoreDisabledFeatures { get; set; }
}
