using OrchardCore.Deployment;
using OrchardCore.Localization;

namespace OrchardCore.Themes.Deployment;

/// <summary>
/// Adds the currently selected admin theme and site theme to a <see cref="DeploymentPlanResult"/>.
/// </summary>
public class ThemesDeploymentStep : DeploymentStep
{
    private static readonly LocalizationSource s_category = new("Theming", typeof(ThemesDeploymentStep));

    public ThemesDeploymentStep()
    {
        Name = "Themes";
        Category = s_category;
    }
}
