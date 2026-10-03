using OrchardCore.Deployment;
using OrchardCore.Localization;

namespace OrchardCore.Features.Deployment;

/// <summary>
/// Adds enabled and disabled features to a <see cref="DeploymentPlanResult"/>.
/// </summary>
public class AllFeaturesDeploymentStep : DeploymentStep
{
    private static readonly LocalizationSource s_category = new("Infrastructure", typeof(AllFeaturesDeploymentStep));
    private static readonly LocalizationSource s_title = new("All Features", typeof(AllFeaturesDeploymentStep));

    public AllFeaturesDeploymentStep()
    {
        Name = "AllFeatures";
        Category = s_category;
        Title = s_title;
    }

    public bool IgnoreDisabledFeatures { get; set; }
}
