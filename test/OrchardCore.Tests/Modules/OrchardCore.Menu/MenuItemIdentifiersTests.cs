using System.Text.Json.Nodes;
using OrchardCore.Menu.Services;

namespace OrchardCore.Tests.Modules.OrchardCore.Menu;

public class MenuItemIdentifiersTests
{
    [Fact]
    public void AssignMissing_MenuItemsWithoutIdentifier_GetAnIdentifier()
    {
        var content = Menu(Item("Home"), Item("About"));
        var ids = new Queue<string>(["id1", "id2"]);

        Assert.True(MenuItemIdentifiers.AssignMissing(content, ids.Dequeue));

        Assert.Equal("id1", IdAt(content, 0));
        Assert.Equal("id2", IdAt(content, 1));
    }

    [Fact]
    public void AssignMissing_ValidIdentifiers_AreKept()
    {
        var content = Menu(Item("Home", "4t69k64e596fk7ectc7wh89d43"), Item("About", "about_page-2"));

        Assert.False(MenuItemIdentifiers.AssignMissing(content, () => "new"));

        Assert.Equal("4t69k64e596fk7ectc7wh89d43", IdAt(content, 0));
        Assert.Equal("about_page-2", IdAt(content, 1));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("[object Object]")]
    [InlineData("[js: uuid()]")]
    [InlineData("an id")]
    public void AssignMissing_IdentifierThatIsNotUsable_IsReplaced(string storedId)
    {
        var item = Item("Home");
        item["ContentItemId"] = storedId;
        var content = Menu(item);

        Assert.True(MenuItemIdentifiers.AssignMissing(content, () => "new"));

        Assert.Equal("new", IdAt(content, 0));
    }

    [Fact]
    public void AssignMissing_IdentifierThatIsAnObject_IsReplaced()
    {
        var item = Item("Home");
        item["ContentItemId"] = new JsonObject();
        var content = Menu(item);

        Assert.True(MenuItemIdentifiers.AssignMissing(content, () => "new"));

        Assert.Equal("new", IdAt(content, 0));
    }

    [Fact]
    public void AssignMissing_DuplicateIdentifier_KeepsTheFirstAndReplacesTheOthers()
    {
        var child = Item("Child", "same");
        var parent = Item("Parent", "same");
        parent["MenuItemsListPart"] = new JsonObject { ["MenuItems"] = new JsonArray(child) };
        var content = Menu(Item("Home", "same"), parent);

        Assert.True(MenuItemIdentifiers.AssignMissing(content, new Queue<string>(["new1", "new2"]).Dequeue));

        Assert.Equal("same", IdAt(content, 0));
        Assert.Equal("new1", IdAt(content, 1));
        Assert.Equal("new2", ((JsonObject)ItemAt(content, 1)["MenuItemsListPart"]["MenuItems"][0])["ContentItemId"].GetValue<string>());
    }

    [Fact]
    public void AssignMissing_NestedMenuItemsWithoutIdentifier_GetAnIdentifier()
    {
        var parent = Item("Parent", "parent");
        parent["MenuItemsListPart"] = new JsonObject { ["MenuItems"] = new JsonArray(Item("Child")) };
        var content = Menu(parent);

        Assert.True(MenuItemIdentifiers.AssignMissing(content, () => "child"));

        Assert.Equal("parent", IdAt(content, 0));
        Assert.Equal("child", ((JsonObject)ItemAt(content, 0)["MenuItemsListPart"]["MenuItems"][0])["ContentItemId"].GetValue<string>());
    }

    [Fact]
    public void AssignMissing_ContentWithoutMenuItems_ChangesNothing()
    {
        Assert.False(MenuItemIdentifiers.AssignMissing([], () => "new"));
        Assert.False(MenuItemIdentifiers.AssignMissing(Menu(), () => "new"));
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

    private static JsonObject ItemAt(JsonObject content, int index)
        => (JsonObject)content["MenuItemsListPart"]["MenuItems"][index];

    private static string IdAt(JsonObject content, int index)
        => ItemAt(content, index)["ContentItemId"].GetValue<string>();
}
