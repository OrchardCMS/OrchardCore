using OrchardCore.Security.Permissions;

namespace OrchardCore.Roles;

public static class RolesPermissions
{
    public static readonly Permission ManageRoles = new("ManageRoles", "Manage Roles", isSecurityCritical: true);
    public static readonly Permission ViewRoles = new("ViewRoles", "View Roles", [ManageRoles]);
}
