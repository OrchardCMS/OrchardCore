using OrchardCore.Deployment;
using OrchardCore.Localization;

namespace OrchardCore.AdminMenu.Deployment;

/// <summary>
/// Adds all admin menus to a <see cref="DeploymentPlanResult"/>.
/// </summary>
public class AdminMenuDeploymentStep : DeploymentStep
{
    private static readonly LocalizationSource s_category = new("Content Management", typeof(AdminMenuDeploymentStep));
    private static readonly LocalizationSource s_title = new("Admin Menus", typeof(AdminMenuDeploymentStep));

    public AdminMenuDeploymentStep()
    {
        Name = "AdminMenu";
        Category = s_category;
        Title = s_title;
    }
}
