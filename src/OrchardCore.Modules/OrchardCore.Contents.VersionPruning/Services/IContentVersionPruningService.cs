using OrchardCore.Contents.VersionPruning.Models;

namespace OrchardCore.Contents.VersionPruning.Services;

public interface IContentVersionPruningService
{
    /// <summary>
    /// Deletes old content item versions that are neither the latest nor published,
    /// and whose <c>ModifiedUtc</c> is older than the retention period defined in
    /// <paramref name="settings"/>, while honouring <see cref="ContentVersionPruningSettings.VersionsToKeep"/>.
    /// </summary>
    /// <param name="settings">The pruning settings to apply.</param>
    /// <returns>The number of versions deleted.</returns>
    Task<int> PruneVersionsAsync(ContentVersionPruningSettings settings);
}
