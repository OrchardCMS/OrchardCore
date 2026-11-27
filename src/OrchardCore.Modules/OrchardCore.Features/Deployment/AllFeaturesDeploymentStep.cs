using OrchardCore.Deployment;
using OrchardCore.Localization;

namespace OrchardCore.Features.Deployment;

/// <summary>
/// Adds enabled and disabled features to a <see cref="DeploymentPlanResult"/>.
/// </summary>
public class AllFeaturesDeploymentStep : DeploymentStep, IDeploymentStep
{
    public override string Name => "AllFeatures";

    public static LocalizationSource Category => CommonLocalizationSources.Infrastructure;

    public static LocalizationSource Title => FeaturesLocalizationSources.AllFeatures;

    public bool IgnoreDisabledFeatures { get; set; }
}
