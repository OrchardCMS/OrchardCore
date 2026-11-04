using System.Text.Json.Serialization;
using OrchardCore.Deployment;
using OrchardCore.Localization;

namespace OrchardCore.AdminMenu.Deployment;

/// <summary>
/// Adds all admin menus to a <see cref="DeploymentPlanResult"/>.
/// </summary>
public class AdminMenuDeploymentStep : DeploymentStep
{
    public override string Name => "AdminMenu";

    [JsonIgnore]
    public override LocalizationSource Category => CommonLocalizationSources.ContentManagement;

    [JsonIgnore]
    public override LocalizationSource Title => AdminMenuLocalizationSources.AdminMenus;
}
