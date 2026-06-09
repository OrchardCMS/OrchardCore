using OrchardCore.ContentManagement;

namespace OrchardCore.Contents.VersionPruning.Services;

public static class ContentVersionPruningSelector
{
    /// <summary>
    /// Selects, for a single content item, the archived (non-latest, non-published) versions that
    /// should be deleted. The <paramref name="versionsToKeep"/> most-recent archived versions are
    /// always retained regardless of age. From the remaining versions, only those modified before
    /// <paramref name="threshold"/> (or with no modification date) are returned for deletion.
    /// </summary>
    /// <param name="versions">The candidate versions for a single content item. Latest and published versions are ignored.</param>
    /// <param name="versionsToKeep">The number of most-recent archived versions to always keep.</param>
    /// <param name="threshold">Versions modified on or after this date are kept.</param>
    public static List<ContentItem> SelectForDeletion(IEnumerable<ContentItem> versions, int versionsToKeep, DateTime threshold)
    {
        var keep = Math.Max(0, versionsToKeep);

        // Newest-first so the most-recent versions are protected by Skip().
        // A null ModifiedUtc is treated as the oldest possible value, so such versions sort last.
        var ordered = versions
            .Where(x => !x.Latest && !x.Published)
            .OrderBy(x => x.ModifiedUtc.HasValue ? 0 : 1)
            .ThenByDescending(x => x.ModifiedUtc)
            .ThenBy(x => x.Id);

        return ordered
            .Skip(keep)
            .Where(x => x.ModifiedUtc == null || x.ModifiedUtc < threshold)
            .ToList();
    }
}
