using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Localization;
using OrchardCore.Layers.Drivers;
using OrchardCore.Navigation;

namespace OrchardCore.Layers;

public sealed class AdminMenu : INavigationProvider
{
    private static readonly RouteValueDictionary _routeValues = new()
    {
        { "area", "OrchardCore.Settings" },
        { "groupId", LayerSiteSettingsDisplayDriver.GroupId },
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
            .Add(S["Design"], design => design
                .Add(S["Settings"], settings => settings
                    .Add(S["Zones"], S["Zones"].PrefixPosition(), zones => zones
                        .Action("Index", "Admin", _routeValues)
                        .Permission(Permissions.ManageLayers)
                        .LocalNav()
                    )
                )
                .Add(S["Widgets"], S["Widgets"].PrefixPosition(), widgets => widgets
                    .Permission(Permissions.ManageLayers)
                    .Action("Index", "Admin", "OrchardCore.Layers")
                    .LocalNav()
                )
            );

        return Task.CompletedTask;
    }
}
