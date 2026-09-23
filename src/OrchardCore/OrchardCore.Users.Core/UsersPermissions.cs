using Microsoft.Extensions.Localization;
using OrchardCore.Security.Permissions;

namespace OrchardCore.Users;

public static class UsersPermissions
{
    /// <summary>
    /// When authorizing request ManageUsers and pass an <see cref="IUser"/>
    /// Do not request a dynamic permission unless you are checking if the user can manage a specific role.
    /// </summary>
    public static readonly Permission ManageUsers = new("ManageUsers", LocalizedString.Create("Manage security settings and all users", typeof(UsersPermissions)), true);

    /// <summary>
    /// Allows viewing user profiles.
    /// </summary>
    public static readonly Permission ViewUsers = new("View Users", LocalizedString.Create("View user profiles", typeof(UsersPermissions)), [ManageUsers]);

    public static readonly Permission EditUsers = new("EditUsers", LocalizedString.Create("Edit any user", typeof(UsersPermissions)), [ManageUsers], true);

    public static readonly Permission DeleteUsers = new("DeleteUsers", LocalizedString.Create("Delete any user", typeof(UsersPermissions)), [ManageUsers], true);

    public static readonly Permission ListUsers = new("ListUsers", LocalizedString.Create("List all users", typeof(UsersPermissions)), [EditUsers, DeleteUsers]);

    public static readonly Permission AssignRoleToUsers = new("AssignRoleToUsers", LocalizedString.Create("Assign any role to users", typeof(UsersPermissions)), true);

    public static readonly Permission DisableTwoFactorAuthenticationForUsers = new("DisableTwoFactorAuthenticationForUsers", LocalizedString.Create("Disable two-factor authentication for any user", typeof(UsersPermissions)), [ManageUsers], true);

    public static readonly Permission EditOwnUser = new("ManageOwnUserInformation", LocalizedString.Create("Edit own user information", typeof(UsersPermissions)), [EditUsers]);

    public static Permission CreateEditUsersInRolePermission(string roleName) =>
        CreateDynamicPermission(roleName, new Permission("EditUsersInRole_{0}", "Edit users in {0} role", [EditUsers], true));

    public static Permission CreateDeleteUsersInRolePermission(string roleName) =>
        CreateDynamicPermission(roleName, new Permission("DeleteUsersInRole_{0}", "Delete users in {0} role", [DeleteUsers], true));

    public static Permission CreateListUsersInRolePermission(string roleName) =>
        CreateDynamicPermission(roleName, new Permission("ListUsersInRole_{0}", "List users in {0} role", [ListUsers]));

    public static Permission CreateAssignRoleToUsersPermission(string roleName) =>
        CreateDynamicPermission(roleName, new Permission("AssignRoleToUsers_{0}", "Assign {0} role to users", [AssignRoleToUsers], true));

    public static Permission CreatePermissionForManageUsersInRole(string name) =>
        CreateDynamicPermission(name, new Permission("ManageUsersInRole_{0}", "Manage users in {0} role", [ManageUsers], true));

    // Dynamic permission template.
    private static Permission CreateDynamicPermission(string roleName, Permission permission)
        => new(
            string.Format(permission.Name, roleName),
            string.Format(permission.Description, roleName),
            permission.ImpliedBy,
            permission.IsSecurityCritical
        );
}
