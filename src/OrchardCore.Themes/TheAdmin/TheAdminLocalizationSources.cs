using OrchardCore.Localization;

namespace OrchardCore.Themes.TheAdmin;

/// <summary>
/// Untranslated strings of this theme. Their PO context is <c>OrchardCore.Themes.TheAdmin.TheAdminLocalizationSources</c>.
/// </summary>
/// <remarks>
/// Use the sources of <see cref="CommonLocalizationSources"/> for the strings that have the same meaning across modules.
/// </remarks>
internal static class TheAdminLocalizationSources
{
    /// <summary>Gets the "Theming" text.</summary>
    public static readonly LocalizationSource Theming = LocalizationSource.Create("Theming", typeof(TheAdminLocalizationSources));

    /// <summary>Gets the "Admin schemes" text.</summary>
    public static readonly LocalizationSource AdminSchemes = LocalizationSource.Create("Admin schemes", typeof(TheAdminLocalizationSources));
}
