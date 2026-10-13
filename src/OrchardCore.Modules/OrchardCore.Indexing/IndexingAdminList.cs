namespace OrchardCore.Indexing;

/// <summary>
/// The admin list of index profiles rendered by the <c>AdminList</c> shape.
/// Its columns are declared by <see cref="IndexingAdminListColumnDriver"/>.
/// </summary>
public static class IndexingAdminList
{
    /// <summary>
    /// The name of the list, passed to its column drivers and used by the
    /// <c>AdminList__Indexes</c> and <c>AdminListCell__Indexes__{Column}</c> alternates.
    /// </summary>
    public const string Name = "Indexes";
}
