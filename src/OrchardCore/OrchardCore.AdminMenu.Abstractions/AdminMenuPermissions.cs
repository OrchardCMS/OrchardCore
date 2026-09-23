using Microsoft.Extensions.Localization;
using OrchardCore.Security.Permissions;

namespace OrchardCore.AdminMenu;

public static class AdminMenuPermissions
{
    public static readonly Permission ManageAdminMenu = new("ManageAdminMenu", LocalizedString.Create("Manage the admin menu", typeof(AdminMenuPermissions)));

    public static readonly Permission ViewAdminMenuAll = new("ViewAdminMenuAll", LocalizedString.Create("View Admin Menu - View All", typeof(AdminMenuPermissions)), new[] { ManageAdminMenu });
}
