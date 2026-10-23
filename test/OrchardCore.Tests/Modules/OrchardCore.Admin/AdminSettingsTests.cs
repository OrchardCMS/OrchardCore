using System.Text.Json;
using OrchardCore.Admin;
using OrchardCore.Admin.Models;

namespace OrchardCore.Tests.Modules.OrchardCore.Admin;

public class AdminSettingsTests
{
    [Theory]
    [InlineData("Buttons", AdminListActionsLayout.Buttons)]
    [InlineData("Menu", AdminListActionsLayout.Menu)]
    public void Deserialize_ActionsLayout_ReadsEnumByName(string value, AdminListActionsLayout expected)
    {
        var settings = JsonSerializer.Deserialize<AdminSettings>($$"""{ "ActionsLayout": "{{value}}" }""", JOptions.Default);

        Assert.Equal(expected, settings.ActionsLayout);
        var json = JsonSerializer.Serialize(settings, JOptions.Default);
        Assert.Equal(expected, JsonSerializer.Deserialize<AdminSettings>(json, JOptions.Default).ActionsLayout);
    }

    [Fact]
    public void Deserialize_NoActionsChoice_InheritsConfiguration()
    {
        var settings = JsonSerializer.Deserialize<AdminSettings>("{}", JOptions.Default);

        Assert.Null(settings.ActionsLayout);
    }

    // The default of the navigation menu behavior is the first member of its enum, not an
    // attribute or an initializer: reordering the members would silently change it.
    [Fact]
    public void MenuBehavior_NotSet_IsFocused()
    {
        Assert.Equal(AdminMenuBehavior.Focused, new AdminSettings().MenuBehavior);
    }

    [Fact]
    public void Deserialize_SettingsSavedBeforeTheMenuBehaviorExisted_IsFocused()
    {
        var settings = JsonSerializer.Deserialize<AdminSettings>("""{ "DisplayMenuFilter": true }""", JOptions.Default);

        Assert.Equal(AdminMenuBehavior.Focused, settings.MenuBehavior);
    }

    [Theory]
    [InlineData("Focused", AdminMenuBehavior.Focused)]
    [InlineData("Persistent", AdminMenuBehavior.Persistent)]
    public void Deserialize_RecipeValue_ReadsTheBehaviorByName(string value, AdminMenuBehavior expected)
    {
        var settings = JsonSerializer.Deserialize<AdminSettings>($$"""{ "MenuBehavior": "{{value}}" }""", JOptions.Default);

        Assert.Equal(expected, settings.MenuBehavior);
    }
}
