namespace OrchardCore.ContentManagement;

/// <summary>
/// Represents options for retrieving or creating content items with specific versioning requirements.
/// </summary>
/// <remarks>
/// <para>
/// <see cref="VersionOptions"/> provides a set of predefined options to specify which version of a content item should be retrieved or created.
/// </para>
/// <para>
/// Use the static properties to indicate the desired version:
/// <list type="bullet">
/// <item><see cref="Latest"/>: The most recent version, whether published or not.</item>
/// <item><see cref="Published"/>: The latest published version.</item>
/// <item><see cref="Draft"/>: The most recent draft version, if available.</item>
/// <item><see cref="DraftRequired"/>: Ensures a new draft is created, even if one already exists.</item>
/// </list>
/// </para>
/// </remarks>
public record VersionOptions
{
    /// <summary>
    /// Gets a <see cref="VersionOptions"/> instance that specifies the latest version of the content item,
    /// regardless of its published or draft status.
    /// </summary>
    public static readonly VersionOptions Latest = new()
    {
        IsLatest = true,
    };

    /// <summary>
    /// Gets a <see cref="VersionOptions"/> instance that specifies the latest published version of the content item.
    /// </summary>
    public static readonly VersionOptions Published = new()
    {
        IsPublished = true,
    };

    /// <summary>
    /// Gets a <see cref="VersionOptions"/> instance that specifies the latest draft version of the content item,
    /// if a draft exists.
    /// </summary>
    public static readonly VersionOptions Draft = new()
    {
        IsDraft = true,
    };

    /// <summary>
    /// Gets a <see cref="VersionOptions"/> instance that specifies a new draft version should be created,
    /// even if an existing draft is present. The new draft is based on the latest version.
    /// </summary>
    public static readonly VersionOptions DraftRequired = new()
    {
        IsDraft = true,
        IsDraftRequired = true,
    };

    /// <summary>
    /// Gets a value indicating whether the latest version of the content item is requested.
    /// </summary>
    public bool IsLatest { get; private set; }

    /// <summary>
    /// Gets a value indicating whether the published version of the content item is requested.
    /// </summary>
    public bool IsPublished { get; private set; }

    /// <summary>
    /// Gets a value indicating whether the draft version of the content item is requested.
    /// </summary>
    public bool IsDraft { get; private set; }

    /// <summary>
    /// Gets a value indicating whether a new draft version is required, even if an existing draft is present.
    /// </summary>
    public bool IsDraftRequired { get; private set; }
}
