using OrchardCore.AdminMenu.Services;
using OrchardCore.Data.Migration;
using OrchardCore.Environment.Shell.Scope;

namespace OrchardCore.AdminMenu;

public sealed class Migrations : DataMigration
{
    public static int Create()
    {
        // Then migrate pre-existing admin menus (created with pre-3.0 libraries) to the 3.0 format.
        ShellScope.AddDeferredTask(async scope =>
            await AdminMenuItemMigrator.MigrateItemTo(scope, async (menu, menuItem, scope) => menuItem.MenuName = menu.Name));

        return 1;
    }
}
