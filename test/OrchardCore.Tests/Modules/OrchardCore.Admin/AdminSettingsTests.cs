using System.Text.Json;
using OrchardCore.Admin.Models;

namespace OrchardCore.Tests.Modules.OrchardCore.Admin;

public class AdminSettingsTests
{
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
