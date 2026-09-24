namespace OrchardCore.Queries;

/// <summary>
/// The admin list of queries rendered by the <c>AdminList</c> shape.
/// Its columns are declared by <see cref="QueriesAdminListColumnProvider"/>.
/// </summary>
public static class QueriesAdminList
{
    /// <summary>
    /// The name of the list, used by <see cref="OrchardCore.Admin.IAdminListColumnProvider"/> and the
    /// <c>AdminList__Queries</c> and <c>AdminListCell__Queries__{Column}</c> alternates.
    /// </summary>
    public const string Name = "Queries";
}
