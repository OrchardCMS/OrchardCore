using OrchardCore.Localization;

namespace OrchardCore.Deployment;

/// <summary>
/// Untranslated strings of this module. Their PO context is <c>OrchardCore.Deployment.DeploymentLocalizationSources</c>.
/// </summary>
/// <remarks>
/// Use the sources of <see cref="CommonLocalizationSources"/> for the strings that have the same meaning across modules.
/// </remarks>
internal static class DeploymentLocalizationSources
{
    /// <summary>Gets the "Custom File" text.</summary>
    public static readonly LocalizationSource CustomFile = LocalizationSource.Create("Custom File", typeof(DeploymentLocalizationSources));

    /// <summary>Gets the "Deployment" text.</summary>
    public static readonly LocalizationSource Deployment = LocalizationSource.Create("Deployment", typeof(DeploymentLocalizationSources));

    /// <summary>Gets the "Deployment Plans" text.</summary>
    public static readonly LocalizationSource DeploymentPlans = LocalizationSource.Create("Deployment Plans", typeof(DeploymentLocalizationSources));

    /// <summary>Gets the "JSON Recipe" text.</summary>
    public static readonly LocalizationSource JsonRecipe = LocalizationSource.Create("JSON Recipe", typeof(DeploymentLocalizationSources));

    /// <summary>Gets the "Recipe File" text.</summary>
    public static readonly LocalizationSource RecipeFile = LocalizationSource.Create("Recipe File", typeof(DeploymentLocalizationSources));

    /// <summary>Gets the title of the job that imports a deployment package.</summary>
    public static readonly LocalizationSource ImportJobTitle = LocalizationSource.Create("Deployment Package Import", typeof(DeploymentLocalizationSources));

    /// <summary>Gets the description of the job that imports a deployment package.</summary>
    public static readonly LocalizationSource ImportJobDescription = LocalizationSource.Create("Imports a deployment package uploaded from the admin.", typeof(DeploymentLocalizationSources));
}
