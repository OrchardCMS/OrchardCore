namespace OrchardCore.UrlRewriting;

/// <summary>
/// The admin list of rewrite rules rendered by the <c>AdminList</c> shape.
/// Its columns are declared by <see cref="UrlRewritingAdminListColumnProvider"/>.
/// </summary>
public static class UrlRewritingAdminList
{
    /// <summary>
    /// The name of the list, used by <see cref="Admin.IAdminListColumnProvider"/> and the
    /// <c>AdminList__UrlRewriting</c> and <c>AdminListCell__UrlRewriting__{Column}</c> alternates.
    /// </summary>
    public const string Name = "UrlRewriting";

    /// <summary>
    /// The id of the element wrapping the rows, which the sortable script reorders.
    /// </summary>
    public const string SortableContainerId = "rewrite-rules-sortable-list";
}
