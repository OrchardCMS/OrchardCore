using System.Text.Json.Serialization;
using OrchardCore.Deployment;
using OrchardCore.Localization;

namespace OrchardCore.Themes.Schemes.Deployment;

/// <summary>
/// Adds the schemes of every theme that supports them, the themes' defaults included, and the site's default scheme
/// of each theme, to a <see cref="DeploymentPlanResult"/>.
/// </summary>
public sealed class ThemeSchemesDeploymentStep : DeploymentStep
{
    /// <summary>
    /// The name of the deployment step, and of the recipe step that imports what it exports.
    /// </summary>
    public const string StepName = "ThemeSchemes";

    /// <summary>
    /// The name of the array of schemes in the recipe step.
    /// </summary>
    public const string EntriesName = "Schemes";

    /// <summary>
    /// The name of the object of the recipe step that maps a theme id to the id of its default scheme.
    /// </summary>
    public const string DefaultsName = "Defaults";

    /// <summary>
    /// The name of the property of an entry that holds the id of the theme of the scheme.
    /// </summary>
    public const string ThemeIdName = "ThemeId";

    /// <summary>
    /// The name of the property of an entry that holds the id of the scheme.
    /// </summary>
    public const string ItemIdName = "ItemId";

    public override string Name => StepName;

    [JsonIgnore]
    public override LocalizationSource Category => ThemesLocalizationSources.Theming;

    [JsonIgnore]
    public override LocalizationSource Title => ThemesLocalizationSources.ThemeSchemes;
}
