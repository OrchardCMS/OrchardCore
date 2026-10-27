using System.Text.Json.Serialization;
using OrchardCore.Deployment;
using OrchardCore.Localization;

namespace OrchardCore.Features.Deployment;

/// <summary>
/// Adds enabled and disabled features to a <see cref="DeploymentPlanResult"/>.
/// </summary>
public class AllFeaturesDeploymentStep : DeploymentStep
{
    public override string Name => "AllFeatures";

    [JsonIgnore]
    public override LocalizationSource Category => CommonLocalizationSources.Infrastructure;

    [JsonIgnore]
    public override LocalizationSource Title => FeaturesLocalizationSources.AllFeatures;

    public bool IgnoreDisabledFeatures { get; set; }
}
