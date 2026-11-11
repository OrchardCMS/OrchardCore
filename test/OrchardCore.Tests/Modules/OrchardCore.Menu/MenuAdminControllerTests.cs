using System.Text.Json.Nodes;
using MenuAdminController = OrchardCore.Menu.Controllers.AdminController;

namespace OrchardCore.Tests.Modules.OrchardCore.Menu;

public class MenuAdminControllerTests
{
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void FindMenuItem_EmptyIdentifier_DoesNotMatchTheMenu(string menuItemId)
    {
        // Menu items without an identifier make the menu editor send an empty one.
        var menu = Menu(Item("Home"), Item("About"));

        Assert.Null(MenuAdminController.FindMenuItem(menu, menuItemId));
    }

    [Fact]
    public void FindMenuItem_MatchingIdentifier_ReturnsTheMenuItem()
    {
        var about = Item("About", "about");
        var menu = Menu(Item("Home", "home"), about);

        Assert.Same(about, MenuAdminController.FindMenuItem(menu, "about"));
    }

    [Fact]
    public void FindMenuItem_NestedMenuItem_ReturnsTheMenuItem()
    {
        var child = Item("Child", "child");
        var parent = Item("Parent", "parent");
        parent["MenuItemsListPart"] = new JsonObject { ["MenuItems"] = new JsonArray(child) };
        var menu = Menu(parent);

        Assert.Same(child, MenuAdminController.FindMenuItem(menu, "child"));
    }

    [Fact]
    public void FindMenuItem_IdentifierThatIsNotAString_IsSkipped()
    {
        var invalid = Item("Home");
        invalid["ContentItemId"] = new JsonObject();
        var about = Item("About", "about");
        var menu = Menu(invalid, about);

        Assert.Same(about, MenuAdminController.FindMenuItem(menu, "about"));
    }

    [Fact]
    public void FindMenuItem_MenuItemsListPartWithoutMenuItems_ReturnsNull()
    {
        var menu = new JsonObject { ["MenuItemsListPart"] = new JsonObject() };

        Assert.Null(MenuAdminController.FindMenuItem(menu, "about"));
    }

    private static JsonObject Menu(params JsonObject[] items)
        => new()
        {
            ["MenuItemsListPart"] = new JsonObject { ["MenuItems"] = new JsonArray(items) },
        };

    private static JsonObject Item(string displayText, string contentItemId = null)
    {
        var item = new JsonObject
        {
            ["ContentType"] = "LinkMenuItem",
            ["DisplayText"] = displayText,
        };

        if (contentItemId is not null)
        {
            item["ContentItemId"] = contentItemId;
        }

        return item;
    }
}
