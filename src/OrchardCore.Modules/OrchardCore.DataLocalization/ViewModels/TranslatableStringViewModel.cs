namespace OrchardCore.DataLocalization.ViewModels;

/// <summary>
/// Represents a single translatable string.
/// </summary>
public class TranslatableStringViewModel
{
    /// <summary>
    /// The full context string (e.g., "Permissions", "Content Types", "Content Fields;TextField").
    /// </summary>
    public string Context { get; set; }

    /// <summary>
    /// The original (untranslated) string key.
    /// </summary>
    public string Key { get; set; }

    /// <summary>
    /// The translated value for the current culture, or empty if not translated.
    /// </summary>
    public string Value { get; set; }
}
