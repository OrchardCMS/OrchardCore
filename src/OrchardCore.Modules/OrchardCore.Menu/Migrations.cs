using System.Text.Json.Nodes;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Records;
using OrchardCore.Data.Migration;
using OrchardCore.Menu.Models;
using OrchardCore.Menu.Services;
using OrchardCore.Recipes;
using OrchardCore.Recipes.Services;
using YesSql;

namespace OrchardCore.Menu;

public sealed class Migrations : DataMigration
{
    private readonly IRecipeMigrator _recipeMigrator;
    private readonly ISession _session;
    private readonly IContentDefinitionManager _contentDefinitionManager;

    public Migrations(
        IRecipeMigrator recipeMigrator,
        ISession session,
        IContentDefinitionManager contentDefinitionManager)
    {
        _recipeMigrator = recipeMigrator;
        _session = session;
        _contentDefinitionManager = contentDefinitionManager;
    }

    public async Task<int> CreateAsync()
    {
        await _recipeMigrator.ExecuteAsync($"menu{RecipesConstants.RecipeExtension}", this);

        // Shortcut other migration steps on new content definition schemas.
        return 5;
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
            if (menu.TryGet<MenuItemsListPart>(out var menuItemsListPart))
            {
                MigrateMenuItems(menuItemsListPart.MenuItems);
                menu.Apply(menuItemsListPart);
            }

            await _session.SaveAsync(menu);
        }

        return 4;
    }

    // Give an identifier to the menu items stored without one, for example by a recipe that omits it. The menu
    // admin finds a menu item by its identifier, so these menu items could not be edited or deleted.
    public async Task<int> UpdateFrom4Async()
    {
        // Index providers read the content definition while the session saves. Loading it first caches it in the
        // scope, so the save does not use the session from a second thread.
        await _contentDefinitionManager.ListTypeDefinitionsAsync();

        var menus = await _session.Query<ContentItem, ContentItemIndex>(x => x.ContentType == "Menu" && (x.Latest || x.Published)).ListAsync();

        foreach (var menu in menus)
        {
            if (MenuItemIdentifiers.AssignMissing((JsonObject)menu.Content, IdGenerator.GenerateId))
            {
                await _session.SaveAsync(menu);
            }
        }

        return 5;
    }

    private static void MigrateMenuItems(List<ContentItem> menuItems)
    {
        foreach (var menuItem in menuItems)
        {
            if (menuItem.TryGet<MenuItemsListPart>(out var menuItemsListPart))
            {
                MigrateMenuItems(menuItemsListPart.MenuItems);
                menuItem.Apply(menuItemsListPart);
            }
        }
    }
}
