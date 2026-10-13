namespace OrchardCore.Queries;

/// <summary>
/// The admin list of queries rendered by the <c>AdminList</c> shape.
/// Its columns are declared by <see cref="QueriesAdminListColumnDriver"/>.
/// </summary>
public static class QueriesAdminList
{
    /// <summary>
    /// The name of the list, passed to its column drivers and used by the
    /// <c>AdminList__Queries</c> and <c>AdminListCell__Queries__{Column}</c> alternates.
    /// </summary>
    public const string Name = "Queries";
}
