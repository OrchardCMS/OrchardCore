namespace OrchardCore.DataLocalization.ViewModels;

/// <summary>
/// Represents a single translatable string.
/// </summary>
public class TranslatableStringViewModel
{
    /// <summary>
    /// The full context string (e.g., "Permissions", "Content Types", "Content Fields:TextField").
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

    /// <summary>
    /// The plural source identifier for a UI catalog entry, or null for a singular entry.
    /// </summary>
    public string Plural { get; set; }

    /// <summary>
    /// The UI override forms, with an empty input for each required form when no override exists.
    /// Null for dynamic data translations.
    /// </summary>
    public string[] Values { get; set; }

    /// <summary>
    /// Extracted UI catalog comments, flags and source references.
    /// </summary>
    public string[] Metadata { get; set; }

    /// <summary>
    /// Format placeholders and their argument descriptions from the UI catalog.
    /// </summary>
    public string[] FormatArguments { get; set; }
}
