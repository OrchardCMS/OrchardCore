namespace OrchardCore.Contents.VersionPruning.Models;

public sealed class ContentVersionPruningSettings
{
    /// <summary>
    /// The number of days after which non-latest, non-published content item versions are deleted.
    /// </summary>
    public int RetentionDays { get; set; } = 30;

    /// <summary>
    /// The number of the most-recent archived (non-latest, non-published) versions to retain
    /// per content item, regardless of age.
    /// </summary>
    public int VersionsToKeep { get; set; } = 1;

    /// <summary>
    /// The content types to prune. When empty, no content types are pruned.
    /// </summary>
    public string[] ContentTypes { get; set; } = [];

    /// <summary>
    /// Whether the pruning background task is disabled.
    /// </summary>
    public bool Disabled { get; set; }
}
