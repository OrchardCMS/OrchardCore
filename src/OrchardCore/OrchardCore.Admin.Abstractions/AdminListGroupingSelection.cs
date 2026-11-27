namespace OrchardCore.Admin;

/// <summary>
/// The available options and resolved grouping for one list.
/// </summary>
public sealed class AdminListGroupingSelection
{
    /// <summary>
    /// The query parameter used by the grouping selector and page links.
    /// </summary>
    public const string QueryKey = "groupBy";

    /// <summary>
    /// The explicit choice disabling grouping, including a saved preference.
    /// </summary>
    public const string None = "None";

    /// <summary>
    /// The providers registered for this list.
    /// </summary>
    public IReadOnlyList<IAdminListGroupingProvider> Providers { get; init; } = [];

    /// <summary>
    /// The selected provider, or null when grouping is disabled.
    /// </summary>
    public IAdminListGroupingProvider Provider { get; init; }

    /// <summary>
    /// The normalized choice to preserve in page links and filter submissions.
    /// </summary>
    public string Name => Provider?.Name ?? None;
}
