namespace OrchardCore.DataPipelines.SharedLinks;

/// <summary>
/// A link that gives users of the site access to a resource, such as a file a pipeline created or a report. Only the
/// SHA-256 hash of the link's token is stored. The link works for its recipients only, once signed in, until it expires,
/// is used as many times as allowed, or is revoked.
/// </summary>
public sealed class SharedLink
{
    /// <summary>
    /// Gets or sets the document identifier.
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// Gets or sets the identifier of the link, which is part of its URL.
    /// </summary>
    public string LinkId { get; set; }

    /// <summary>
    /// Gets or sets the kind of resource the link gives access to, which names the <see cref="ISharedLinkHandler"/> that
    /// opens it, such as <c>DataPipelines.File</c>.
    /// </summary>
    public string Kind { get; set; }

    /// <summary>
    /// Gets or sets the identifier of the resource, as its handler knows it.
    /// </summary>
    public string ResourceId { get; set; }

    /// <summary>
    /// Gets or sets the name of the resource, such as a file name, shown to the recipients and in the admin.
    /// </summary>
    public string Title { get; set; }

    /// <summary>
    /// Gets or sets the name of what shared the resource, such as a pipeline or a report, shown in the admin.
    /// </summary>
    public string SourceName { get; set; }

    /// <summary>
    /// Gets or sets values the handler of the link keeps with it, such as the media type of a file.
    /// </summary>
    public Dictionary<string, string> Properties { get; set; } = new(StringComparer.Ordinal);

    /// <summary>
    /// Gets or sets the SHA-256 hash of the link's token, as hexadecimal text.
    /// </summary>
    public string TokenHash { get; set; }

    /// <summary>
    /// Gets or sets the identifiers of the users the link works for.
    /// </summary>
    public List<string> RecipientUserIds { get; set; } = [];

    /// <summary>
    /// Gets or sets the names of the users the link works for, for display.
    /// </summary>
    public List<string> RecipientNames { get; set; } = [];

    /// <summary>
    /// Gets or sets when the link was created, in UTC.
    /// </summary>
    public DateTime CreatedUtc { get; set; }

    /// <summary>
    /// Gets or sets when the link stops working, in UTC.
    /// </summary>
    public DateTime ExpiresUtc { get; set; }

    /// <summary>
    /// Gets or sets when the link was revoked, in UTC, if it was.
    /// </summary>
    public DateTime? RevokedUtc { get; set; }

    /// <summary>
    /// Gets or sets how many times the link can be used, or <c>0</c> for no limit.
    /// </summary>
    public int MaxUses { get; set; }

    /// <summary>
    /// Gets or sets how many times the link was used. <see cref="ISharedLinkManager"/> loads it from a counter the
    /// database updates atomically, so the maximum of uses is never exceeded.
    /// </summary>
    public int Uses { get; set; }

    /// <summary>
    /// Gets or sets when the link was last used, in UTC.
    /// </summary>
    public DateTime? LastUsedUtc { get; set; }

    /// <summary>
    /// Tells whether the link was used as many times as allowed.
    /// </summary>
    public bool IsUsedUp => MaxUses > 0 && Uses >= MaxUses;

    /// <summary>
    /// Tells whether the link works at a given time.
    /// </summary>
    /// <param name="utcNow">The current time, in UTC.</param>
    /// <returns><see langword="true"/> when the link is neither expired, revoked nor used up.</returns>
    public bool IsActive(DateTime utcNow)
    {
        return RevokedUtc is null && ExpiresUtc > utcNow && !IsUsedUp;
    }
}
