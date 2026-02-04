namespace OrchardCore.DataLocalization.ViewModels;

/// <summary>
/// Represents a culture available for translation.
/// </summary>
public class CultureViewModel
{
    /// <summary>
    /// The culture name (e.g., "fr-FR").
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// The display name of the culture (e.g., "French (France)").
    /// </summary>
    public string DisplayName { get; set; }

    /// <summary>
    /// Whether the current user can edit translations for this culture.
    /// </summary>
    public bool CanEdit { get; set; }
}
