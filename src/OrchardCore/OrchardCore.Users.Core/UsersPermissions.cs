using OrchardCore.Security.Permissions;

namespace OrchardCore.Users;

public static class UsersPermissions
{
    private static readonly Dictionary<string, PermissionTemplate> s_userPermissionTemplates;
    
    /// <summary>
    /// When authorizing request ManageUsers and pass an <see cref="IUser"/>
    /// Do not request a dynamic permission unless you are checking if the user can manage a specific role.
    /// </summary>
    public static readonly Permission ManageUsers = new("ManageUsers", "Manage security settings and all users", true);

    /// <summary>
    /// Allows viewing user profiles.
    /// </summary>
    public static readonly Permission ViewUsers;
    public static readonly Permission EditUsers;
    public static readonly Permission DeleteUsers;
    public static readonly Permission ListUsers;
    public static readonly Permission AssignRoleToUsers;
    public static readonly Permission DisableTwoFactorAuthenticationForUsers;
    public static readonly Permission EditOwnUser;

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
        ViewUsers = new("View Users", "View user profiles", [ManageUsers]);
        EditUsers = new("EditUsers", "Edit any user", [ManageUsers], true);
        DeleteUsers = new("DeleteUsers", "Delete any user", [ManageUsers], true);
        ListUsers = new("ListUsers", "List all users", [EditUsers, DeleteUsers]);
        AssignRoleToUsers = new("AssignRoleToUsers", "Assign any role to users", true);
        DisableTwoFactorAuthenticationForUsers = new("DisableTwoFactorAuthenticationForUsers", "Disable two-factor authentication for any user", [ManageUsers], true);
        EditOwnUser = new("ManageOwnUserInformation", "Edit own user information", [EditUsers]);
        
        s_userPermissionTemplates = new Dictionary<string, PermissionTemplate>
        {
            [EditUsers.Name] = CreateTemplate("EditUsersInRole", "Edit users in {0} role", EditUsers),
            [DeleteUsers.Name] = CreateTemplate("DeleteUsersInRole", "Delete users in {0} role", DeleteUsers),
            [ListUsers.Name] = CreateTemplate("ListUsersInRole", "List users in {0} role", ListUsers),
            [AssignRoleToUsers.Name] = CreateTemplate("AssignRoleToUsers", "Assign {0} role to users", AssignRoleToUsers),
            [ManageUsers.Name] = CreateTemplate("ManageUsersInRole", "Manage users in {0} role", ManageUsers),
        };
    }

    private static PermissionTemplate CreateTemplate(string nameBase, string description, Permission impliedBy) =>
        new(nameBase + "_{0}", description, Category: null, [impliedBy], [], impliedBy.IsSecurityCritical);
}
