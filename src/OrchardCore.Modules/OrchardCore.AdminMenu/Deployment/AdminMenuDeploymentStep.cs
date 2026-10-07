using OrchardCore.Deployment;
using OrchardCore.Localization;

namespace OrchardCore.AdminMenu.Deployment;

/// <summary>
/// Adds all admin menus to a <see cref="DeploymentPlanResult"/>.
/// </summary>
public class AdminMenuDeploymentStep : DeploymentStep
{
    private static readonly LocalizationSource s_category = LocalizationSource.Create<AdminMenuDeploymentStep>("Content Management");
    private static readonly LocalizationSource s_title = LocalizationSource.Create<AdminMenuDeploymentStep>("Admin Menus");

    public AdminMenuDeploymentStep()
    {
        Name = "AdminMenu";
        Category = s_category;
        Title = s_title;
    }
}
