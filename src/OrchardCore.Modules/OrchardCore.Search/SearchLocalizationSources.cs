using OrchardCore.Localization;

namespace OrchardCore.Search;

/// <summary>
/// Untranslated strings of this module. Their PO context is <c>OrchardCore.Search.SearchLocalizationSources</c>.
/// </summary>
/// <remarks>
/// Use the sources of <see cref="CommonLocalizationSources"/> for the strings that have the same meaning across modules.
/// </remarks>
internal static class SearchLocalizationSources
{
    /// <summary>Gets the "Search Settings" text.</summary>
    public static readonly LocalizationSource SearchSettings = LocalizationSource.Create("Search Settings", typeof(SearchLocalizationSources));
}
