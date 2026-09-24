namespace OrchardCore.Users;

/// <summary>
/// The admin list of users rendered by the <c>AdminList</c> shape.
/// Its columns are declared by <see cref="UsersAdminListColumnProvider"/>.
/// </summary>
public static class UsersAdminList
{
    /// <summary>
    /// The name of the list, used by <see cref="OrchardCore.Admin.IAdminListColumnProvider"/> and the
    /// <c>AdminList__Users</c> and <c>AdminListCell__Users__{Column}</c> alternates.
    /// </summary>
    public const string Name = "Users";
}
