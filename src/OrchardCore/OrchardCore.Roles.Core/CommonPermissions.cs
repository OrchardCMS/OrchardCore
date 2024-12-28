using OrchardCore.Security.Permissions;

namespace OrchardCore.Roles;

public static class CommonPermissions
{
    public static readonly Permission ManageRoles = new("ManageRoles", "Managing Roles", isSecurityCritical: true);

    [Obsolete("This Permission is no longer used and will be removed. Instead use OrchardCore.Users.CommonPermissions.AssignRoleToUsers.")]
    public static readonly Permission AssignRoles = new("AssignRoles", "Assign Roles", [ManageRoles], isSecurityCritical: true);

    /// <summary>
    /// Dynamic permission template for assign role.
    /// </summary>
    [Obsolete("This Permission is no longer used and will be removed. Instead use OrchardCore.Users.CommonPermissions.CreateAssignRoleToUsersPermission(roleName).")]
    private static readonly Permission _assignRole = new("AssignRole_{0}", "Assign Role - {0}", [AssignRoles, ManageRoles]);

    [Obsolete("This Permission is no longer used and will be removed. Instead use OrchardCore.Users.CommonPermissions.CreateAssignRoleToUsersPermission(roleName).")]
    public static Permission CreatePermissionForAssignRole(string name) =>
        new(
            string.Format(_assignRole.Name, name),
            string.Format(_assignRole.Description, name),
            _assignRole.ImpliedBy
        );
}
