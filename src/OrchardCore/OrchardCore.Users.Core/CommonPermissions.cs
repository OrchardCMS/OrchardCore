using OrchardCore.Security.Permissions;

namespace OrchardCore.Users;

[Obsolete("This class is deprecated, use `UsersPermissions` instead.")]
public static class CommonPermissions
{
    /// <summary>
    /// When authorizing request ManageUsers and pass an <see cref="IUser"/>
    /// Do not request a dynamic permission unless you are checking if the user can manage a specific role.
    /// </summary>
    public static readonly Permission ManageUsers = UsersPermissions.ManageUsers;

    /// <summary>
    /// Allows viewing user profiles.
    /// </summary>
    public static readonly Permission ViewUsers = UsersPermissions.ViewUsers;

    public static readonly Permission EditUsers = UsersPermissions.EditUsers;

    public static readonly Permission DeleteUsers = UsersPermissions.DeleteUsers;

    public static readonly Permission ListUsers = UsersPermissions.ListUsers;

    public static readonly Permission AssignRoleToUsers = UsersPermissions.AssignRoleToUsers;

    public static readonly Permission DisableTwoFactorAuthenticationForUsers = UsersPermissions.DisableTwoFactorAuthenticationForUsers;

    public static readonly Permission EditOwnUser = UsersPermissions.EditOwnUser;

    public static Permission CreateEditUsersInRolePermission(string roleName) =>
        UsersPermissions.CreateEditUsersInRolePermission(roleName);

    public static Permission CreateDeleteUsersInRolePermission(string roleName) =>
        UsersPermissions.CreateDeleteUsersInRolePermission(roleName);

    public static Permission CreateListUsersInRolePermission(string roleName) =>
        UsersPermissions.CreateListUsersInRolePermission(roleName);

    public static Permission CreateAssignRoleToUsersPermission(string roleName) =>
        UsersPermissions.CreateEditUsersInRolePermission(roleName);

    public static Permission CreatePermissionForManageUsersInRole(string name) =>
        UsersPermissions.CreatePermissionForManageUsersInRole(name);
}
