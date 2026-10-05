using OrchardCore.Data.Documents;

namespace OrchardCore.DataLocalization.Models;

/// <summary>
/// Tenant UI overrides, independently versioned from dynamic data translations.
/// </summary>
public sealed class UiTranslationsDocument : Document
{
    /// <summary>Gets the translations by configured culture.</summary>
    public Dictionary<string, List<UiTranslation>> Translations { get; } = new(StringComparer.OrdinalIgnoreCase);
}
