using OrchardCore.Deployment;
using OrchardCore.Localization;

namespace OrchardCore.Roles.Deployment;

/// <summary>
/// Adds roles to a <see cref="DeploymentPlanResult"/>.
/// </summary>
public class AllRolesDeploymentStep : DeploymentStep
{
    public AllRolesDeploymentStep()
    {
        Name = "AllRoles";
        Category = CommonLocalizationSources.Security;
    }
}
