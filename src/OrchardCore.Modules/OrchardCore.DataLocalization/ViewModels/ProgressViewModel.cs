namespace OrchardCore.DataLocalization.ViewModels;

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
