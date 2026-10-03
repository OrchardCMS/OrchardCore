using OrchardCore.Deployment;
using OrchardCore.Localization;

namespace OrchardCore.Roles.Deployment;

/// <summary>
/// Adds roles to a <see cref="DeploymentPlanResult"/>.
/// </summary>
public class AllRolesDeploymentStep : DeploymentStep
{
    private static readonly LocalizationSource s_category = new("Security", typeof(AllRolesDeploymentStep));

    public AllRolesDeploymentStep()
    {
        Name = "AllRoles";
        Category = s_category;
    }
}
