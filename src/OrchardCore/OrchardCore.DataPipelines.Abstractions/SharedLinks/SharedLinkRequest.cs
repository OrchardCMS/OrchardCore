namespace OrchardCore.DataPipelines.SharedLinks;

/// <summary>
/// What a link gives access to, who to share it with, and for how long.
/// </summary>
public sealed class SharedLinkRequest
{
    /// <summary>
    /// Gets or sets the kind of resource, the <see cref="ISharedLinkHandler.Kind"/> of the handler that opens it.
    /// </summary>
    public string Kind { get; set; }

    /// <summary>
    /// Gets or sets the identifier of the resource, as its handler knows it.
    /// </summary>
    public string ResourceId { get; set; }

    /// <summary>
    /// Gets or sets the name of the resource, such as a file name.
    /// </summary>
    public string Title { get; set; }

    /// <summary>
    /// Gets or sets the name of what shares the resource, such as a pipeline or a report.
    /// </summary>
    public string SourceName { get; set; }

    /// <summary>
    /// Gets or sets values the handler keeps with the link.
    /// </summary>
    public Dictionary<string, string> Properties { get; set; } = new(StringComparer.Ordinal);

    /// <summary>
    /// Gets or sets the user names or emails of the users of the site the link works for.
    /// </summary>
    public IReadOnlyList<string> Recipients { get; set; } = [];

    /// <summary>
    /// Gets or sets how long the link works.
    /// </summary>
    public TimeSpan Lifetime { get; set; } = TimeSpan.FromDays(7);

    /// <summary>
    /// Gets or sets how many times the link can be used, or <c>0</c> for no limit.
    /// </summary>
    public int MaxUses { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the link is emailed to the recipients. This requires the Email feature.
    /// Otherwise, the caller delivers the URL of the link itself.
    /// </summary>
    public bool NotifyByEmail { get; set; } = true;

    /// <summary>
    /// Gets or sets the subject of the email.
    /// </summary>
    public string Subject { get; set; }

    /// <summary>
    /// Gets or sets a message added to the email.
    /// </summary>
    public string Message { get; set; }
}

/// <summary>
/// A link that was created, and its URL.
/// </summary>
public sealed class SharedLinkResult
{
    /// <summary>
    /// Gets or sets the link.
    /// </summary>
    public SharedLink Link { get; set; }

    /// <summary>
    /// Gets or sets the URL of the link. It holds the token, so it is only sent to the recipients.
    /// </summary>
    public string Url { get; set; }

    /// <summary>
    /// Gets or sets the names of the recipients the link was emailed to. The others have no email address, or the email
    /// could not be sent.
    /// </summary>
    public IReadOnlyList<string> Notified { get; set; } = [];
}

/// <summary>
/// Tells whether a user may use a link, or why not.
/// </summary>
public enum SharedLinkAccessResult
{
    /// <summary>
    /// The user may use the link.
    /// </summary>
    Allowed,

    /// <summary>
    /// There is no such link.
    /// </summary>
    NotFound,

    /// <summary>
    /// The link expired or was revoked.
    /// </summary>
    Expired,

    /// <summary>
    /// The link was used as many times as allowed.
    /// </summary>
    UsedUp,

    /// <summary>
    /// The token of the link is missing or wrong.
    /// </summary>
    InvalidToken,

    /// <summary>
    /// The user is not signed in, or is not a recipient of the link.
    /// </summary>
    NotARecipient,
}
