namespace OrchardCore.Users;

/// <summary>
/// The admin list of users rendered by the <c>AdminList</c> shape.
/// </summary>
public static class UsersAdminList
{
    /// <summary>
    /// The name of the list, which a column provider checks to target it, and which selects the
    /// <c>AdminList__Users</c> and <c>AdminListCell__Users__{Column}</c> alternates.
    /// </summary>
    public const string Name = "Users";
}
