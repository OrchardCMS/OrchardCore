using OrchardCore.Localization;

namespace OrchardCore.Elasticsearch.Core;

/// <summary>
/// Untranslated strings of this module. Their PO context is <c>OrchardCore.Elasticsearch.Core.ElasticsearchLocalizationSources</c>.
/// </summary>
/// <remarks>
/// Use the sources of <see cref="CommonLocalizationSources"/> for the strings that have the same meaning across modules.
/// </remarks>
internal static class ElasticsearchLocalizationSources
{
    /// <summary>Gets the "Elasticsearch Search Indexes" text.</summary>
    public static readonly LocalizationSource ElasticsearchSearchIndexes = LocalizationSource.Create("Elasticsearch Search Indexes", typeof(ElasticsearchLocalizationSources));

    /// <summary>Gets the "Rebuild Elasticsearch Indices" text.</summary>
    public static readonly LocalizationSource RebuildElasticsearchIndices = LocalizationSource.Create("Rebuild Elasticsearch Indices", typeof(ElasticsearchLocalizationSources));

    /// <summary>Gets the "Reset Elasticsearch Indices" text.</summary>
    public static readonly LocalizationSource ResetElasticsearchIndices = LocalizationSource.Create("Reset Elasticsearch Indices", typeof(ElasticsearchLocalizationSources));
}
