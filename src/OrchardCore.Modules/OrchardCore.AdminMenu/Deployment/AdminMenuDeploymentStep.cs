using OrchardCore.Deployment;
using OrchardCore.Localization;

namespace OrchardCore.AdminMenu.Deployment;

/// <summary>
/// Adds all admin menus to a <see cref="DeploymentPlanResult"/>.
/// </summary>
public class AdminMenuDeploymentStep : DeploymentStep, IDeploymentStep
{
    public override string Name => "AdminMenu";

    public static LocalizationSource Category => CommonLocalizationSources.ContentManagement;

    public static LocalizationSource Title => AdminMenuLocalizationSources.AdminMenus;
}
