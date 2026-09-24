using Microsoft.Extensions.Localization;
using OrchardCore.Security.Permissions;

namespace OrchardCore.Roles;

public static class RolesPermissions
{
    public static readonly Permission ManageRoles = new("ManageRoles", LocalizedString.Create("Manage Roles", typeof(RolesPermissions)), isSecurityCritical: true);
    public static readonly Permission ViewRoles = new("ViewRoles", LocalizedString.Create("View Roles", typeof(RolesPermissions)), [ManageRoles]);
}
