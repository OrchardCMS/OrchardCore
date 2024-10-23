using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Localization;
using OrchardCore.Navigation;
using OrchardCore.Settings.Drivers;

namespace OrchardCore.Settings;

public sealed class AdminMenu : AdminNavigationProvider
{
    internal readonly IStringLocalizer S;

    private static readonly RouteValueDictionary _routeDate = new RouteValueDictionary
    {
        { "action", "Index" },
        { "controller", "Admin" },
        { "area", "OrchardCore.Settings" },
        { "groupId", DefaultSiteSettingsDisplayDriver.GroupId },
    };
    public AdminMenu(IStringLocalizer<AdminMenu> stringLocalizer)
    {
        S = stringLocalizer;
    }

    protected override ValueTask BuildAsync(NavigationBuilder builder)
    {
        builder
            .Add(S["Tools"], NavigationConstants.AdminMenuToolsPosition, tools => tools
                .AddClass("menu-tools")
                .Id("tools"),
                priority: 1
            )
            .Add(S["Settings"], NavigationConstants.AdminMenuSettingsPosition, settings => settings
                .AddClass("menu-settings")
                .Id("settings")
                .Action(_routeDate)
                .LocalNav(),
                priority: 1
            );

        return ValueTask.CompletedTask;
    }
}
