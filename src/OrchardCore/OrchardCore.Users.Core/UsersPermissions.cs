using OrchardCore.Localization;
using OrchardCore.Security.Permissions;

namespace OrchardCore.Users;

public static class UsersPermissions
{
    private static readonly Dictionary<string, PermissionTemplate> s_userPermissionTemplates;

    /// <summary>
    /// When authorizing request ManageUsers and pass an <see cref="IUser"/>
    /// Do not request a dynamic permission unless you are checking if the user can manage a specific role.
    /// </summary>
    public static readonly Permission ManageUsers = new("ManageUsers", LocalizationSource.Create("Manage security settings and all users", typeof(UsersPermissions)), true);

    /// <summary>
    /// Allows viewing user profiles.
    /// </summary>
    public static readonly Permission ViewUsers = new("View Users", LocalizationSource.Create("View user profiles", typeof(UsersPermissions)), [ManageUsers]);

    public static readonly Permission EditUsers = new("EditUsers", LocalizationSource.Create("Edit any user", typeof(UsersPermissions)), [ManageUsers], true);

    public static readonly Permission DeleteUsers = new("DeleteUsers", LocalizationSource.Create("Delete any user", typeof(UsersPermissions)), [ManageUsers], true);

    public static readonly Permission ListUsers = new("ListUsers", LocalizationSource.Create("List all users", typeof(UsersPermissions)), [EditUsers, DeleteUsers]);

    public static readonly Permission AssignRoleToUsers = new("AssignRoleToUsers", LocalizationSource.Create("Assign any role to users", typeof(UsersPermissions)), true);

    public static readonly Permission DisableTwoFactorAuthenticationForUsers = new("DisableTwoFactorAuthenticationForUsers", LocalizationSource.Create("Disable two-factor authentication for any user", typeof(UsersPermissions)), [ManageUsers], true);

    public static readonly Permission EditOwnUser = new("ManageOwnUserInformation", LocalizationSource.Create("Edit own user information", typeof(UsersPermissions)), [EditUsers]);

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
        ViewUsers = new("View Users", LocalizationSource.Create("View user profiles", typeof(UsersPermissions)), [ManageUsers]);
        EditUsers = new("EditUsers", LocalizationSource.Create("Edit any user", typeof(UsersPermissions)), [ManageUsers], true);
        DeleteUsers = new("DeleteUsers", LocalizationSource.Create("Delete any user", typeof(UsersPermissions)), [ManageUsers], true);
        ListUsers = new("ListUsers", LocalizationSource.Create("List all users", typeof(UsersPermissions)), [EditUsers, DeleteUsers]);
        AssignRoleToUsers = new("AssignRoleToUsers", LocalizationSource.Create("Assign any role to users", typeof(UsersPermissions)), true);
        DisableTwoFactorAuthenticationForUsers = new("DisableTwoFactorAuthenticationForUsers", LocalizationSource.Create("Disable two-factor authentication for any user", typeof(UsersPermissions)), [ManageUsers], true);
        EditOwnUser = new("ManageOwnUserInformation", LocalizationSource.Create("Edit own user information", typeof(UsersPermissions)), [EditUsers]);
        
        s_userPermissionTemplates = new Dictionary<string, PermissionTemplate>
        {
            [EditUsers.Name] = CreateTemplate("EditUsersInRole", LocalizationSource.Create("Edit users in {0} role", typeof(UsersPermissions)), EditUsers),
            [DeleteUsers.Name] = CreateTemplate("DeleteUsersInRole", LocalizationSource.Create("Delete users in {0} role", typeof(UsersPermissions)), DeleteUsers),
            [ListUsers.Name] = CreateTemplate("ListUsersInRole", LocalizationSource.Create("List users in {0} role", typeof(UsersPermissions)), ListUsers),
            [AssignRoleToUsers.Name] = CreateTemplate("AssignRoleToUsers", LocalizationSource.Create("Assign {0} role to users", typeof(UsersPermissions)), AssignRoleToUsers),
            [ManageUsers.Name] = CreateTemplate("ManageUsersInRole", LocalizationSource.Create("Manage users in {0} role", typeof(UsersPermissions)), ManageUsers),
        };
    }

    private static PermissionTemplate CreateTemplate(string nameBase, LocalizationSource description, Permission impliedBy) =>
        new(nameBase + "_{0}", description, category: null, impliedBy)
        {
            IsSecurityCritical = impliedBy.IsSecurityCritical,
        };
}
