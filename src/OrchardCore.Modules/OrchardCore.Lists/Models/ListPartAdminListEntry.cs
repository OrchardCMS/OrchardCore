using OrchardCore.ContentManagement;
using OrchardCore.DisplayManagement.ModelBinding;

namespace OrchardCore.Lists.Models;

/// <summary>
/// A contained content item and its ListPart-specific rendering context.
/// </summary>
public sealed class ListPartAdminListEntry
{
    /// <summary>
    /// The contained content item.
    /// </summary>
    public ContentItem ContentItem { get; set; }

    /// <summary>
    /// Whether this row offers a drag-ordering handle.
    /// </summary>
    public bool EnableOrdering { get; set; }

    /// <summary>
    /// The parent display group forwarded to the content display manager.
    /// </summary>
    public string GroupId { get; set; }

    /// <summary>
    /// The update model of the containing display context.
    /// </summary>
    public IUpdateModel Updater { get; set; }
}
