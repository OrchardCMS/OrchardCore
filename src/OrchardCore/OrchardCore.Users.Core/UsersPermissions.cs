using OrchardCore.Security.Permissions;

namespace OrchardCore.Users;

public static class UsersPermissions
{
    /// <summary>
    /// When authorizing request ManageUsers and pass an <see cref="IUser"/>
    /// Do not request a dynamic permission unless you are checking if the user can manage a specific role.
    /// </summary>
    public static readonly Permission ManageUsers = new("ManageUsers", "Manage security settings and all users", true);

    /// <summary>
    /// Allows viewing user profiles.
    /// </summary>
    public static readonly Permission ViewUsers = new("View Users", "View user profiles", [ManageUsers]);

    public static readonly Permission EditUsers = new("EditUsers", "Edit any user", [ManageUsers], true);

    public static readonly Permission DeleteUsers = new("DeleteUsers", "Delete any user", [ManageUsers], true);

    public static readonly Permission ListUsers = new("ListUsers", "List all users", [EditUsers, DeleteUsers]);

    public static readonly Permission AssignRoleToUsers = new("AssignRoleToUsers", "Assign any role to users", true);

    public static readonly Permission DisableTwoFactorAuthenticationForUsers = new("DisableTwoFactorAuthenticationForUsers", "Disable two-factor authentication for any user", [ManageUsers], true);

    public static readonly Permission EditOwnUser = new("ManageOwnUserInformation", "Edit own user information", [EditUsers]);

    public static Permission CreateEditUsersInRolePermission(string roleName) =>
        CreateDynamicPermission(roleName, "EditUsersInRole_{0}", "Edit users in {0} role", EditUsers);

    public static Permission CreateDeleteUsersInRolePermission(string roleName) =>
        CreateDynamicPermission(roleName, "DeleteUsersInRole_{0}", "Delete users in {0} role", DeleteUsers);

    public static Permission CreateListUsersInRolePermission(string roleName) =>
        CreateDynamicPermission(roleName, "ListUsersInRole_{0}", "List users in {0} role", ListUsers, isSecurityCritical: false);

    public static Permission CreateAssignRoleToUsersPermission(string roleName) =>
        CreateDynamicPermission(roleName, "AssignRoleToUsers_{0}", "Assign {0} role to users", AssignRoleToUsers);

    public static Permission CreatePermissionForManageUsersInRole(string name) =>
        CreateDynamicPermission(name, "ManageUsersInRole_{0}", "Manage users in {0} role", ManageUsers);

    // Dynamic permission template.
    private static Permission CreateDynamicPermission(
        string roleName, string name, string description, Permission impliedBy, bool isSecurityCritical = true) =>
        new PermissionTemplate(name, description, Category: null, [impliedBy], isSecurityCritical)
            .CreateDynamicPermission(roleName);
}
