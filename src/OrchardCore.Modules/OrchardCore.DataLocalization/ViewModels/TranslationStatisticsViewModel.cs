namespace OrchardCore.DataLocalization.ViewModels;

/// <summary>
/// ViewModel for the translation statistics page.
/// </summary>
public class TranslationStatisticsViewModel
{
    /// <summary>
    /// Overall translation progress across all cultures.
    /// </summary>
    public ProgressViewModel Overall { get; set; } = new();

    /// <summary>
    /// Translation progress by culture.
    /// </summary>
    public IList<CultureStatisticsViewModel> ByCulture { get; set; } = [];

    /// <summary>
    /// Translation progress by category for a selected culture.
    /// </summary>
    public IDictionary<string, IList<CategoryStatisticsViewModel>> ByCategory { get; set; } = new Dictionary<string, IList<CategoryStatisticsViewModel>>();
}

/// <summary>
/// Represents progress statistics.
/// </summary>
public class ProgressViewModel
{
    /// <summary>
    /// Total number of translatable strings.
    /// </summary>
    public int Total { get; set; }

    /// <summary>
    /// Number of translated strings.
    /// </summary>
    public int Translated { get; set; }

    /// <summary>
    /// Percentage of completion (0-100).
    /// </summary>
    public int Percentage => Total > 0 ? (int)Math.Round(Translated * 100.0 / Total) : 0;
}

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
