using OrchardCore.AdminMenu.Services;
using OrchardCore.Data.Migration;
using OrchardCore.Environment.Shell.Scope;

namespace OrchardCore.AdminMenu;

public sealed class Migrations : DataMigration
{
    // Migrate pre-existing admin menus (created with pre-3.0 libraries) to the 3.0 format.
    public static int Create()
    {
        // Data manipulation must be deferred so that it runs in a fresh scope after the
        // migration's schema transaction has been committed. Each deferred task runs in its
        // own scope, which also avoids concurrency issues when several features migrate the
        // same AdminMenuList document.
        ShellScope.AddDeferredTask(async scope => await AdminMenuItemMigrator.MigrateItemTo(scope, async (menu, menuItem, scope) => menuItem.MenuName = menu.Name));

        return 1;
    }
}
