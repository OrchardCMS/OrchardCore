using OrchardCore.Localization;
using OrchardCore.Security.Permissions;

namespace OrchardCore.Admin;

public static class AdminPermissions
{
    public static readonly Permission AccessAdminPanel = new("AccessAdminPanel", new LocalizationSource("Access admin panel", typeof(AdminPermissions)));

    public static readonly Permission ManageAdminSettings = new("ManageAdminSettings", new LocalizationSource("Manage Admin Settings", typeof(AdminPermissions)));
}
