using OrchardCore.ContentManagement;

namespace OrchardCore.Contents.VersionPruning.Services;

public static class ContentVersionPruningSelector
{
    public static List<ContentItem> SelectForDeletion(IEnumerable<ContentItem> candidates, int minVersionsToKeep)
    {
        var result = new List<ContentItem>();

        foreach (var group in candidates.GroupBy(x => x.ContentItemId))
        {
            // Sort newest-first so Skip() protects the most-recent N versions.
            // Null ModifiedUtc is treated as oldest: Nullable.Compare places null before
            // any non-null value, so descending order puts null-dated versions last.
            var ordered = group
                .Where(x =>
                    !x.Latest &&
                    !x.Published)
                .OrderByDescending(x => x.ModifiedUtc, Comparer<DateTime?>.Create(Nullable.Compare))
                .Skip(minVersionsToKeep);

            result.AddRange(ordered);
        }

        return result;
    }
}
