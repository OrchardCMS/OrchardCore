using OrchardCore.Security.Permissions;

namespace OrchardCore.Roles;

public static class CommonPermissions
{
    [Obsolete("This Permission is no longer used and will be removed. Instead use `RolesPermissions.ManageRoles`.")]
    public static readonly Permission ManageRoles = RolesPermissions.ManageRoles;

    [Obsolete("This Permission is no longer used and will be removed. Instead use OrchardCore.Users.CommonPermissions.AssignRoleToUsers.")]
    public static readonly Permission AssignRoles = new("AssignRoles", "Assign Roles", [RolesPermissions.ManageRoles], isSecurityCritical: true);

    /// <summary>
    /// Dynamic permission template for assign role.
    /// </summary>
    [Obsolete("This Permission is no longer used and will be removed. Instead use OrchardCore.Users.CommonPermissions.CreateAssignRoleToUsersPermission(roleName).")]
    private static readonly Permission _assignRole = new("AssignRole_{0}", "Assign Role - {0}", [AssignRoles, RolesPermissions.ManageRoles]);

    [Obsolete("This Permission is no longer used and will be removed. Instead use OrchardCore.Users.CommonPermissions.CreateAssignRoleToUsersPermission(roleName).")]
    public static Permission CreatePermissionForAssignRole(string name) =>
        new(
            string.Format(_assignRole.Name, name),
            string.Format(_assignRole.Description, name),
            _assignRole.ImpliedBy
        );
}
