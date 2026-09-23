using Microsoft.Extensions.Localization;
using OrchardCore.Security.Permissions;

namespace OrchardCore.Admin;

public static class AdminPermissions
{
    public static readonly Permission AccessAdminPanel = new("AccessAdminPanel", LocalizedString.Create("Access admin panel", typeof(AdminPermissions)));

    public static readonly Permission ManageAdminSettings = new("ManageAdminSettings", LocalizedString.Create("Manage Admin Settings", typeof(AdminPermissions)));
}
