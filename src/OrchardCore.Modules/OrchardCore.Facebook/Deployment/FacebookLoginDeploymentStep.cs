using OrchardCore.Deployment;
using OrchardCore.Localization;

namespace OrchardCore.Facebook.Deployment;

/// <summary>
/// Adds Facebook Login settings to a <see cref="DeploymentPlanResult"/>.
/// </summary>
public class FacebookLoginDeploymentStep : DeploymentStep
{
    private static readonly LocalizationSource s_category = new("Meta", typeof(FacebookLoginDeploymentStep));

    public FacebookLoginDeploymentStep()
    {
        Name = "Facebook Login";
        Category = s_category;
    }
}
