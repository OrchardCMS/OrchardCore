using OrchardCore.Localization;

namespace OrchardCore.Recipes;

/// <summary>
/// Untranslated strings of this module. Their PO context is <c>OrchardCore.Recipes.RecipesLocalizationSources</c>.
/// </summary>
/// <remarks>
/// Use the sources of <see cref="CommonLocalizationSources"/> for the strings that have the same meaning across modules.
/// </remarks>
internal static class RecipesLocalizationSources
{
    /// <summary>Gets the title of the job that runs a recipe.</summary>
    public static readonly LocalizationSource ExecutionJobTitle = LocalizationSource.Create("Recipe", typeof(RecipesLocalizationSources));

    /// <summary>Gets the description of the job that runs a recipe.</summary>
    public static readonly LocalizationSource ExecutionJobDescription = LocalizationSource.Create("Runs a recipe from the list of recipes of the admin.", typeof(RecipesLocalizationSources));
}
