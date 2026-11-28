using OrchardCore.Deployment;
using OrchardCore.Localization;

namespace OrchardCore.Users.Deployment;

/// <summary>
/// Adds users to a <see cref="DeploymentPlanResult"/>.
/// </summary>
public class AllUsersDeploymentStep : DeploymentStep, IDeploymentStep
{
    public override string Name => "AllUsers";

    public static LocalizationSource Category => CommonLocalizationSources.Security;
}
