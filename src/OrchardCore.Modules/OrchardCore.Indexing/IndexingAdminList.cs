namespace OrchardCore.Indexing;

/// <summary>
/// The admin list of index profiles rendered by the <c>AdminList</c> shape.
/// Its columns are declared by <see cref="IndexingAdminListColumnProvider"/>.
/// </summary>
public static class IndexingAdminList
{
    /// <summary>
    /// The name of the list, used by <see cref="Admin.IAdminListColumnProvider"/> and the
    /// <c>AdminList__Indexes</c> and <c>AdminListCell__Indexes__{Column}</c> alternates.
    /// </summary>
    public const string Name = "Indexes";
}
