namespace OrchardCore.ContentManagement;

public record VersionOptions
{
    /// <summary>
    /// Gets the latest version.
    /// </summary>
    public static readonly VersionOptions Latest = new()
    {
        IsLatest = true,
    };

    /// <summary>
    /// Gets the latest published version.
    /// </summary>
    public static readonly VersionOptions Published = new()
    {
        IsPublished = true,
    };

    /// <summary>
    /// Gets the latest draft version.
    /// </summary>
    public static readonly VersionOptions Draft = new()
    {
        IsDraft = true,
    };

    /// <summary>
    /// Gets the latest version and creates a new version draft based on it.
    /// </summary>
    public static readonly VersionOptions DraftRequired = new()
    {
        IsDraft = true,
        IsDraftRequired = true,
    };

    public bool IsLatest { get; private set; }

    public bool IsPublished { get; private set; }

    public bool IsDraft { get; private set; }

    public bool IsDraftRequired { get; private set; }
}
