using OrchardCore.Deployment;
using OrchardCore.Localization;

namespace OrchardCore.Themes.Deployment;

/// <summary>
/// Adds the currently selected admin theme and site theme to a <see cref="DeploymentPlanResult"/>.
/// </summary>
public class ThemesDeploymentStep : DeploymentStep, IDeploymentStep
{
    public override string Name => "Themes";

    public static LocalizationSource Category => ThemesLocalizationSources.Theming;
}
