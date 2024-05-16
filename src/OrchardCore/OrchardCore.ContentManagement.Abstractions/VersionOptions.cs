namespace OrchardCore.ContentManagement;

public class VersionOptions
{
    /// <summary>
    /// Gets the latest version.
    /// </summary>
    public static VersionOptions Latest { get { return new VersionOptions { IsLatest = true }; } }

    /// <summary>
    /// Gets the latest published version.
    /// </summary>
    public static VersionOptions Published { get { return new VersionOptions { IsPublished = true }; } }

    /// <summary>
    /// Gets the latest draft version.
    /// </summary>
    public static VersionOptions Draft { get { return new VersionOptions { IsDraft = true }; } }

    /// <summary>
    /// Gets the latest version and creates a new version draft based on it.
    /// </summary>
    public static VersionOptions DraftRequired { get { return new VersionOptions { IsDraft = true, IsDraftRequired = true }; } }

    /// <summary>
    /// Gets all versions.
    /// </summary>
    public static VersionOptions AllVersions { get { return new VersionOptions { IsAllVersions = true }; } }

    public bool IsLatest { get; private set; }
    public bool IsPublished { get; private set; }
    public bool IsDraft { get; private set; }
    public bool IsDraftRequired { get; private set; }
    public bool IsAllVersions { get; private set; }
}
