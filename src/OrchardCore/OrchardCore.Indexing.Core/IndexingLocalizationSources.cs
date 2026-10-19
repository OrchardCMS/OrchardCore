using OrchardCore.Localization;

namespace OrchardCore.Indexing.Core;

/// <summary>
/// Untranslated strings of this module. Their PO context is <c>OrchardCore.Indexing.Core.IndexingLocalizationSources</c>.
/// </summary>
/// <remarks>
/// Use the sources of <see cref="CommonLocalizationSources"/> for the strings that have the same meaning across modules.
/// </remarks>
internal static class IndexingLocalizationSources
{
    /// <summary>Gets the "Index Profiles" text.</summary>
    public static readonly LocalizationSource IndexProfiles = LocalizationSource.Create("Index Profiles", typeof(IndexingLocalizationSources));

    /// <summary>Gets the "Indexing" text.</summary>
    public static readonly LocalizationSource Indexing = LocalizationSource.Create("Indexing", typeof(IndexingLocalizationSources));

    /// <summary>Gets the "Rebuild Indexes" text.</summary>
    public static readonly LocalizationSource RebuildIndexes = LocalizationSource.Create("Rebuild Indexes", typeof(IndexingLocalizationSources));

    /// <summary>Gets the "Reset Indexes" text.</summary>
    public static readonly LocalizationSource ResetIndexes = LocalizationSource.Create("Reset Indexes", typeof(IndexingLocalizationSources));
}
