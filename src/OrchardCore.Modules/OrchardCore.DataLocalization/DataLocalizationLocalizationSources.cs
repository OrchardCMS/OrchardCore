using OrchardCore.Localization;

namespace OrchardCore.DataLocalization;

/// <summary>
/// Untranslated strings of this module. Their PO context is <c>OrchardCore.DataLocalization.DataLocalizationLocalizationSources</c>.
/// </summary>
/// <remarks>
/// Use the sources of <see cref="CommonLocalizationSources"/> for the strings that have the same meaning across modules.
/// </remarks>
internal static class DataLocalizationLocalizationSources
{
    /// <summary>Gets the "Internationalization" text.</summary>
    public static readonly LocalizationSource Internationalization = LocalizationSource.Create("Internationalization", typeof(DataLocalizationLocalizationSources));
}
