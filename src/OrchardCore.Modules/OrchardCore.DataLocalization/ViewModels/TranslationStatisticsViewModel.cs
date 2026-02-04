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
