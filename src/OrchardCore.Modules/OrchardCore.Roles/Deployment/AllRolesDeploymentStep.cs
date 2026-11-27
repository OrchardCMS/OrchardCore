using OrchardCore.Deployment;
using OrchardCore.Localization;

namespace OrchardCore.Roles.Deployment;

/// <summary>
/// Adds roles to a <see cref="DeploymentPlanResult"/>.
/// </summary>
public class AllRolesDeploymentStep : DeploymentStep, IDeploymentStep
{
    public override string Name => "AllRoles";

    public static LocalizationSource Category => CommonLocalizationSources.Security;
}
