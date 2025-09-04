using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Records;
using OrchardCore.Data.Migration;
using OrchardCore.Menu.Models;
using OrchardCore.Recipes;
using OrchardCore.Recipes.Services;
using YesSql;

namespace OrchardCore.Menu;

public sealed class Migrations : DataMigration
{
    private readonly IRecipeMigrator _recipeMigrator;
    private readonly ISession _session;

    public Migrations(
        IRecipeMigrator recipeMigrator,
        ISession session)
    {
        _recipeMigrator = recipeMigrator;
        _session = session;
    }

    public async Task<int> CreateAsync()
    {
        await _recipeMigrator.ExecuteAsync($"menu{RecipesConstants.RecipeExtension}", this);

        // Shortcut other migration steps on new content definition schemas.
        return 4;
    }

    // Add content menu. This only needs to run on old content definition schemas.
    // This code can be removed in a later version.
    public async Task<int> UpdateFrom1Async()
    {
        await _recipeMigrator.ExecuteAsync($"content-menu-updatefrom1{RecipesConstants.RecipeExtension}", this);

        return 2;
    }

    // Add html menu. This only needs to run on old content definition schemas.
    // This code can be removed in a later version.
    public async Task<int> UpdateFrom2Async()
    {
        await _recipeMigrator.ExecuteAsync($"html-menu-updatefrom2{RecipesConstants.RecipeExtension}", this);

        return 3;
    }

    public async Task<int> UpdateFrom3Async()
    {
        var menus = await _session.Query<ContentItem, ContentItemIndex>(x => x.ContentType == "Menu").ListAsync();

        foreach (var menu in menus)
        {
            var menuItemsListPart = menu.As<MenuItemsListPart>();
            if (menuItemsListPart != null)
            {
                MigrateMenuItems(menuItemsListPart.MenuItems);
                menu.Apply(menuItemsListPart);
            }

            await _session.SaveAsync(menu);
        }

        return 4;
    }

    private static void MigrateMenuItems(List<ContentItem> menuItems)
    {
        foreach (var menuItem in menuItems)
        {
            var menuItemsListPart = menuItem.As<MenuItemsListPart>();
            if (menuItemsListPart != null)
            {
                MigrateMenuItems(menuItemsListPart.MenuItems);
                menuItem.Apply(menuItemsListPart);
            }
        }
    }
}
