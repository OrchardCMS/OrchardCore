using Microsoft.Extensions.Localization;
using OrchardCore.Navigation;
using OrchardCore.Settings.Drivers;

namespace OrchardCore.Settings;

public sealed class AdminMenu : AdminNavigationProvider
{
    internal readonly IStringLocalizer S;

    public AdminMenu(IStringLocalizer<AdminMenu> stringLocalizer)
    {
        S = stringLocalizer;
    }

    protected override ValueTask BuildAsync(NavigationBuilder builder)
    {
        builder
            .Add(S["Tools"], NavigationConstants.AdminMenuToolsPosition, tools => tools
                .AddClass("menu-tools")
                .Id("tools")
                .Add(S["Settings"], NavigationConstants.AdminMenuSettingsPosition, settings => settings
                    .AddClass("menu-settings")
                    .Id("settings")
                    .Action(SettingsNavigationProvider.GetRouteValues(DefaultSiteSettingsDisplayDriver.GroupId))
                    .LocalNav(),
                    priority: 1
                ),
                priority: 1
            );

        return ValueTask.CompletedTask;
    }
}
