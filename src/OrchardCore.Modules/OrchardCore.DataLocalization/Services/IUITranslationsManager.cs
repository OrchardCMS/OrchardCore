using OrchardCore.DataLocalization.Models;

namespace OrchardCore.DataLocalization.Services;

/// <summary>Validates and persists UI overrides for admin and automated translation workflows.</summary>
public interface IUITranslationsManager
{
    /// <summary>Gets the entries of the embedded localization catalogs, including untranslated entries and extracted metadata.</summary>
    IReadOnlyList<UILocalizationResource> GetResources();

    /// <summary>Gets the stored overrides of a culture.</summary>
    /// <param name="culture">The configured culture.</param>
    Task<IReadOnlyList<UITranslation>> GetTranslationsAsync(string culture);

    /// <summary>Gets an example count for each plural form, in the order of the culture's plural rule.</summary>
    /// <param name="culture">The configured culture.</param>
    int[] GetPluralFormExamples(string culture);

    /// <summary>Validates a batch against the embedded catalogs and saves it atomically. Empty values remove overrides.</summary>
    /// <param name="culture">The configured culture.</param>
    /// <param name="translations">The updates, identified by context and key.</param>
    Task UpdateAsync(string culture, IEnumerable<UITranslation> translations);

    /// <summary>Validates and imports a file atomically. Entries without translations remove their overrides.</summary>
    /// <param name="culture">The configured culture.</param>
    /// <param name="format">The file format, identified by its extension, e.g. <c>po</c>.</param>
    /// <param name="reader">The file reader.</param>
    Task ImportAsync(string culture, string format, TextReader reader);

    /// <summary>Exports the overrides of a culture.</summary>
    /// <param name="culture">The configured culture.</param>
    /// <param name="format">The file format, identified by its extension, e.g. <c>po</c>.</param>
    /// <param name="writer">The file writer.</param>
    Task ExportAsync(string culture, string format, TextWriter writer);
}
