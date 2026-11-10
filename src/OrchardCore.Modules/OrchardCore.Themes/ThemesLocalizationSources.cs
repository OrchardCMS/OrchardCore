using OrchardCore.Localization;

namespace OrchardCore.Themes;

/// <summary>
/// Untranslated strings of this module. Their PO context is <c>OrchardCore.Themes.ThemesLocalizationSources</c>.
/// </summary>
/// <remarks>
/// Use the sources of <see cref="CommonLocalizationSources"/> for the strings that have the same meaning across modules.
/// </remarks>
internal static class ThemesLocalizationSources
{
    /// <summary>Gets the "Theming" text.</summary>
    public static readonly LocalizationSource Theming = LocalizationSource.Create("Theming", typeof(ThemesLocalizationSources));
}
