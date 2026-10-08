using OrchardCore.Deployment;
using OrchardCore.Localization;

namespace OrchardCore.DataLocalization.Deployment;

/// <summary>
/// Adds translations to a <see cref="DeploymentPlanResult"/>.
/// </summary>
public class TranslationsDeploymentStep : DeploymentStep
{
    private static readonly LocalizationSource s_category = LocalizationSource.Create<TranslationsDeploymentStep>("Internationalization");

    public TranslationsDeploymentStep()
    {
        Name = "Translations";
        Category = s_category;
    }

    /// <summary>
    /// Gets or sets whether to include all cultures or selected cultures.
    /// </summary>
    public bool IncludeAll { get; set; } = true;

    /// <summary>
    /// Gets or sets the cultures to export when <see cref="IncludeAll"/> is false.
    /// </summary>
    public string[] Cultures { get; set; } = [];

    /// <summary>
    /// Gets or sets the categories to export. Empty means all categories.
    /// </summary>
    public string[] Categories { get; set; } = [];
}
