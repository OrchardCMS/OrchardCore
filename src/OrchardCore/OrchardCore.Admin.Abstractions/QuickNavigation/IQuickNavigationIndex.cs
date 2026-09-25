namespace OrchardCore.Admin.QuickNavigation;

/// <summary>
/// Stores tenant-wide, culture-independent entry identifiers, partitioned by source.
/// Features may update their partition from background tasks or event handlers.
/// Only registered sources are rendered, and each source must authorize its entries at display time.
/// </summary>
public interface IQuickNavigationIndex
{
    /// <summary>
    /// Gets an immutable snapshot of the identifiers belonging to a source.
    /// </summary>
    /// <param name="source">The unique source name.</param>
    IReadOnlyList<string> GetEntryIds(string source);

    /// <summary>
    /// Atomically replaces a source's identifiers. An empty collection removes its entries.
    /// Index contents are in memory and must be repopulated after a tenant restart.
    /// </summary>
    /// <param name="source">The unique source name.</param>
    /// <param name="entryIds">Culture-independent identifiers, not localized labels or user-specific URLs.</param>
    void Replace(string source, IEnumerable<string> entryIds);
}
