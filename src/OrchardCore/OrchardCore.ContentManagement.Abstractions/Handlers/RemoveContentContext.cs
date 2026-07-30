namespace OrchardCore.ContentManagement.Handlers;

/// <summary>
/// Describes why active content item versions are being removed.
/// </summary>
public enum RemoveContentReason
{
    /// <summary>
    /// Active versions are being removed because the content item itself is being deleted.
    /// </summary>
    Deletion,

    /// <summary>
    /// Active versions are being removed because a replacement version is about to be created.
    /// </summary>
    NewVersion,
}

/// <summary>
/// Provides context for content handlers when active versions of a content item are being removed.
/// </summary>
public class RemoveContentContext : ContentContextBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RemoveContentContext"/> class.
    /// </summary>
    /// <param name="contentItem">The content item whose active versions are being removed.</param>
    /// <param name="noActiveVersionLeft">
    /// <see langword="true"/> when no active version will remain after the current removal operation completes;
    /// otherwise, <see langword="false"/>.
    /// </param>
    /// <param name="reason">The reason the active versions are being removed.</param>
    public RemoveContentContext(ContentItem contentItem, bool noActiveVersionLeft = false, RemoveContentReason reason = RemoveContentReason.Deletion) : base(contentItem)
    {
        NoActiveVersionLeft = noActiveVersionLeft;
        Reason = reason;
    }

    /// <summary>
    /// Gets a value indicating whether no active version will remain after the current removal operation completes.
    /// </summary>
    public bool NoActiveVersionLeft { get; }

    /// <summary>
    /// Gets the reason the active versions are being removed.
    /// </summary>
    public RemoveContentReason Reason { get; }

    /// <summary>
    /// Gets or sets a value indicating whether the removal operation should be canceled by a handler.
    /// </summary>
    public bool Cancel { get; set; }
}
