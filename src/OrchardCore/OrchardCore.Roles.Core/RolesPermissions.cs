using OrchardCore.Localization;
using OrchardCore.Security.Permissions;

namespace OrchardCore.Roles;

public static class RolesPermissions
{
    public static readonly Permission ManageRoles = new("ManageRoles", LocalizationSource.Create("Manage Roles", typeof(RolesPermissions)), isSecurityCritical: true);
    public static readonly Permission ViewRoles = new("ViewRoles", LocalizationSource.Create("View Roles", typeof(RolesPermissions)), [ManageRoles]);
}
