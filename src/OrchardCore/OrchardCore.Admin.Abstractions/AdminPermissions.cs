using OrchardCore.Localization;
using OrchardCore.Security.Permissions;

namespace OrchardCore.Admin;

public static class AdminPermissions
{
    public static readonly Permission AccessAdminPanel = new("AccessAdminPanel", LocalizationSource.Create("Access admin panel", typeof(AdminPermissions)));

    public static readonly Permission ManageAdminSettings = new("ManageAdminSettings", LocalizationSource.Create("Manage Admin Settings", typeof(AdminPermissions)));
}
