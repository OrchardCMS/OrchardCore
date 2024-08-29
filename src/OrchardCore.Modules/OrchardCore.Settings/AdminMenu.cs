using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Localization;
using OrchardCore.Navigation;
using OrchardCore.Settings.Drivers;

namespace OrchardCore.Settings;

public sealed class AdminMenu : INavigationProvider
{
    private static readonly RouteValueDictionary _routeValues = new()
    {
        { "area", "OrchardCore.Settings" },
        { "groupId", DefaultSiteSettingsDisplayDriver.GroupId },
    };

    internal readonly IStringLocalizer S;

    public AdminMenu(IStringLocalizer<AdminMenu> localizer)
    {
        S = localizer;
    }

    public Task BuildNavigationAsync(string name, NavigationBuilder builder)
    {
        if (!NavigationHelper.IsAdminMenu(name))
        {
            return Task.CompletedTask;
        }

        builder
            .Add(S["Configuration"], NavigationConstants.AdminMenuConfigurationPosition, configuration => configuration
                .AddClass("menu-configuration")
                .Id("configuration")
                .Add(S["Settings"], "1", settings => settings
                    .Add(S["General"], "1", entry => entry
                        .AddClass("general")
                        .Id("general")
                        .Action("Index", "Admin", _routeValues)
                        .Permission(Permissions.ManageGroupSettings)
                        .LocalNav()
                    ),
                priority: 1)
            );

        return Task.CompletedTask;
    }
}
