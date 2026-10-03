using OrchardCore.Localization;
using OrchardCore.Security.Permissions;

namespace OrchardCore.Roles;

public static class RolesPermissions
{
    public static readonly Permission ManageRoles = new("ManageRoles", new LocalizationSource("Manage Roles", typeof(RolesPermissions)), isSecurityCritical: true);
    public static readonly Permission ViewRoles = new("ViewRoles", new LocalizationSource("View Roles", typeof(RolesPermissions)), [ManageRoles]);
}
