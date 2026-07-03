using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OrchardCore.AdminMenu.Models;
using OrchardCore.Environment.Shell.Scope;
using OrchardCore.Navigation;

namespace OrchardCore.AdminMenu.Services;

public class AdminMenuItemMigrator
{
    public static async Task MigrateItemTo(ShellScope scope, Func<Models.AdminMenu, MenuItem, ShellScope, Task> migrateItem)
    {
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<AdminMenuItemMigrator>>();

        try
        {
            var adminMenuService = scope.ServiceProvider.GetRequiredService<IAdminMenuService>();

            var adminMenuList = await adminMenuService.LoadAdminMenuListAsync();

            // invalidate a live enumerator.
            foreach (var menu in adminMenuList.AdminMenu.ToArray())
            {
                await MigrateAdminNodeAsync(menu, menu.MenuItems, scope, migrateItem);
                await adminMenuService.SaveAsync(menu);
            }
        }
        catch (Exception e)
        {
            // Log explicitly any exception.
            logger.LogError(e, "Failed to migrate admin menus to the 3.0 format.");
            throw;
        }
    }

    public static async Task MigrateAdminNodeAsync(Models.AdminMenu menu, List<AdminNode> menuItems, ShellScope scope, Func<Models.AdminMenu, MenuItem, ShellScope, Task> migrateItem)
    {
        foreach (var menuItem in menuItems)
        {
            await migrateItem(menu, menuItem, scope);
            await MigrateMenuItemsAsync(menu, menuItem.Items, scope, migrateItem);
        }
    }

    public static async Task MigrateMenuItemsAsync(Models.AdminMenu menu, List<MenuItem> menuItems, ShellScope scope, Func<Models.AdminMenu, MenuItem, ShellScope, Task> migrateItem)
    {
        foreach (var menuItem in menuItems)
        {
            await migrateItem(menu, menuItem, scope);
            await MigrateMenuItemsAsync(menu, menuItem.Items, scope, migrateItem);
        }
    }
}
