using OrchardCore.Data.Documents;

namespace OrchardCore.DataLocalization.Models;

/// <summary>
/// Tenant UI overrides, independently versioned from dynamic data translations.
/// </summary>
internal sealed class UITranslationsDocument : Document
{
    /// <summary>Gets the translations by configured culture.</summary>
    public Dictionary<string, List<UITranslation>> Translations { get; } = new(StringComparer.OrdinalIgnoreCase);
}
