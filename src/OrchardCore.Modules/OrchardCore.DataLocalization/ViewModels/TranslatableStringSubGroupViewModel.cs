namespace OrchardCore.DataLocalization.ViewModels;

/// <summary>
/// Represents a sub-group of translatable strings within a category (e.g., by field type).
/// </summary>
public class TranslatableStringSubGroupViewModel
{
    /// <summary>
    /// The sub-group name (e.g., field type name).
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// The translatable strings in this sub-group.
    /// </summary>
    public IList<TranslatableStringViewModel> Strings { get; set; } = [];
}
