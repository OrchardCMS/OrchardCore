using OrchardCore.Deployment;
using OrchardCore.Localization;

namespace OrchardCore.Features.Deployment;

/// <summary>
/// Adds enabled and disabled features to a <see cref="DeploymentPlanResult"/>.
/// </summary>
public class AllFeaturesDeploymentStep : DeploymentStep
{
    private static readonly LocalizationSource s_category = LocalizationSource.Create<AllFeaturesDeploymentStep>("Infrastructure");
    private static readonly LocalizationSource s_title = LocalizationSource.Create<AllFeaturesDeploymentStep>("All Features");

    public AllFeaturesDeploymentStep()
    {
        Name = "AllFeatures";
        Category = s_category;
        Title = s_title;
    }

    public bool IgnoreDisabledFeatures { get; set; }
}
