using Microsoft.Extensions.Localization;
using OrchardCore.Deployment;

namespace OrchardCore.Media.Deployment;

/// <summary>
/// Adds media profiles to a <see cref="DeploymentPlanResult"/>.
/// </summary>
public class AllMediaProfilesDeploymentStep : DeploymentStep
{
    public AllMediaProfilesDeploymentStep()
    {
        Name = "AllMediaProfiles";
    }

    public AllMediaProfilesDeploymentStep(IStringLocalizer<AllMediaProfilesDeploymentStep> S)
        : this()
    {
        Category = S["Content Management"];
    }
}
