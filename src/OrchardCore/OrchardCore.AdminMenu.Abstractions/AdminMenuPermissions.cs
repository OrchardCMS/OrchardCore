using OrchardCore.Localization;
using OrchardCore.Security.Permissions;

namespace OrchardCore.AdminMenu;

public static class AdminMenuPermissions
{
    public static readonly Permission ManageAdminMenu = new("ManageAdminMenu", new LocalizationSource("Manage the admin menu", typeof(AdminMenuPermissions)));

    public static readonly Permission ViewAdminMenuAll = new("ViewAdminMenuAll", new LocalizationSource("View Admin Menu - View All", typeof(AdminMenuPermissions)), new[] { ManageAdminMenu });
}
