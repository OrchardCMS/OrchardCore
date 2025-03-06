using OrchardCore.Security.Permissions;

namespace OrchardCore.AdminMenu;

public static class AdminMenuPermissions
{
    public static readonly Permission ManageAdminMenu = new("ManageAdminMenu", "Manage the admin menu");

    public static readonly Permission ViewAdminMenuAll = new("ViewAdminMenuAll", "View Admin Menu - View All", new[] { ManageAdminMenu });
}
