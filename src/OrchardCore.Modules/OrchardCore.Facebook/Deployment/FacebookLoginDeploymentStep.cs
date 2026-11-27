using OrchardCore.Deployment;
using OrchardCore.Localization;

namespace OrchardCore.Facebook.Deployment;

/// <summary>
/// Adds Facebook Login settings to a <see cref="DeploymentPlanResult"/>.
/// </summary>
public class FacebookLoginDeploymentStep : DeploymentStep, IDeploymentStep
{
    public override string Name => "Facebook Login";

    public static LocalizationSource Category => FacebookLocalizationSources.Meta;
}
