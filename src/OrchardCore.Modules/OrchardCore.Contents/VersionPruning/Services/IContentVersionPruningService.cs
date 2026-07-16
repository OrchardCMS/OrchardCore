using OrchardCore.Contents.VersionPruning.Models;

namespace OrchardCore.Contents.VersionPruning.Services;

public interface IContentVersionPruningService
{
    /// <summary>
    /// Deletes old content item versions that are neither the latest nor published,
    /// and whose <c>ModifiedUtc</c> is older than the retention period defined in
    /// <paramref name="settings"/>, while always retaining the
    /// <see cref="ContentVersionPruningSettings.VersionsToKeep"/> most-recent archived
    /// versions per content item.
    /// </summary>
    /// <param name="settings">The pruning settings to apply.</param>
    /// <param name="cancellationToken">A token used to cancel the operation.</param>
    /// <returns>The number of versions deleted.</returns>
    Task<int> PruneVersionsAsync(ContentVersionPruningSettings settings, CancellationToken cancellationToken = default);
}
