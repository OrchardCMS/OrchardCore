using Microsoft.Extensions.Localization;
using OrchardCore.Deployment;

namespace OrchardCore.Features.Deployment;

/// <summary>
/// Adds enabled and disabled features to a <see cref="DeploymentPlanResult"/>.
/// </summary>
public class AllFeaturesDeploymentStep : DeploymentStep
{
    public AllFeaturesDeploymentStep()
    {
        Name = "AllFeatures";
        Category = LocalizedString.Create("Infrastructure");
    }

    public bool IgnoreDisabledFeatures { get; set; }
}
