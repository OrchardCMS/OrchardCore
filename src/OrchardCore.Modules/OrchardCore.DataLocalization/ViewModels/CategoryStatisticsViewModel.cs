namespace OrchardCore.DataLocalization.ViewModels;

/// <summary>
/// Translation statistics for a specific category/provider.
/// </summary>
public class CategoryStatisticsViewModel : ProgressViewModel
{
    /// <summary>
    /// The category/provider name (e.g., "Permissions", "Content Types").
    /// </summary>
    public string Category { get; set; }
}
