using OrchardCore.Localization;

namespace OrchardCore.Lucene;

/// <summary>
/// Untranslated strings of this module. Their PO context is <c>OrchardCore.Lucene.LuceneLocalizationSources</c>.
/// </summary>
/// <remarks>
/// Use the sources of <see cref="CommonLocalizationSources"/> for the strings that have the same meaning across modules.
/// </remarks>
internal static class LuceneLocalizationSources
{
    /// <summary>Gets the "Lucene Search Indexes" text.</summary>
    public static readonly LocalizationSource LuceneSearchIndexes = LocalizationSource.Create("Lucene Search Indexes", typeof(LuceneLocalizationSources));

    /// <summary>Gets the "Rebuild Lucene Search Indices" text.</summary>
    public static readonly LocalizationSource RebuildLuceneSearchIndices = LocalizationSource.Create("Rebuild Lucene Search Indices", typeof(LuceneLocalizationSources));

    /// <summary>Gets the "Reset Lucene Search Indices" text.</summary>
    public static readonly LocalizationSource ResetLuceneSearchIndices = LocalizationSource.Create("Reset Lucene Search Indices", typeof(LuceneLocalizationSources));
}
