namespace OrchardCore.Lists;

/// <summary>
/// The list of content items contained in a List content item, rendered by the <c>AdminList</c> shape.
/// </summary>
/// <remarks>
/// It has its own name rather than reusing the one of the Manage Content list: this module does not
/// reference the Contents module, and the two lists are configured independently. The columns read the same
/// zones of the <c>Content_SummaryAdmin</c> shape, so the rows present the same way.
/// </remarks>
public static class ListPartContentsAdminList
{
    /// <summary>
    /// The name of the list, used by <see cref="Admin.IAdminListColumnProvider"/> and the
    /// <c>AdminList__ListPartContents</c> and <c>AdminListCell__ListPartContents__{Column}</c> alternates.
    /// </summary>
    public const string Name = "ListPartContents";
}
