using OrchardCore.Security.Permissions;

namespace OrchardCore.Roles;

public static class RolesPermissions
{
    public static readonly Permission ManageRoles = new("ManageRoles", "Managing Roles", isSecurityCritical: true);
}
