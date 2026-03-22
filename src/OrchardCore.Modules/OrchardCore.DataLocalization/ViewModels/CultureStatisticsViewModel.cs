namespace OrchardCore.DataLocalization.ViewModels;

/// <summary>
/// Translation statistics for a specific culture.
/// </summary>
public class CultureStatisticsViewModel : ProgressViewModel
{
    /// <summary>
    /// The culture name (e.g., "fr-FR").
    /// </summary>
    public string Culture { get; set; }

    /// <summary>
    /// The display name of the culture (e.g., "French (France)").
    /// </summary>
    public string DisplayName { get; set; }
}
