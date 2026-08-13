using OrchardCore.Security.Permissions;

namespace OrchardCore.Users;

public static class UsersPermissions
{
    private static readonly IReadOnlyDictionary<string, PermissionTemplate> s_userPermissionTemplates;
    
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
        s_userPermissionTemplates[EditUsers.Name].CreateDynamicPermission(roleName);

    public static Permission CreateDeleteUsersInRolePermission(string roleName) =>
        s_userPermissionTemplates[DeleteUsers.Name].CreateDynamicPermission(roleName);

    public static Permission CreateListUsersInRolePermission(string roleName) =>
        s_userPermissionTemplates[ListUsers.Name].CreateDynamicPermission(roleName);

    public static Permission CreateAssignRoleToUsersPermission(string roleName) =>
        s_userPermissionTemplates[AssignRoleToUsers.Name].CreateDynamicPermission(roleName);

    public static Permission CreatePermissionForManageUsersInRole(string name) =>
        s_userPermissionTemplates[ManageUsers.Name].CreateDynamicPermission(name);

    static UsersPermissions()
    {
        s_userPermissionTemplates = new Dictionary<string, PermissionTemplate>
        {
            [EditUsers.Name] = new("EditUsersInRole_{0}", "Edit users in {0} role", null, [EditUsers], true),
            [DeleteUsers.Name] = new("DeleteUsersInRole_{0}", "Delete users in {0} role", null, [DeleteUsers], true),
            [ListUsers.Name] = new("ListUsersInRole_{0}", "List users in {0} role", null, [ListUsers]),
            [AssignRoleToUsers.Name] = new("AssignRoleToUsers_{0}", "Assign {0} role to users", null, [AssignRoleToUsers], true),
            [ManageUsers.Name] = new("ManageUsersInRole_{0}", "Manage users in {0} role", null, [ManageUsers], true),
        };
    }
}
