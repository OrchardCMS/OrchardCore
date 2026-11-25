using System.Text.Json.Serialization;
using OrchardCore.Deployment;
using OrchardCore.Localization;

namespace OrchardCore.Themes.TheAdmin.Deployment;

/// <summary>
/// Adds every scheme of TheAdmin, the theme's own ones included, and the site's default scheme to a
/// <see cref="DeploymentPlanResult"/>.
/// </summary>
public sealed class AdminSchemesDeploymentStep : DeploymentStep
{
    /// <summary>
    /// The name of the deployment step, and of the recipe step that imports what it exports.
    /// </summary>
    public const string StepName = "AdminSchemes";

    /// <summary>
    /// The name of the array of schemes in the recipe step.
    /// </summary>
    public const string EntriesName = "Schemes";

    /// <summary>
    /// The name of the property of the recipe step that holds the id of the site's default scheme.
    /// </summary>
    public const string DefaultSchemeIdName = "DefaultSchemeId";

    /// <summary>
    /// The name of the property of an entry that holds the id of the scheme.
    /// </summary>
    public const string ItemIdName = "ItemId";

    public override string Name => StepName;

    [JsonIgnore]
    public override LocalizationSource Category => TheAdminLocalizationSources.Theming;

    [JsonIgnore]
    public override LocalizationSource Title => TheAdminLocalizationSources.AdminSchemes;
}
