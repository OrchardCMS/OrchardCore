using System.Text.Json.Serialization;
using OrchardCore.Deployment;
using OrchardCore.Localization;

namespace OrchardCore.Themes.Deployment;

/// <summary>
/// Adds the currently selected admin theme and site theme to a <see cref="DeploymentPlanResult"/>.
/// </summary>
public class ThemesDeploymentStep : DeploymentStep
{
    [JsonIgnore]
    public override LocalizationSource Category => CommonLocalizationSources.Theming;

    public ThemesDeploymentStep()
    {
        Name = "Themes";
    }
}
