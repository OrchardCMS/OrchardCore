using OrchardCore.Localization;
using OrchardCore.Security.Permissions;

namespace OrchardCore.AdminMenu;

public static class AdminMenuPermissions
{
    public static readonly Permission ManageAdminMenu = new("ManageAdminMenu", LocalizationSource.Create("Manage the admin menu", typeof(AdminMenuPermissions)));

    public static readonly Permission ViewAdminMenuAll = new("ViewAdminMenuAll", LocalizationSource.Create("View Admin Menu - View All", typeof(AdminMenuPermissions)), new[] { ManageAdminMenu });
}
