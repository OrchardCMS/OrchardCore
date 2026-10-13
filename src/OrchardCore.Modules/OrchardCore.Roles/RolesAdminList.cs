namespace OrchardCore.Roles;

/// <summary>
/// The admin list of roles rendered by the <c>AdminList</c> shape.
/// Its columns are declared by <see cref="RolesAdminListColumnDriver"/>.
/// </summary>
public static class RolesAdminList
{
    /// <summary>
    /// The name of the list, passed to its column drivers and used by the
    /// <c>AdminList__Roles</c> and <c>AdminListCell__Roles__{Column}</c> alternates.
    /// </summary>
    public const string Name = "Roles";
}
