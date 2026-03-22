namespace OrchardCore.DataLocalization.ViewModels;

/// <summary>
/// Represents a group of translatable strings from a single provider/category.
/// </summary>
public class TranslatableStringGroupViewModel
{
    /// <summary>
    /// The primary category name (e.g., "Permissions", "Content Types", "Content Fields").
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// The sub-groups within this category (e.g., grouped by field type).
    /// </summary>
    public IList<TranslatableStringSubGroupViewModel> SubGroups { get; set; } = [];

    /// <summary>
    /// The translatable strings directly in this group (not in sub-groups).
    /// </summary>
    public IList<TranslatableStringViewModel> Strings { get; set; } = [];
}
