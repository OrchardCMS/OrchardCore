using OrchardCore.DataLocalization.Models;

namespace OrchardCore.DataLocalization.Services;

/// <summary>Validates and persists UI overrides for admin and automated translation workflows.</summary>
public interface IUiTranslationsManager
{
    /// <summary>Gets the immutable tenant overrides.</summary>
    Task<UiTranslationsDocument> GetAsync();

    /// <summary>Gets the required number of integer plural forms for the configured runtime rule.</summary>
    /// <param name="culture">The configured culture.</param>
    int GetPluralFormCount(string culture);

    /// <summary>Validates a batch against embedded resources and saves it atomically. Empty arrays remove overrides.</summary>
    /// <param name="culture">The configured culture.</param>
    /// <param name="translations">The updates, identified by context and key.</param>
    Task UpdateAsync(string culture, IEnumerable<UiTranslation> translations);

    /// <summary>Validates and imports a PO file atomically. Empty translations restore PO fallback.</summary>
    /// <param name="culture">The configured culture.</param>
    /// <param name="reader">The PO reader.</param>
    Task ImportAsync(string culture, TextReader reader);

    /// <summary>Exports the selected culture's overrides as PO.</summary>
    /// <param name="culture">The configured culture.</param>
    Task<string> ExportAsync(string culture);
}
